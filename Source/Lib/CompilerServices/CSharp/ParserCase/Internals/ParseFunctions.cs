using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.CompilerServices.CSharp.Facts;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public class ParseFunctions
{
    public static void HandleFunctionDefinition(
        SyntaxToken consumedIdentifierToken,
        TypeReference consumedTypeReference,
        ref CSharpParserModel parserModel)
    {
        var functionDefinitionNode = new FunctionDefinitionNode(
            AccessModifierKind.Public,
            consumedTypeReference,
            consumedIdentifierToken,
            openAngleBracketToken: default,
    		indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
    		closeAngleBracketToken: default,
            openParenthesisToken: default,
            functionArgumentEntryList: null,
            closeParenthesisToken: default,
            default,
            parserModel.Compilation.ResourceUri);
            
        parserModel.BindFunctionDefinitionNode(functionDefinitionNode);
        
        bool isFunctionOverloadCase;
        
        if (parserModel.TryGetFunctionDefinitionNodeByScope(
            parserModel.Compilation,
            parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
            parserModel.GetTextSpanText(consumedIdentifierToken.TextSpan),
            out var existingFunctionDefinitionNode))
        {
            isFunctionOverloadCase = true;
        }
        else
        {
            isFunctionOverloadCase = false;
        }
        
        parserModel.NewScopeAndBuilderFromOwner(
            functionDefinitionNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
        {
            parserModel.ForceParseExpressionInitialPrimaryExpression = functionDefinitionNode;

            var openAngleBracketToken = parserModel.TokenWalker.Consume();
            
            ParseExpressions.ParseGenericParameterNode_Start(
                functionDefinitionNode, ref openAngleBracketToken, ref parserModel);
            
            parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.FunctionDefinitionNode);
            var successGenericParametersListingNode = ParseExpressions.TryParseExpression(
                ref parserModel,
                out var expressionNode);
            
            if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.CloseAngleBracketToken &&
                parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.CloseAngleBracketToken)
            {
                _ = parserModel.TokenWalker.Consume();
                _ = parserModel.TokenWalker.Consume();
            }
        }
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenParenthesisToken)
            return;

        HandleFunctionArguments(functionDefinitionNode, ref parserModel);
        
        if (isFunctionOverloadCase)
        {
            HandleFunctionOverloadDefinition(
                newNode: functionDefinitionNode,
                existingNode: existingFunctionDefinitionNode,
                ref parserModel);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
        else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
            ParseTokens.MoveToExpressionBody(ref parserModel);
    }
    
    public static void HandleFunctionOverloadDefinition(
        FunctionDefinitionNode newNode,
        FunctionDefinitionNode existingNode,
        ref CSharpParserModel parserModel)
    {
        if (!parserModel.Binder.MethodOverload_ResourceUri_WasCleared)
        {
            parserModel.Binder.MethodOverload_ResourceUri_WasCleared = true;
            for (int clearIndex = 0; clearIndex < parserModel.Binder.MethodOverloadDefinitionList.Count; clearIndex++)
            {
                var entry = parserModel.Binder.MethodOverloadDefinitionList[clearIndex];
                if (entry.ResourceUri == parserModel.Compilation.ResourceUri)
                {
                    entry.ScopeIndexKey = -1;
                    parserModel.Binder.MethodOverloadDefinitionList[clearIndex] = entry;
                }
            }
        }
    
        // This scope should only set the 'existingNode' lest a grand performance improvement be found.
        // Otherwise things get quite confusing.
        if (existingNode.IndexMethodOverloadDefinition == -1)
        {
            // TODO: handle a partial type which contains an existing overload
            // TODO: handle partial type definition that only has partial definitions in the same file by using 'TryGetCompilationUnit_Previous(...)'.
            
            var existingWasFound = false;
        
            if (parserModel.Binder.__CompilationUnitMap.TryGetValue(parserModel.Compilation.ResourceUri, out var previousCompilationUnit))
            {
                existingWasFound = false;
                
                if (existingNode.Unsafe_ParentIndexKey < previousCompilationUnit.CodeBlockOwnerList.Count)
                {
                    var previousParent = previousCompilationUnit.CodeBlockOwnerList[existingNode.Unsafe_ParentIndexKey];
                    var currentParent = parserModel.GetParent(newNode, parserModel.Compilation);
                    
                    if (currentParent.SyntaxKind == previousParent.SyntaxKind &&
                        parserModel.Binder.GetIdentifierText(currentParent, parserModel.Compilation) == parserModel.Binder.GetIdentifierText(previousParent, previousCompilationUnit))
                    {
                        // All the existing entires will be "emptied"
                        // so don't both with checking whether the arguments are the same here.
                        //
                        // All that matters is that they're put in the same "method group".
                        //
                        var binder = parserModel.Binder;
                        
                        // TODO: Cannot use ref, out, or in...
                        var compilation = parserModel.Compilation;
                        
                        var previousNode = previousCompilationUnit.CodeBlockOwnerList.FirstOrDefault(x =>
                            x.Unsafe_ParentIndexKey == previousParent.Unsafe_SelfIndexKey &&
                            x.SyntaxKind == SyntaxKind.FunctionDefinitionNode &&
                            binder.GetIdentifierText(x, previousCompilationUnit) == binder.GetIdentifierText(existingNode, compilation));
                    
                        if (previousNode is not null)
                        {
                            var previousFunctionDefinitionNode = (FunctionDefinitionNode)previousNode;
                            existingNode.IndexMethodOverloadDefinition = previousFunctionDefinitionNode.IndexMethodOverloadDefinition;
                            
                            if (existingNode.IndexMethodOverloadDefinition != -1)
                            {
                                existingWasFound = true;
                                
                                var entry = parserModel.Binder.MethodOverloadDefinitionList[existingNode.IndexMethodOverloadDefinition];
                                entry.ScopeIndexKey = existingNode.Unsafe_SelfIndexKey;
                                parserModel.Binder.MethodOverloadDefinitionList[existingNode.IndexMethodOverloadDefinition] = entry;
                            }
                        }
                    }
                }
            }
            
            if (!existingWasFound)
            {
                existingNode.IndexMethodOverloadDefinition = parserModel.Binder.MethodOverloadDefinitionList.Count;
                parserModel.Binder.MethodOverloadDefinitionList.Add(new MethodOverloadDefinitionEntry(
                    parserModel.Compilation.ResourceUri,
                    parserModel.Binder.MethodOverloadDefinitionList.Count,
                    existingNode.Unsafe_SelfIndexKey));
            }
        }
        
        var usedExistingSlot = false;
        var i = existingNode.IndexMethodOverloadDefinition + 1;
        
        for (; i < parserModel.Binder.MethodOverloadDefinitionList.Count; i++)
        {
            var entry = parserModel.Binder.MethodOverloadDefinitionList[i];
            if (entry.IndexStartGroup == existingNode.IndexMethodOverloadDefinition && entry.ScopeIndexKey == -1)
            {
                usedExistingSlot = true;
                parserModel.Binder.MethodOverloadDefinitionList[i] = new MethodOverloadDefinitionEntry(
                    parserModel.Compilation.ResourceUri,
                    existingNode.IndexMethodOverloadDefinition,
                    newNode.Unsafe_SelfIndexKey);
            }
        }
        
        if (!usedExistingSlot)
        {
            parserModel.Binder.MethodOverloadDefinitionList.Insert(
                i,
                new MethodOverloadDefinitionEntry(
                    parserModel.Compilation.ResourceUri,
                    existingNode.IndexMethodOverloadDefinition,
                    newNode.Unsafe_SelfIndexKey));
        }
        
        /*Console.WriteLine();
        Console.WriteLine("========");
        
        foreach (var entry in parserModel.Binder.MethodOverloadDefinitionList)
        {
            Console.Write($"isg:{entry.IndexStartGroup}");
            Console.Write(", ");
            Console.Write($"sik:{entry.ScopeIndexKey}");
            Console.Write(", ");
            Console.Write($"ruv:{entry.ResourceUri.Value}");
            Console.Write(",");
            Console.WriteLine();
        }
        
        Console.WriteLine("========");
        Console.WriteLine();*/
    }

    public static void HandleConstructorDefinition(
        TypeDefinitionNode typeDefinitionNodeCodeBlockOwner,
        SyntaxToken consumedIdentifierToken,
        ref CSharpParserModel parserModel)
    {
        var typeClauseNode = parserModel.ConstructOrRecycleTypeClauseNode(
            typeDefinitionNodeCodeBlockOwner.TypeIdentifierToken,
            openAngleBracketToken: default,
    		indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
    		closeAngleBracketToken: default,
            isKeywordType: false);

        var constructorDefinitionNode = new ConstructorDefinitionNode(
            new TypeReference(typeClauseNode),
            consumedIdentifierToken,
            openAngleBracketToken: default,
    		indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
    		closeAngleBracketToken: default,
            openParenthesisToken: default,
            functionArgumentEntryList: null,
            closeParenthesisToken: default,
            default,
            parserModel.Compilation.ResourceUri);
    
        parserModel.BindConstructorDefinitionIdentifierToken(consumedIdentifierToken);
        
        parserModel.NewScopeAndBuilderFromOwner(
            constructorDefinitionNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        HandleFunctionArguments(constructorDefinitionNode, ref parserModel);

        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ColonToken)
        {
            _ = parserModel.TokenWalker.Consume();
            // Constructor invokes some other constructor as well
            // 'this(...)' or 'base(...)'
            
            SyntaxToken keywordToken;
            
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ThisTokenKeyword)
                keywordToken = parserModel.TokenWalker.Match(SyntaxKind.ThisTokenKeyword);
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.BaseTokenKeyword)
                keywordToken = parserModel.TokenWalker.Match(SyntaxKind.BaseTokenKeyword);
            else
                keywordToken = default;
            
            while (!parserModel.TokenWalker.IsEof)
            {
                // "short circuit"
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseAngleBracketEqualsToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
                {
                    break;
                }
                
                // Good case
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
                {
                    break;
                }

                _ = parserModel.TokenWalker.Consume();
            }
            
            // Parse secondary syntax ': base(myVariable, 7)'
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
            {
                var openParenthesisToken = parserModel.TokenWalker.Current;
            
                var functionInvocationNode = new FunctionInvocationNode(
                    consumedIdentifierToken,
                    openAngleBracketToken: default,
            		indexGenericParameterEntryList: -1,
                    countGenericParameterEntryList: 0,
            		closeAngleBracketToken: default,
                    openParenthesisToken,
                    indexFunctionParameterEntryList: parserModel.Binder.FunctionParameterEntryList.Count,
                    countFunctionParameterEntryList: 0,
                    closeParenthesisToken: default,
                    CSharpFacts.Types.Void.ToTypeReference());
                    
                functionInvocationNode.IsParsingFunctionParameters = true;
                    
                parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, functionInvocationNode));
                parserModel.ExpressionList.Add((SyntaxKind.ColonToken, functionInvocationNode));
                
                // TODO: The 'ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(...)' code needs to be invoked...
                // ...from within the expression loop.
                // But, as of this comment a way to do so does not exist.
                //
                // Therefore, if the secondary constructor invocation were ': base(person: new Person())'
                // then the first named parameter would not parse correctly.
                //
                // If the second or onwards parameters were named they would be parsed correctly.
                //
                // So, explicitly adding this invocation so that the first named parameter parses correctly.
                //
                _ = ParseExpressions.ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(ref parserModel, guaranteeConsume: true);
                
                // This invocation will parse all of the parameters because the 'parserModel.ExpressionList'
                // contains (SyntaxKind.CommaToken, functionParametersListingNode).
                //
                // Upon encountering a CommaToken the expression loop will set 'functionParametersListingNode'
                // to the primary expression, then return an EmptyExpressionNode in order to parse the next parameter.
                _ = ParseExpressions.ParseExpression(ref parserModel);
            }
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            ParseTokens.MoveToExpressionBody(ref parserModel);
        }
    }

    /// <summary>Use this method for function definition, whereas <see cref="HandleFunctionParameters"/> should be used for function invocation.</summary>
    public static void HandleFunctionArguments(
        IFunctionDefinitionNode functionDefinitionNode,
        ref CSharpParserModel parserModel,
        VariableKind variableKind = VariableKind.Local)
    {
        var openParenthesisToken = parserModel.TokenWalker.Consume();
        var functionArgumentEntryList = new List<FunctionArgumentEntry>();
        var openParenthesisCount = 1;
        var corruptState = false;
        
        while (!parserModel.TokenWalker.IsEof)
        {
            if (corruptState)
            {
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
                    openParenthesisCount++;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
            {
                openParenthesisCount--;
                
                if (openParenthesisCount == 0)
                {
                    break;
                }
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
            {
                break;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
            {
                break;
            }
            else if (!corruptState)
            {
                parserModel.ArgumentModifierKind = ArgumentModifierKind.None;
                
                checkModifier:
                switch (parserModel.TokenWalker.Current.SyntaxKind)
                {
                    case SyntaxKind.OutTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        parserModel.ArgumentModifierKind = ArgumentModifierKind.Out;
                        break;
                    case SyntaxKind.InTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        parserModel.ArgumentModifierKind = ArgumentModifierKind.In;
                        break;
                    case SyntaxKind.RefTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        if (parserModel.ArgumentModifierKind == ArgumentModifierKind.This)
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.ThisRef;
                            break;
                        }
                        else if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Readonly)
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.ReadonlyRef;
                            break;
                        }
                        else
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.Ref;
                            goto checkModifier;
                        }
                    case SyntaxKind.ThisTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Ref)
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.RefThis;
                            break;
                        }
                        else
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.This;
                            goto checkModifier;
                        }
                    case SyntaxKind.ReadonlyTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Ref)
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.RefReadonly;
                            break;
                        }
                        else
                        {
                            parserModel.ArgumentModifierKind = ArgumentModifierKind.Readonly;
                            goto checkModifier;
                        }
                    case SyntaxKind.ParamsTokenKeyword:
                        _ = parserModel.TokenWalker.Consume();
                        parserModel.ArgumentModifierKind = ArgumentModifierKind.Params;
                        break;
                }
            
                var tokenIndexOriginal = parserModel.TokenWalker.Index;
                
                var successParse = ParseExpressions.TryParseVariableDeclarationNode(ref parserModel, out var variableDeclarationNode);
                
                if (successParse)
                {
                    parserModel.CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind);
                    variableDeclarationNode.VariableKind = variableKind;
                    parserModel.BindVariableDeclarationNode(variableDeclarationNode, shouldCreateVariableSymbol: false);
                    
                    SyntaxToken optionalCompileTimeConstantToken;
                    
                    if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsToken)
                    {
                        _ = parserModel.TokenWalker.Consume();
                        optionalCompileTimeConstantToken = parserModel.TokenWalker.Current;
                        
                        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
                        parserModel.ExpressionList.Add((SyntaxKind.CommaToken, null));
                        _ = ParseExpressions.ParseExpression(ref parserModel);
                    }
                    else
                    {
                        optionalCompileTimeConstantToken = new SyntaxToken(SyntaxKind.NotApplicable, textSpan: default);
                    }
                    
                    functionArgumentEntryList.Add(
                        new FunctionArgumentEntry(
                            variableDeclarationNode,
                            optionalCompileTimeConstantToken,
                            parserModel.ArgumentModifierKind));
                    
                    if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
                        _ = parserModel.TokenWalker.Consume();
                }
                else
                {
                    variableDeclarationNode = null;
                    corruptState = true;
                }
                
                if (tokenIndexOriginal < parserModel.TokenWalker.Index)
                    continue; // Already consumed so avoid the one at the end of the while loop
            }

            _ = parserModel.TokenWalker.Consume();
        }
        
        var closeParenthesisToken = default(SyntaxToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
            closeParenthesisToken = parserModel.TokenWalker.Consume();
        
        functionDefinitionNode.OpenParenthesisToken = openParenthesisToken;
        functionDefinitionNode.FunctionArgumentEntryList = functionArgumentEntryList;
        functionDefinitionNode.CloseParenthesisToken = closeParenthesisToken;
    }
}
