using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.Facts;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseTokens
{
    public static void ParseIdentifierToken(ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Current.TextSpan.Length == 1 &&
            // 95 is ASCII code for '_'
            parserModel.TokenWalker.Current.TextSpan.CharIntSum == 95)
        {
            if (!parserModel.TryGetVariableDeclarationHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    parserModel.TokenWalker.Current.TextSpan,
                    out _))
            {
                parserModel.BindDiscard(parserModel.TokenWalker.Current);
                var identifierToken = parserModel.TokenWalker.Consume();
                
                var variableReferenceNode = parserModel.Rent_VariableReferenceNode();
                variableReferenceNode.VariableIdentifierToken = identifierToken;
                    
                parserModel.StatementBuilder.MostRecentNode = variableReferenceNode;
                return;
            }
        }
        
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.ColonToken)
        {
            ParseOthers.HandleLabelDeclaration(ref parserModel);
            return;
        }
        
        var originalTokenIndex = parserModel.TokenWalker.Index;
        
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.TypeClauseNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.VariableDeclarationNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.VariableReferenceNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.ConstructorInvocationExpressionNode);
        
        if (parserModel.CurrentCodeBlockOwner.SyntaxKind != SyntaxKind.TypeDefinitionNode)
        {
            // There is a syntax conflict between a ConstructorDefinitionNode and a FunctionInvocationNode.
            //
            // Disambiguation is done based on the 'CodeBlockOwner' until a better solution is found.
            //
            // If the supposed "ConstructorDefinitionNode" does not have the same name as
            // the CodeBlockOwner.
            //
            // Then, it perhaps should be treated as a function invocation (or function definition).
            // The main case for this being someone typing out pseudo code within a CodeBlockOwner
            // that is a TypeDefinitionNode.
            parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.FunctionInvocationNode);
        }
        
        parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
        
        var successParse = ParseExpressions.TryParseExpression(ref parserModel, out var expressionNode);
        
        if (!successParse)
        {
            expressionNode = ParseExpressions.ParseExpression(ref parserModel);
            parserModel.StatementBuilder.MostRecentNode = expressionNode;
            return;
        }
        
        switch (expressionNode.SyntaxKind)
        {
            case SyntaxKind.TypeClauseNode:
                MoveToHandleTypeClauseNode(originalTokenIndex, (TypeClauseNode)expressionNode, ref parserModel);
                return;
            case SyntaxKind.VariableDeclarationNode:
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
                {
                    MoveToHandleFunctionDefinition((VariableDeclarationNode)expressionNode, ref parserModel);
                    return;
                }
                
                MoveToHandleVariableDeclarationNode((VariableDeclarationNode)expressionNode, ref parserModel);
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
                {
                    HandleMultiVariableDeclaration((VariableDeclarationNode)expressionNode, ref parserModel);
                }
                return;
            case SyntaxKind.VariableReferenceNode:
            
                var isQuestionMarkMemberAccessToken = parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.QuestionMarkToken &&
                    parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken;
                
                var isBangMemberAccessToken = parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.BangToken &&
                    parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken;
            
                if ((parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.MemberAccessToken || isQuestionMarkMemberAccessToken || isBangMemberAccessToken) &&
                    originalTokenIndex == parserModel.TokenWalker.Index - 1)
                {
                    parserModel.TokenWalker.BacktrackNoReturnValue();
                    expressionNode = ParseExpressions.ParseExpression(ref parserModel);
                    parserModel.StatementBuilder.MostRecentNode = expressionNode;
                    return;
                }
                
                parserModel.StatementBuilder.MostRecentNode = expressionNode;
                return;
            case SyntaxKind.FunctionInvocationNode:
            case SyntaxKind.ConstructorInvocationExpressionNode:
                parserModel.StatementBuilder.MostRecentNode = expressionNode;
                return;
            default:
                // compilationUnit.DiagnosticBag.ReportTodoException(parserModel.TokenWalker.Current.TextSpan, $"nameof(ParseIdentifierToken) default case");
                return;
        }
    }
    
    public static void MoveToHandleFunctionDefinition(VariableDeclarationNode variableDeclarationNode, ref CSharpParserModel parserModel)
    {
        ParseFunctions.HandleFunctionDefinition(
            variableDeclarationNode.IdentifierToken,
            variableDeclarationNode.TypeReference,
            ref parserModel);
    }
    
    public static void MoveToHandleVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode, ref CSharpParserModel parserModel)
    {
        var variableKind = VariableKind.Local;
                
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            variableKind = VariableKind.Property;
        }
        else if (parserModel.CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
        {
            variableKind = VariableKind.Field;
        }
        
        ((VariableDeclarationNode)variableDeclarationNode).VariableKind = variableKind;
        
        parserModel.BindVariableDeclarationNode(variableDeclarationNode);
        // parserModel.CurrentCodeBlockBuilder.AddChild(variableDeclarationNode);
        parserModel.StatementBuilder.MostRecentNode = variableDeclarationNode;
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            ParsePropertyDefinition_ExpressionBound(ref parserModel);
        }
        else
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
                ParsePropertyDefinition(variableDeclarationNode, ref parserModel);
            
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsToken)
            {
                parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode = variableDeclarationNode.TypeReference;
            
                IExpressionNode expression;
            
                parserModel.ForceParseExpressionInitialPrimaryExpression = variableDeclarationNode;
                if (variableKind == VariableKind.Local)
                {
                    parserModel.ExpressionList.Add((SyntaxKind.CommaToken, null));
                }
                expression = ParseExpressions.ParseExpression(ref parserModel);
                parserModel.ForceParseExpressionInitialPrimaryExpression = EmptyExpressionNode.Empty;
                
                if (expression.SyntaxKind == SyntaxKind.BinaryExpressionNode &&
                    parserModel.Binder.CSharpCompilerService.SafeCompareText(parserModel.ResourceUri.Value, "var", variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan))
                {
                    var binaryExpressionNode = (BinaryExpressionNode)expression;
                    variableDeclarationNode.SetImplicitTypeReference(binaryExpressionNode.RightExpressionResultTypeReference);
                }
                
                parserModel.StatementBuilder.MostRecentNode = expression;
            }
        }
    }
    
    public static void HandleMultiVariableDeclaration(VariableDeclarationNode variableDeclarationNode, ref CSharpParserModel parserModel)
    {
        var previousTokenIndex = parserModel.TokenWalker.Index;
        
        while (!parserModel.TokenWalker.IsEof)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
            parserModel.ExpressionList.Add((SyntaxKind.CommaToken, null));
            
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsToken)
            {
                ParseTokens.MoveToHandleVariableDeclarationNode(variableDeclarationNode, ref parserModel);
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
            {
                parserModel.BindVariableDeclarationNode(variableDeclarationNode);
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
            {
                parserModel.BindVariableDeclarationNode(variableDeclarationNode);
                return;
            }
            
            if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.CommaToken)
            {
                break;
            }
            
            _ = parserModel.TokenWalker.Consume(); // Comma Token
            
            if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind))
            {
                var token = parserModel.TokenWalker.Consume();
                var identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
            
                variableDeclarationNode = new VariableDeclarationNode(
                    variableDeclarationNode.TypeReference,
                    identifierToken,
                    VariableKind.Local,
                    isInitialized: false,
                    resourceUri: parserModel.ResourceUri);
            }
            else
            {
                return;
            }
            
            if (previousTokenIndex == parserModel.TokenWalker.Index)
            {
                break;
            }
            
            previousTokenIndex = parserModel.TokenWalker.Index;
        }
    }
    
    public static void MoveToHandleTypeClauseNode(int originalTokenIndex, TypeClauseNode typeClauseNode, ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EndOfFileToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
        {
            parserModel.StatementBuilder.MostRecentNode = typeClauseNode;
        }
        else if (parserModel.CurrentCodeBlockOwner is TypeDefinitionNode typeDefinitionNode &&
                 UtilityApi.IsConvertibleToIdentifierToken(typeClauseNode.TypeIdentifierToken.SyntaxKind) &&
                 parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken &&
                 parserModel.GetTextSpanText(typeDefinitionNode.TypeIdentifierToken.TextSpan) == parserModel.GetTextSpanText(typeClauseNode.TypeIdentifierToken.TextSpan))
        {
            // ConstructorDefinitionNode
            
            var typeClauseToken = typeClauseNode.TypeIdentifierToken;
            var identifierToken = UtilityApi.ConvertToIdentifierToken(ref typeClauseToken, ref parserModel);
            
            ParseFunctions.HandleConstructorDefinition(
                typeDefinitionNode,
                identifierToken,
                ref parserModel);
        }
        else
        {
            parserModel.StatementBuilder.MostRecentNode = typeClauseNode;
        }
        
        return;
    }
    
    public static void ParsePropertyDefinition(VariableDeclarationNode variableDeclarationNode, ref CSharpParserModel parserModel)
    {
        var openBraceToken = parserModel.TokenWalker.Consume();
        
        var openBraceCounter = 1;
        
        bool consumed;
        
        while (true)
        {
            consumed = false;
        
            if (parserModel.TokenWalker.IsEof)
                break;

            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
            {
                ++openBraceCounter;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
            {
                if (--openBraceCounter <= 0)
                    break;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.GetTokenContextualKeyword)
            {
                variableDeclarationNode.HasGetter = true;
                
                if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.StatementDelimiterToken)
                {
                    consumed = true;
                    ParseGetterOrSetter(variableDeclarationNode, ref parserModel);
                }
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.SetTokenContextualKeyword)
            {
                variableDeclarationNode.HasSetter = true;
                
                if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.StatementDelimiterToken)
                {
                    consumed = true;
                    ParseGetterOrSetter(variableDeclarationNode, ref parserModel);
                }
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.InitTokenContextualKeyword)
            {
                variableDeclarationNode.HasSetter = true;
                
                if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.StatementDelimiterToken)
                {
                    consumed = true;
                    ParseGetterOrSetter(variableDeclarationNode, ref parserModel);
                }
            }

            if (!consumed)
                _ = parserModel.TokenWalker.Consume();
        }

        var closeTokenIndex = parserModel.TokenWalker.Index;
        var closeBraceToken = parserModel.TokenWalker.Match(SyntaxKind.CloseBraceToken);
    }
    
    /// <summary>
    /// This method must consume at least once or an infinite loop in 'ParsePropertyDefinition(...)'
    /// will occur due to the 'bool consumed' variable.
    /// </summary>
    public static void ParseGetterOrSetter(VariableDeclarationNode variableDeclarationNode, ref CSharpParserModel parserModel)
    {
        parserModel.TokenWalker.Consume(); // Consume the 'get' or 'set' contextual keyword.
    
        var getterOrSetterNode = new GetterOrSetterNode();
    
        parserModel.NewScopeAndBuilderFromOwner(
            getterOrSetterNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
        {
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
        }
        else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            ParseTokens.MoveToExpressionBody(ref parserModel);
        }
        else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
        {
            //var deferredParsingOccurred = parserModel.StatementBuilder.FinishStatement(parserModel.TokenWalker.Index, compilationUnit, ref parserModel);
            //if (deferredParsingOccurred)
            //    break;
            
            ParseTokens.ParseOpenBraceToken(ref parserModel);
        }
    }
    
    public static void ParsePropertyDefinition_ExpressionBound(ref CSharpParserModel parserModel)
    {
        parserModel.TokenWalker.BacktrackNoReturnValue();
        ParseGetterOrSetter(variableDeclarationNode: null, ref parserModel);
    }

    public static void ParseOpenBraceToken(ref CSharpParserModel parserModel)
    {
        var openBraceToken = parserModel.TokenWalker.Consume();
        
        if (parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan ||
            parserModel.CurrentCodeBlockOwner.CodeBlock_StartInclusiveIndex != -1)
        {
            var arbitraryCodeBlockNode = new ArbitraryCodeBlockNode(parserModel.CurrentCodeBlockOwner);
            
            parserModel.NewScopeAndBuilderFromOwner(
                arbitraryCodeBlockNode,
                openBraceToken.TextSpan);
        }
        
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = false;

        // Global scope has a null parent.
        var parentScopeDirection = parserModel.GetParent(parserModel.CurrentCodeBlockOwner, parserModel.Compilation)?.ScopeDirectionKind ?? ScopeDirectionKind.Both;
        
        if (parentScopeDirection == ScopeDirectionKind.Both)
        {
            if (!parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing)
            {
                parserModel.TokenWalker.DeferParsingOfChildScope(ref parserModel);
                return;
            }

            parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing = false;
        }
        
        // This has to come after the 'DeferParsingOfChildScope(...)'
        // or it makes an ArbitraryCodeBlockNode when it comes back around.
        parserModel.CurrentCodeBlockOwner.CodeBlock_StartInclusiveIndex = openBraceToken.TextSpan.StartInclusiveIndex;
    }

    /// <summary>
    /// CloseBraceToken is passed in to the method because it is a protected token,
    /// and is preferably consumed from the main loop so it can be more easily tracked.
    /// </summary>
    public static void ParseCloseBraceToken(int closeBraceTokenIndex, ref CSharpParserModel parserModel)
    {
        var closeBraceToken = parserModel.TokenWalker.Consume();
    
        // while () if not CloseBraceToken accepting bubble up until someone takes it or null parent.
        
        /*if (parserModel.CurrentCodeBlockBuilder.IsImplicitOpenCodeBlockTextSpan)
        {
            throw new NotImplementedException("ParseCloseBraceToken(...) -> if (parserModel.CurrentCodeBlockBuilder.IsImplicitOpenCodeBlockTextSpan)");
        }*/
    
        if (parserModel.ParseChildScopeStack.Count > 0)
        {
            var tuple = parserModel.ParseChildScopeStack.Peek();
            
            if (Object.ReferenceEquals(tuple.CodeBlockOwner, parserModel.CurrentCodeBlockOwner))
            {
                tuple = parserModel.ParseChildScopeStack.Pop();
                tuple.DeferredChildScope.PrepareMainParserLoop(closeBraceTokenIndex, ref parserModel);
                return;
            }
        }

        if (parserModel.CurrentCodeBlockOwner.SyntaxKind != SyntaxKind.GlobalCodeBlockNode)
        {
            parserModel.CurrentCodeBlockOwner.CodeBlock_EndExclusiveIndex = closeBraceToken.TextSpan.EndExclusiveIndex;
        }
        
        parserModel.CloseScope(closeBraceToken.TextSpan);
    }

    public static void ParseOpenParenthesisToken(ref CSharpParserModel parserModel)
    {
        var originalTokenIndex = parserModel.TokenWalker.Index;
        
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.VariableDeclarationNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.TypeClauseNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.AmbiguousParenthesizedExpressionNode);
        
        parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
        
        var successParse = ParseExpressions.TryParseExpression(ref parserModel, out var expressionNode);
        
        if (!successParse)
        {
            expressionNode = ParseExpressions.ParseExpression(ref parserModel);
            parserModel.StatementBuilder.MostRecentNode = expressionNode;
            return;
        }
        
        if (expressionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken ||
                parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
            {
                MoveToHandleFunctionDefinition((VariableDeclarationNode)expressionNode, ref parserModel);
                return;
            }
            
            MoveToHandleVariableDeclarationNode((VariableDeclarationNode)expressionNode, ref parserModel);
        }
        
        //// I am catching the next two but not doing anything with them
        //// only so that the TryParseExpression won't return early due to those being the
        //// SyntaxKind(s) that will appear during the process of parsing the VariableDeclarationNode
        //// given that the TypeClauseNode is a tuple.
        //
        // else if (expressionNode.SyntaxKind == SyntaxKind.TypeClauseNode)
        // else if (expressionNode.SyntaxKind == SyntaxKind.AmbiguousParenthesizedExpressionNode)
    }

    public static void ParseOpenSquareBracketToken(ref CSharpParserModel parserModel)
    {
        var openSquareBracketToken = parserModel.TokenWalker.Consume();
    
        if (!parserModel.StatementBuilder.StatementIsEmpty)
        {
            /*compilationUnit.DiagnosticBag.ReportTodoException(
                openSquareBracketToken.TextSpan,
                $"Unexpected '{nameof(SyntaxKind.OpenSquareBracketToken)}'");*/
            return;
        }
        var openSquareBracketCounter = 1;
        var corruptState = false;
        
        while (!parserModel.TokenWalker.IsEof)
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenSquareBracketToken)
            {
                ++openSquareBracketCounter;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseSquareBracketToken)
            {
                if (--openSquareBracketCounter <= 0)
                    break;
            }
            else if (!corruptState)
            {
                var tokenIndexOriginal = parserModel.TokenWalker.Index;
                
                parserModel.ExpressionList.Add((SyntaxKind.CloseSquareBracketToken, null));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, null));
                var expression = ParseExpressions.ParseExpression(ref parserModel);
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
                    _ = parserModel.TokenWalker.Consume();
                    
                if (tokenIndexOriginal < parserModel.TokenWalker.Index)
                    continue; // Already consumed so avoid the one at the end of the while loop
            }

            _ = parserModel.TokenWalker.Consume();
        }

        var closeTokenIndex = parserModel.TokenWalker.Index;
        var closeSquareBracketToken = parserModel.TokenWalker.Match(SyntaxKind.CloseSquareBracketToken);
    }

    public static void ParseEqualsToken(ref CSharpParserModel parserModel)
    {
        var shouldBacktrack = false;
        IExpressionNode backtrackNode = EmptyExpressionNode.Empty;
        
        // No, this is not missing an else
        if (parserModel.StatementBuilder.MostRecentNode != EmptyExpressionNode.Empty)
        {
            var previousNode = parserModel.StatementBuilder.MostRecentNode;
            
            if (previousNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                shouldBacktrack = true;
                parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode = ((VariableReferenceNode)previousNode).ResultTypeReference;
                backtrackNode = (VariableReferenceNode)previousNode;
            }
            else if (previousNode.SyntaxKind == SyntaxKind.TypeClauseNode)
            {
                shouldBacktrack = true;
                parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode = new TypeReference((TypeClauseNode)previousNode);
                parserModel.Return_TypeClauseNode((TypeClauseNode)previousNode);
                backtrackNode = (TypeClauseNode)previousNode;
            }
            else
            {
                parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode = CSharpFacts.Types.Void.ToTypeReference();
            }
        }
        
        if (shouldBacktrack)
        {
            parserModel.ForceParseExpressionInitialPrimaryExpression = backtrackNode;
        }
        _ = ParseExpressions.ParseExpression(ref parserModel);
        parserModel.ForceParseExpressionInitialPrimaryExpression = EmptyExpressionNode.Empty;
    }

    /// <summary>
    /// StatementDelimiterToken is passed in to the method because it is a protected token,
    /// and is preferably consumed from the main loop so it can be more easily tracked.
    /// </summary>
    public static void ParseStatementDelimiterToken(ref CSharpParserModel parserModel)
    {
        var statementDelimiterToken = parserModel.TokenWalker.Consume();
    
        if (parserModel.CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.NamespaceStatementNode)
        {
            var namespaceStatementNode = (NamespaceStatementNode)parserModel.CurrentCodeBlockOwner;
            
            ICodeBlockOwner nextCodeBlockOwner = namespaceStatementNode;
            TypeClauseNode? scopeReturnTypeClauseNode = null;
            
            namespaceStatementNode.CodeBlock_EndExclusiveIndex = statementDelimiterToken.TextSpan.EndExclusiveIndex;

            parserModel.AddNamespaceToCurrentScope(
                parserModel.GetTextSpanText(namespaceStatementNode.IdentifierToken.TextSpan));
        }
        else 
        {
            while (parserModel.CurrentCodeBlockOwner.SyntaxKind != SyntaxKind.GlobalCodeBlockNode &&
                   parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan)
            {
                parserModel.CurrentCodeBlockOwner.CodeBlock_EndExclusiveIndex = statementDelimiterToken.TextSpan.EndExclusiveIndex;
                parserModel.CloseScope(statementDelimiterToken.TextSpan);
            }
        }
    }
    
    public static void MoveToExpressionBody(ref CSharpParserModel parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // Consume 'EqualsCloseAngleBracketToken'
    
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
        
        // Global scope has a null parent.
        var parentScopeDirection = parserModel.GetParent(parserModel.CurrentCodeBlockOwner, parserModel.Compilation)?.ScopeDirectionKind ?? ScopeDirectionKind.Both;
        
        if (parentScopeDirection == ScopeDirectionKind.Both)
        {
            if (!parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing)
            {
                parserModel.TokenWalker.DeferParsingOfChildScope(ref parserModel);
                return;
            }

            parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing = false;
        }
        else
        {
            var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
            // parserModel.CurrentCodeBlockBuilder.AddChild(expressionNode);
        }
    }

    public static void ParseKeywordToken(ref CSharpParserModel parserModel)
    {
        // 'return', 'if', 'get', etc...
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.AsTokenKeyword:
                ParseDefaultKeywords.HandleAsTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.BaseTokenKeyword:
                ParseDefaultKeywords.HandleBaseTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.BoolTokenKeyword:
                ParseDefaultKeywords.HandleBoolTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.BreakTokenKeyword:
                ParseDefaultKeywords.HandleBreakTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ByteTokenKeyword:
                ParseDefaultKeywords.HandleByteTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.CaseTokenKeyword:
                ParseDefaultKeywords.HandleCaseTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.CatchTokenKeyword:
                ParseDefaultKeywords.HandleCatchTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.CharTokenKeyword:
                ParseDefaultKeywords.HandleCharTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.CheckedTokenKeyword:
                ParseDefaultKeywords.HandleCheckedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ConstTokenKeyword:
                ParseDefaultKeywords.HandleConstTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ContinueTokenKeyword:
                ParseDefaultKeywords.HandleContinueTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.DecimalTokenKeyword:
                ParseDefaultKeywords.HandleDecimalTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.DefaultTokenKeyword:
                ParseDefaultKeywords.HandleDefaultTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.DelegateTokenKeyword:
                ParseDefaultKeywords.HandleDelegateTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.DoTokenKeyword:
                ParseDefaultKeywords.HandleDoTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.DoubleTokenKeyword:
                ParseDefaultKeywords.HandleDoubleTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ElseTokenKeyword:
                ParseDefaultKeywords.HandleElseTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.EnumTokenKeyword:
                ParseDefaultKeywords.HandleEnumTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.EventTokenKeyword:
                ParseDefaultKeywords.HandleEventTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ExplicitTokenKeyword:
                ParseDefaultKeywords.HandleExplicitTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ExternTokenKeyword:
                ParseDefaultKeywords.HandleExternTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.FalseTokenKeyword:
                ParseDefaultKeywords.HandleFalseTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.FinallyTokenKeyword:
                ParseDefaultKeywords.HandleFinallyTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.FixedTokenKeyword:
                ParseDefaultKeywords.HandleFixedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.FloatTokenKeyword:
                ParseDefaultKeywords.HandleFloatTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ForTokenKeyword:
                ParseDefaultKeywords.HandleForTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ForeachTokenKeyword:
                ParseDefaultKeywords.HandleForeachTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.GotoTokenKeyword:
                ParseDefaultKeywords.HandleGotoTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ImplicitTokenKeyword:
                ParseDefaultKeywords.HandleImplicitTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.InTokenKeyword:
                ParseDefaultKeywords.HandleInTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.IntTokenKeyword:
                ParseDefaultKeywords.HandleIntTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.IsTokenKeyword:
                ParseDefaultKeywords.HandleIsTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.LockTokenKeyword:
                ParseDefaultKeywords.HandleLockTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.LongTokenKeyword:
                ParseDefaultKeywords.HandleLongTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.NullTokenKeyword:
                ParseDefaultKeywords.HandleNullTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ObjectTokenKeyword:
                ParseDefaultKeywords.HandleObjectTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.OperatorTokenKeyword:
                ParseDefaultKeywords.HandleOperatorTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.OutTokenKeyword:
                ParseDefaultKeywords.HandleOutTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ParamsTokenKeyword:
                ParseDefaultKeywords.HandleParamsTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ProtectedTokenKeyword:
                ParseDefaultKeywords.HandleProtectedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ReadonlyTokenKeyword:
                ParseDefaultKeywords.HandleReadonlyTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.RefTokenKeyword:
                ParseDefaultKeywords.HandleRefTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.SbyteTokenKeyword:
                ParseDefaultKeywords.HandleSbyteTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ShortTokenKeyword:
                ParseDefaultKeywords.HandleShortTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.SizeofTokenKeyword:
                ParseDefaultKeywords.HandleSizeofTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.StackallocTokenKeyword:
                ParseDefaultKeywords.HandleStackallocTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.StringTokenKeyword:
                ParseDefaultKeywords.HandleStringTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.StructTokenKeyword:
                ParseDefaultKeywords.HandleStructTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.SwitchTokenKeyword:
                ParseDefaultKeywords.HandleSwitchTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ThisTokenKeyword:
                ParseDefaultKeywords.HandleThisTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ThrowTokenKeyword:
                ParseDefaultKeywords.HandleThrowTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.TrueTokenKeyword:
                ParseDefaultKeywords.HandleTrueTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.TryTokenKeyword:
                ParseDefaultKeywords.HandleTryTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.TypeofTokenKeyword:
                ParseDefaultKeywords.HandleTypeofTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UintTokenKeyword:
                ParseDefaultKeywords.HandleUintTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UlongTokenKeyword:
                ParseDefaultKeywords.HandleUlongTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UncheckedTokenKeyword:
                ParseDefaultKeywords.HandleUncheckedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UnsafeTokenKeyword:
                ParseDefaultKeywords.HandleUnsafeTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UshortTokenKeyword:
                ParseDefaultKeywords.HandleUshortTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.VoidTokenKeyword:
                ParseDefaultKeywords.HandleVoidTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.VolatileTokenKeyword:
                ParseDefaultKeywords.HandleVolatileTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.WhileTokenKeyword:
                ParseDefaultKeywords.HandleWhileTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UnrecognizedTokenKeyword:
                ParseDefaultKeywords.HandleUnrecognizedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ReturnTokenKeyword:
                ParseDefaultKeywords.HandleReturnTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.NamespaceTokenKeyword:
                ParseDefaultKeywords.HandleNamespaceTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.ClassTokenKeyword:
                ParseDefaultKeywords.HandleClassTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.InterfaceTokenKeyword:
                ParseDefaultKeywords.HandleInterfaceTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.UsingTokenKeyword:
                ParseDefaultKeywords.HandleUsingTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.PublicTokenKeyword:
                ParseDefaultKeywords.HandlePublicTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.InternalTokenKeyword:
                ParseDefaultKeywords.HandleInternalTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.PrivateTokenKeyword:
                ParseDefaultKeywords.HandlePrivateTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.StaticTokenKeyword:
                ParseDefaultKeywords.HandleStaticTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.OverrideTokenKeyword:
                ParseDefaultKeywords.HandleOverrideTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.VirtualTokenKeyword:
                ParseDefaultKeywords.HandleVirtualTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.AbstractTokenKeyword:
                ParseDefaultKeywords.HandleAbstractTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.SealedTokenKeyword:
                ParseDefaultKeywords.HandleSealedTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.IfTokenKeyword:
                ParseDefaultKeywords.HandleIfTokenKeyword(ref parserModel);
                break;
            case SyntaxKind.NewTokenKeyword:
                ParseDefaultKeywords.HandleNewTokenKeyword(ref parserModel);
                break;
            default:
                ParseDefaultKeywords.HandleDefault(ref parserModel);
                break;
        }
    }

    public static void ParseKeywordContextualToken(ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.ColonToken)
        {
            ParseOthers.HandleLabelDeclaration(ref parserModel);
            return;
        }
    
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.VarTokenContextualKeyword:
                ParseContextualKeywords.HandleVarTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.PartialTokenContextualKeyword:
                ParseContextualKeywords.HandlePartialTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AddTokenContextualKeyword:
                ParseContextualKeywords.HandleAddTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AndTokenContextualKeyword:
                ParseContextualKeywords.HandleAndTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AliasTokenContextualKeyword:
                ParseContextualKeywords.HandleAliasTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AscendingTokenContextualKeyword:
                ParseContextualKeywords.HandleAscendingTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.ArgsTokenContextualKeyword:
                ParseContextualKeywords.HandleArgsTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AsyncTokenContextualKeyword:
                ParseContextualKeywords.HandleAsyncTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.AwaitTokenContextualKeyword:
                ParseContextualKeywords.HandleAwaitTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.ByTokenContextualKeyword:
                ParseContextualKeywords.HandleByTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.DescendingTokenContextualKeyword:
                ParseContextualKeywords.HandleDescendingTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.DynamicTokenContextualKeyword:
                ParseContextualKeywords.HandleDynamicTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.EqualsTokenContextualKeyword:
                ParseContextualKeywords.HandleEqualsTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.FileTokenContextualKeyword:
                ParseContextualKeywords.HandleFileTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.FromTokenContextualKeyword:
                ParseContextualKeywords.HandleFromTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.GetTokenContextualKeyword:
                ParseContextualKeywords.HandleGetTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.GlobalTokenContextualKeyword:
                ParseContextualKeywords.HandleGlobalTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.GroupTokenContextualKeyword:
                ParseContextualKeywords.HandleGroupTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.InitTokenContextualKeyword:
                ParseContextualKeywords.HandleInitTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.IntoTokenContextualKeyword:
                ParseContextualKeywords.HandleIntoTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.JoinTokenContextualKeyword:
                ParseContextualKeywords.HandleJoinTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.LetTokenContextualKeyword:
                ParseContextualKeywords.HandleLetTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.ManagedTokenContextualKeyword:
                ParseContextualKeywords.HandleManagedTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.NameofTokenContextualKeyword:
                ParseContextualKeywords.HandleNameofTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.NintTokenContextualKeyword:
                ParseContextualKeywords.HandleNintTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.NotTokenContextualKeyword:
                ParseContextualKeywords.HandleNotTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.NotnullTokenContextualKeyword:
                ParseContextualKeywords.HandleNotnullTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.NuintTokenContextualKeyword:
                ParseContextualKeywords.HandleNuintTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.OnTokenContextualKeyword:
                ParseContextualKeywords.HandleOnTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.OrTokenContextualKeyword:
                ParseContextualKeywords.HandleOrTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.OrderbyTokenContextualKeyword:
                ParseContextualKeywords.HandleOrderbyTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.RecordTokenContextualKeyword:
                ParseContextualKeywords.HandleRecordTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.RemoveTokenContextualKeyword:
                ParseContextualKeywords.HandleRemoveTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.RequiredTokenContextualKeyword:
                ParseContextualKeywords.HandleRequiredTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.ScopedTokenContextualKeyword:
                ParseContextualKeywords.HandleScopedTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.SelectTokenContextualKeyword:
                ParseContextualKeywords.HandleSelectTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.SetTokenContextualKeyword:
                ParseContextualKeywords.HandleSetTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.UnmanagedTokenContextualKeyword:
                ParseContextualKeywords.HandleUnmanagedTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.ValueTokenContextualKeyword:
                ParseContextualKeywords.HandleValueTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.WhenTokenContextualKeyword:
                ParseContextualKeywords.HandleWhenTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.WhereTokenContextualKeyword:
                ParseContextualKeywords.HandleWhereTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.WithTokenContextualKeyword:
                ParseContextualKeywords.HandleWithTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.YieldTokenContextualKeyword:
                ParseContextualKeywords.HandleYieldTokenContextualKeyword(ref parserModel);
                break;
            case SyntaxKind.UnrecognizedTokenContextualKeyword:
                ParseContextualKeywords.HandleUnrecognizedTokenContextualKeyword(ref parserModel);
                break;
            default:
                // compilationUnit.DiagnosticBag.ReportTodoException(parserModel.TokenWalker.Current.TextSpan, $"Implement the {parserModel.TokenWalker.Current.SyntaxKind.ToString()} contextual keyword.");
                break;
        }
    }
}
