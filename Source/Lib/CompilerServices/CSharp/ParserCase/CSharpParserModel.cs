using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.LexerCase;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.Facts;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase;

/// <summary>
/// The computational state for the CSharpParser is contained within this type.
/// The output of the CSharpParser is the <see cref="CSharpCompilationUnit"/>.<see cref="CSharpCompilationUnit.RootCodeBlockNode"/>
/// </summary>
public ref struct CSharpParserModel
{
    /// <summary>
    /// 0 is the global scope
    /// </summary>
    private int _indexKey = 0;
    
    private int _symbolId = 0;

    public CSharpParserModel(
        CSharpBinder binder,
        CSharpCompilationUnit compilationUnit,
        ref CSharpLexerOutput lexerOutput,
        ICodeBlockOwner currentCodeBlockOwner,
        NamespaceStatementNode topLevelNamespaceStatementNode)
    {
        Binder = binder;
        Compilation = compilationUnit;
        CurrentCodeBlockOwner = currentCodeBlockOwner;
        CurrentNamespaceStatementNode = topLevelNamespaceStatementNode;
    
        TokenWalker = Binder.CSharpParserModel_TokenWalker;
        TokenWalker.Reinitialize(lexerOutput.SyntaxTokenList);
        
        ForceParseExpressionInitialPrimaryExpression = EmptyExpressionNode.Empty;
        
        StatementBuilder = new(Binder);
        
        ParseChildScopeStack = Binder.CSharpParserModel_ParseChildScopeStack;
        ParseChildScopeStack.Clear();
        
        ExpressionList = Binder.CSharpParserModel_ExpressionList;
        ExpressionList.Clear();
        ExpressionList.Add((SyntaxKind.EndOfFileToken, null));
        ExpressionList.Add((SyntaxKind.CloseBraceToken, null));
        ExpressionList.Add((SyntaxKind.StatementDelimiterToken, null));
        
        TryParseExpressionSyntaxKindList = Binder.CSharpParserModel_TryParseExpressionSyntaxKindList;
        TryParseExpressionSyntaxKindList.Clear();
        
        AmbiguousIdentifierExpressionNode = Binder.CSharpParserModel_AmbiguousIdentifierExpressionNode;
        AmbiguousIdentifierExpressionNode.SetSharedInstance(
            default,
            genericParameterListing: default,
            CSharpFacts.Types.Void.ToTypeReference(),
            followsMemberAccessToken: false);
            
        TypeClauseNode = Binder.CSharpParserModel_TypeClauseNode;
        TypeClauseNode.SetSharedInstance(
            typeIdentifier: default,
            genericParameterListing: default,
            isKeywordType: false);
        TypeClauseNode.IsBeingUsed = false;
            
        VariableReferenceNode = Binder.CSharpParserModel_VariableReferenceNode;
        VariableReferenceNode.SetSharedInstance(
            variableIdentifierToken: default,
            variableDeclarationNode: null);
        VariableReferenceNode.IsBeingUsed = false;
        
        ClearedPartialDefinitionHashSet = Binder.CSharpParserModel_ClearedPartialDefinitionHashSet;
        ClearedPartialDefinitionHashSet.Clear();
        
        Binder.MethodOverload_ResourceUri_WasCleared = false;
        
        Text = lexerOutput.Text;
        
        Binder.CSharpParserModel_AddedNamespaceHashSet.Clear();
    }
    
    public ReadOnlySpan<char> Text { get; }

    public TokenWalker TokenWalker { get; }
    public CSharpStatementBuilder StatementBuilder { get; set; }
    
    /// <summary>
    /// Prior to closing a statement-codeblock, you must check whether ParseChildScopeStack has a child that needs to be parsed.
    /// </summary>
    public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> ParseChildScopeStack { get; }
    
    /// <summary>
    /// The C# IParserModel implementation will only "short circuit" if the 'SyntaxKind DelimiterSyntaxKind'
    /// is registered as a delimiter.
    ///
    /// This is done in order to speed up the while loop, as the list of short circuits doesn't have to be
    /// iterated unless the current token is a possible delimiter.
    ///
    /// Walk.CompilerServices.CSharp.ParserCase.Internals.ParseOthers.SyntaxIsEndDelimiter(SyntaxKind syntaxKind) {...}
    /// </summary>
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode ExpressionNode)> ExpressionList { get; set; }
    
    public List<TypeDefinitionNode> ExternalTypeDefinitionList { get; } = new();
    
    public IExpressionNode? NoLongerRelevantExpressionNode { get; set; }
    public List<SyntaxKind> TryParseExpressionSyntaxKindList { get; }
    public IExpressionNode ForceParseExpressionInitialPrimaryExpression { get; set; }
    
    /// <summary>
    /// When parsing a value tuple, this needs to be remembered,
    /// then reset to the initial value foreach of the value tuple's members.
    ///
    /// 'CSharpParserContextKind.ForceStatementExpression' is related
    /// to disambiguating the less than operator '<' and
    /// generic arguments '<...>'.
    ///
    /// Any case where 'ParserContextKind' says that
    /// generic arguments '<...>' for variable declaration
    /// this needs to be available as information to each member.
    /// </summary>
    public CSharpParserContextKind ParserContextKind { get; set; }
    
    public CSharpBinder Binder { get; set; }
    public CSharpCompilationUnit Compilation { get; set; }

    public ICodeBlockOwner CurrentCodeBlockOwner { get; set; }
    public NamespaceStatementNode CurrentNamespaceStatementNode { get; set; }
    public TypeReference MostRecentLeftHandSideAssignmentExpressionTypeClauseNode { get; set; } = CSharpFacts.Types.Void.ToTypeReference();
    
    /// <summary>
    /// TODO: Consider the case where you have just an AmbiguousIdentifierExpressionNode then StatementDelimiterToken.
    /// </summary>
    public AmbiguousIdentifierExpressionNode AmbiguousIdentifierExpressionNode { get; }
    
    /// <summary>
    /// TODO: Consider the case where you have just a TypeClauseNode then StatementDelimiterToken.
    /// </summary>
    public TypeClauseNode TypeClauseNode { get; }
    
    /// <summary>
    /// In order to have many partial definitions for the same type in the same file,
    /// you need to set the ScopeIndexKey to -1 for any entry in the
    /// 'CSharpBinder.PartialTypeDefinitionList' only once per parse.
    ///
    /// Thus, this will track whether a type had been handled already or not.
    /// </summary>
    public HashSet<string> ClearedPartialDefinitionHashSet { get; }
    
    public ParameterModifierKind ParameterModifierKind { get; set; } = ParameterModifierKind.None;
    
    public ArgumentModifierKind ArgumentModifierKind { get; set; } = ArgumentModifierKind.None;
    
    public IExpressionNode ExpressionPrimary { get; set; }
    
    public TypeClauseNode ConstructOrRecycleTypeClauseNode(
        SyntaxToken typeIdentifier,
        Type? valueType,
        GenericParameterListing genericParameterListing,
        bool isKeywordType)
    {
        if (TypeClauseNode.IsBeingUsed)
        {
            return new TypeClauseNode(
                typeIdentifier,
                genericParameterListing,
                isKeywordType);
        }    
        
        TypeClauseNode.SetSharedInstance(
            typeIdentifier,
            genericParameterListing,
            isKeywordType);
            
        return TypeClauseNode;
    }
    
    /// <summary>
    /// TODO: Consider the case where you have just a VariableReferenceNode then StatementDelimiterToken.
    /// </summary>
    public VariableReferenceNode VariableReferenceNode { get; }
    
    public VariableReferenceNode ConstructOrRecycleVariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        VariableDeclarationNode variableDeclarationNode)
    {
        if (VariableReferenceNode.IsBeingUsed)
            return new VariableReferenceNode(variableIdentifierToken, variableDeclarationNode);
    
        VariableReferenceNode.SetSharedInstance(variableIdentifierToken, variableDeclarationNode);
        return VariableReferenceNode;
    }
    
    public ICodeBlockOwner? GetParent(
        ICodeBlockOwner codeBlockOwner,
        Walk.CompilerServices.CSharp.CompilerServiceCase.CSharpCompilationUnit cSharpCompilationUnit)
    {
        if (codeBlockOwner.Unsafe_ParentIndexKey == -1)
            return null;
            
        return (ICodeBlockOwner)cSharpCompilationUnit.CodeBlockOwnerList[codeBlockOwner.Unsafe_ParentIndexKey];
    }
    
    /// <summary>TODO: Delete this code it is only being used temporarily for debugging.</summary>
    // public HashSet<int> SeenTokenIndexHashSet { get; set; } = new();
    
    public int GetNextIndexKey()
    {
        return ++_indexKey;
    }
    
    public int GetNextSymbolId()
    {
        return ++_symbolId;
    }
    
    public void BindDiscard(SyntaxToken identifierToken)
    {
        Compilation.__SymbolList.Add(
            new Symbol(
                SyntaxKind.DiscardSymbol,
                GetNextSymbolId(),
                identifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None,
                }));
    }
    
    public void BindFunctionDefinitionNode(FunctionDefinitionNode functionDefinitionNode)
    {
        var functionSymbol = new Symbol(
            SyntaxKind.FunctionSymbol,
            GetNextSymbolId(),
            functionDefinitionNode.FunctionIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Function
            });

        Compilation.__SymbolList.Add(functionSymbol);
    }
    
    public void BindNamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
    {
        var namespaceString = Binder.TextEditorService.EditContext_GetText(Text.Slice(namespaceStatementNode.IdentifierToken.TextSpan.StartInclusiveIndex, namespaceStatementNode.IdentifierToken.TextSpan.Length));

        if (Binder._namespaceGroupMap.TryGetValue(namespaceString, out var inNamespaceGroupNode))
        {
            inNamespaceGroupNode.NamespaceStatementNodeList.Add(namespaceStatementNode);
        }
        else
        {
            Binder._namespaceGroupMap.Add(namespaceString, new NamespaceGroup(
                namespaceString,
                new List<NamespaceStatementNode> { namespaceStatementNode }));
            
            var splitResult = namespaceString.Split('.');
            
            NamespacePrefixNode? namespacePrefixNode = null;
            
            foreach (var namespacePrefix in splitResult)
            {
                namespacePrefixNode = Binder.NamespacePrefixTree.AddNamespacePrefix(namespacePrefix, namespacePrefixNode);
            }
        }
    }
    
    public void BindVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode, bool shouldCreateVariableSymbol = true)
    {
        if (shouldCreateVariableSymbol)
            CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind);
        
        var text = Binder.TextEditorService.EditContext_GetText(Text.Slice(variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex, variableDeclarationNode.IdentifierToken.TextSpan.Length));
        
        if (TryGetVariableDeclarationNodeByScope(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                out var existingVariableDeclarationNode))
        {
            if (existingVariableDeclarationNode.IsFabricated)
            {
                // Overwrite the fabricated definition with a real one
                //
                // TODO: Track one or many declarations?...
                // (if there is an error where something is defined twice for example)
                SetVariableDeclarationNodeByScope(
                    Compilation,
                    CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    text,
                    variableDeclarationNode);
            }

            DiagnosticHelper.ReportAlreadyDefinedVariable(
                Compilation.__DiagnosticList,
                variableDeclarationNode.IdentifierToken.TextSpan,
                text);
        }
        else
        {
            _ = TryAddVariableDeclarationNodeByScope(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                variableDeclarationNode);
        }
    }
    
    public void BindLabelDeclarationNode(LabelDeclarationNode labelDeclarationNode)
    {
        Compilation.__SymbolList.Add(
            new Symbol(
                SyntaxKind.LabelSymbol,
                GetNextSymbolId(),
                labelDeclarationNode.IdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None
                }));
            
        var text = Binder.TextEditorService.EditContext_GetText(Text.Slice(labelDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex, labelDeclarationNode.IdentifierToken.TextSpan.Length));
        
        if (TryGetLabelDeclarationNodeByScope(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                out var existingLabelDeclarationNode))
        {
            if (existingLabelDeclarationNode.IsFabricated)
            {
                // Overwrite the fabricated definition with a real one
                //
                // TODO: Track one or many declarations?...
                // (if there is an error where something is defined twice for example)
                SetLabelDeclarationNodeByScope(
                    Compilation,
                    CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    text,
                    labelDeclarationNode);
            }

            DiagnosticHelper.ReportAlreadyDefinedLabel(
                Compilation.__DiagnosticList,
                labelDeclarationNode.IdentifierToken.TextSpan,
                text);
        }
        else
        {
            _ = TryAddLabelDeclarationNodeByScope(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                labelDeclarationNode);
        }
    }

    public VariableReferenceNode ConstructAndBindVariableReferenceNode(SyntaxToken variableIdentifierToken)
    {
        var text = Binder.TextEditorService.EditContext_GetText(Text.Slice(variableIdentifierToken.TextSpan.StartInclusiveIndex, variableIdentifierToken.TextSpan.Length));
        VariableReferenceNode? variableReferenceNode;

        if (TryGetVariableDeclarationHierarchically(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                out var variableDeclarationNode)
            && variableDeclarationNode is not null)
        {
            variableReferenceNode = ConstructOrRecycleVariableReferenceNode(
                variableIdentifierToken,
                variableDeclarationNode);
        }
        else
        {
            variableDeclarationNode = new VariableDeclarationNode(
                CSharpFacts.Types.Var.ToTypeReference(),
                variableIdentifierToken,
                VariableKind.Local,
                false,
                Compilation.ResourceUri)
            {
                IsFabricated = true,
            };

            variableReferenceNode = ConstructOrRecycleVariableReferenceNode(
                variableIdentifierToken,
                variableDeclarationNode);
        }

        CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, variableDeclarationNode.VariableKind);
        return variableReferenceNode;
    }
    
    public void BindLabelReferenceNode(LabelReferenceNode labelReferenceNode)
    {
        Compilation.__SymbolList.Add(
            new Symbol(
                SyntaxKind.LabelSymbol,
                GetNextSymbolId(),
                labelReferenceNode.IdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None
                }));
    }

    public void BindConstructorDefinitionIdentifierToken(SyntaxToken identifierToken)
    {
        var constructorSymbol = new Symbol(
            SyntaxKind.ConstructorSymbol,
            GetNextSymbolId(),
            identifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Type
            });

        Compilation.__SymbolList.Add(constructorSymbol);
    }

    public void BindFunctionInvocationNode(FunctionInvocationNode functionInvocationNode)
    {
        var functionInvocationIdentifierText = Binder.TextEditorService.EditContext_GetText(Text.Slice(functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.Length));

        var functionSymbol = new Symbol(
            SyntaxKind.FunctionSymbol,
            GetNextSymbolId(),
            functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Function
            });

        Compilation.__SymbolList.Add(functionSymbol);

        if (TryGetFunctionHierarchically(
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                functionInvocationIdentifierText,
                out var functionDefinitionNode) &&
            functionDefinitionNode is not null)
        {
            functionInvocationNode.ResultTypeReference = functionDefinitionNode.ReturnTypeReference;
        }
    }

    public void BindNamespaceReference(SyntaxToken namespaceIdentifierToken)
    {
        var namespaceSymbol = new Symbol(
            SyntaxKind.NamespaceSymbol,
            GetNextSymbolId(),
            namespaceIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.None
            });

        Compilation.__SymbolList.Add(namespaceSymbol);
    }

    public void BindTypeClauseNode(TypeClauseNode typeClauseNode)
    {
        if (!typeClauseNode.IsKeywordType)
        {
            var typeSymbol = new Symbol(
                SyntaxKind.TypeSymbol,
                GetNextSymbolId(),
                typeClauseNode.TypeIdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.Type
                });

            Compilation.__SymbolList.Add(typeSymbol);
        }
    }
    
    public void BindTypeIdentifier(SyntaxToken identifierToken)
    {
        if (identifierToken.SyntaxKind == SyntaxKind.IdentifierToken)
        {
            var typeSymbol = new Symbol(
                SyntaxKind.TypeSymbol,
                GetNextSymbolId(),
                identifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.Type
                });

            Compilation.__SymbolList.Add(typeSymbol);
        }
    }

    public void BindUsingStatementTuple(SyntaxToken usingKeywordToken, SyntaxToken namespaceIdentifierToken)
    {
        AddNamespaceToCurrentScope(
            Binder.TextEditorService.EditContext_GetText(Text.Slice(namespaceIdentifierToken.TextSpan.StartInclusiveIndex, namespaceIdentifierToken.TextSpan.Length)));
    }
    
    public void BindTypeDefinitionNode(TypeDefinitionNode typeDefinitionNode, bool shouldOverwrite = false)
    {
        var typeIdentifierText = Binder.TextEditorService.EditContext_GetText(Text.Slice(typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeDefinitionNode.TypeIdentifierToken.TextSpan.Length));

        var currentNamespaceStatementText = CurrentNamespaceStatementNode.IdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
        var namespaceAndTypeIdentifiers = new NamespaceAndTypeIdentifiers(currentNamespaceStatementText, typeIdentifierText);

        typeDefinitionNode.EncompassingNamespaceIdentifierString = currentNamespaceStatementText;

        var success = Binder._allTypeDefinitions.TryAdd(typeIdentifierText, typeDefinitionNode);
        if (!success)
        {
            var entryFromAllTypeDefinitions = Binder._allTypeDefinitions[typeIdentifierText];
            
            if (shouldOverwrite || entryFromAllTypeDefinitions.IsFabricated)
                Binder._allTypeDefinitions[typeIdentifierText] = typeDefinitionNode;
        }
    }

    /// <summary>
    /// If the 'codeBlockBuilder.ScopeIndexKey' is null then a scope will be instantiated
    /// added to the list of scopes. The 'codeBlockBuilder.ScopeIndexKey' will then be set
    /// to the instantiated scope's 'IndexKey'. As well, the current scope index key will be set to the
    /// instantiated scope's 'IndexKey'.
    /// 
    /// Also will update the 'CurrentCodeBlockBuilder'.
    /// </summary>
    public void NewScopeAndBuilderFromOwner(ICodeBlockOwner codeBlockOwner, TextEditorTextSpan textSpan)
    {
        codeBlockOwner.Unsafe_ParentIndexKey = CurrentCodeBlockOwner.Unsafe_SelfIndexKey;
        codeBlockOwner.Scope_StartInclusiveIndex = textSpan.StartInclusiveIndex;

        codeBlockOwner.Unsafe_SelfIndexKey = Compilation.CodeBlockOwnerList.Count;
        Compilation.CodeBlockOwnerList.Add(codeBlockOwner);

        var parent = GetParent(codeBlockOwner, Compilation);
        
        var parentScopeDirection = parent?.ScopeDirectionKind ?? ScopeDirectionKind.Both;
        if (parentScopeDirection == ScopeDirectionKind.Both)
            codeBlockOwner.PermitCodeBlockParsing = false;
    
        CurrentCodeBlockOwner = codeBlockOwner;
        
        OnBoundScopeCreatedAndSetAsCurrent(codeBlockOwner, Compilation);
    }

    public void AddNamespaceToCurrentScope(string namespaceString)
    {
        if (!Binder.CSharpParserModel_AddedNamespaceHashSet.Add(namespaceString))
            return;
    
        if (Binder._namespaceGroupMap.TryGetValue(namespaceString, out var namespaceGroup) &&
            namespaceGroup.ConstructorWasInvoked)
        {
            var typeDefinitionNodeList = Binder.Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(namespaceGroup);
            
            foreach (var typeDefinitionNode in typeDefinitionNodeList)
            {
                ExternalTypeDefinitionList.Add(typeDefinitionNode);
            }
        }
    }

    public void CloseScope(TextEditorTextSpan textSpan)
    {
        // Check if it is the global scope, if so return early.
        if (CurrentCodeBlockOwner.Unsafe_SelfIndexKey == 0)
            return;
        
        if (Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_MinimumLocalsData &&
            (CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
             CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.ArbitraryCodeBlockNode))
        {
            for (int i = Compilation.NodeList.Count - 1; i >= 0; i--)
            {
                if (Compilation.NodeList[i].Unsafe_ParentIndexKey == CurrentCodeBlockOwner.Unsafe_SelfIndexKey)
                    Compilation.NodeList.RemoveAt(i);
            }
        }
        
        CurrentCodeBlockOwner.Scope_EndExclusiveIndex = textSpan.EndExclusiveIndex;
        CurrentCodeBlockOwner = GetParent(CurrentCodeBlockOwner, Compilation);
    }

    /// <summary>
    /// Returns the 'symbolId: Compilation.BinderSession.GetNextSymbolId();'
    /// that was used to construct the ITextEditorSymbol.
    /// </summary>
    public int CreateVariableSymbol(SyntaxToken identifierToken, VariableKind variableKind)
    {
        var symbolId = GetNextSymbolId();
        
        switch (variableKind)
        {
            case VariableKind.Field:
                Compilation.__SymbolList.Add(
                    new Symbol(
                        SyntaxKind.FieldSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Field
                        }));
                break;
            case VariableKind.Property:
                Compilation.__SymbolList.Add(
                    new Symbol(
                        SyntaxKind.PropertySymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Property
                        }));
                break;
            case VariableKind.EnumMember:
                Compilation.__SymbolList.Add(
                    new Symbol(
                        SyntaxKind.EnumMemberSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Property
                        }));
                break;
            case VariableKind.Local:
                goto default;
            case VariableKind.Closure:
                goto default;
            default:
                Compilation.__SymbolList.Add(
                    new Symbol(
                        SyntaxKind.VariableSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Variable
                        }));
                break;
        }
        
        return symbolId;
    }
    
    public void SetCurrentNamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
    {
        CurrentNamespaceStatementNode = namespaceStatementNode;
    }
    
    public void OnBoundScopeCreatedAndSetAsCurrent(ICodeBlockOwner codeBlockOwner, CSharpCompilationUnit compilationUnit)
    {
        switch (codeBlockOwner.SyntaxKind)
        {
            case SyntaxKind.NamespaceStatementNode:
                var namespaceStatementNode = (NamespaceStatementNode)codeBlockOwner;
                var namespaceString = Binder.TextEditorService.EditContext_GetText(Text.Slice(namespaceStatementNode.IdentifierToken.TextSpan.StartInclusiveIndex, namespaceStatementNode.IdentifierToken.TextSpan.Length));
                AddNamespaceToCurrentScope(namespaceString);
                
                BindNamespaceStatementNode((NamespaceStatementNode)codeBlockOwner);
                return;
            case SyntaxKind.LambdaExpressionNode:
                var lambdaExpressionNode = (LambdaExpressionNode)codeBlockOwner;
                foreach (var variableDeclarationNode in lambdaExpressionNode.VariableDeclarationNodeList)
                {
                    BindVariableDeclarationNode(variableDeclarationNode);
                }
                return;
            case SyntaxKind.TryStatementCatchNode:
                var tryStatementCatchNode = (TryStatementCatchNode)codeBlockOwner;
            
                if (tryStatementCatchNode.VariableDeclarationNode is not null)
                    BindVariableDeclarationNode(tryStatementCatchNode.VariableDeclarationNode);
                    
                return;
            case SyntaxKind.TypeDefinitionNode:
            
                BindTypeDefinitionNode((TypeDefinitionNode)codeBlockOwner, true);
            
                var typeDefinitionNode = (TypeDefinitionNode)codeBlockOwner;
                
                if (typeDefinitionNode.GenericParameterListing.ConstructorWasInvoked)
                {
                    foreach (var entry in typeDefinitionNode.GenericParameterListing.GenericParameterEntryList)
                    {
                        BindTypeDefinitionNode(
                            new TypeDefinitionNode(
                                AccessModifierKind.Public,
                                hasPartialModifier: false,
                                StorageModifierKind.Class,
                                entry.TypeReference.TypeIdentifierToken,
                                entry.TypeReference.GenericParameterListing,
                                primaryConstructorFunctionArgumentListing: default,
                                inheritedTypeReference: TypeFacts.NotApplicable.ToTypeReference(),
                                string.Empty,
                                compilationUnit.ResourceUri));
                    }
                }
                
                return;
        }
    }
    
    public void SetOpenCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_StartInclusiveIndex)
    {
        codeBlockOwner.CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
    }
    
    public void SetCloseCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_EndExclusiveIndex)
    {
        codeBlockOwner.CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetTypeDefinitionHierarchically(
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetTypeDefinitionNodeByScope(
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    out typeDefinitionNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
            {
                localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
            }
        }

        typeDefinitionNode = null;
        return false;
    }
    
    public bool TryGetTypeDefinitionNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string typeIdentifierText,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        typeDefinitionNode = null;
        foreach (var x in compilationUnit.CodeBlockOwnerList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                GetIdentifierText(x, compilationUnit) == typeIdentifierText)
            {
                typeDefinitionNode = (TypeDefinitionNode)x;
                break;
            }
        }
        
        if (typeDefinitionNode is null)
        {
            if (scopeIndexKey == 0)
            {
                foreach (var x in ExternalTypeDefinitionList)
                {
                    if (GetIdentifierText(x, compilationUnit) == typeIdentifierText)
                    {
                        typeDefinitionNode = (TypeDefinitionNode)x;
                        break;
                    }
                }
                if (typeDefinitionNode is not null)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            return true;
        }
    }
    
    public bool TryAddTypeDefinitionNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string typeIdentifierText,
        TypeDefinitionNode typeDefinitionNode)
    {
        TypeDefinitionNode? matchNode = null;
        foreach (var x in compilationUnit.CodeBlockOwnerList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                GetIdentifierText(x, compilationUnit) == typeIdentifierText)
            {
                matchNode = (TypeDefinitionNode)x;
                break;
            }
        }
        
        if (matchNode is null)
        {
            compilationUnit.CodeBlockOwnerList.Add(typeDefinitionNode);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public FunctionDefinitionNode[] GetFunctionDefinitionNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        return compilationUnit.CodeBlockOwnerList
            .Where(kvp => kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
            .Select(kvp => (FunctionDefinitionNode)kvp)
            .ToArray();
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetFunctionHierarchically(
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out FunctionDefinitionNode? functionDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetFunctionDefinitionNodeByScope(
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    out functionDefinitionNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        functionDefinitionNode = null;
        return false;
    }
    
    public bool TryGetFunctionDefinitionNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string functionIdentifierText,
        out FunctionDefinitionNode functionDefinitionNode)
    {
        functionDefinitionNode = null;
        foreach (var x in compilationUnit.CodeBlockOwnerList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.FunctionDefinitionNode &&
                GetIdentifierText(x, compilationUnit) == functionIdentifierText)
            {
                functionDefinitionNode = (FunctionDefinitionNode)x;
                break;
            }
        }
        
        if (functionDefinitionNode is null)
            return false;
        else
            return true;
    }
    
    public VariableDeclarationNode[] GetVariableDeclarationNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        return compilationUnit.NodeList
            .Where(kvp => kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            .Select(kvp => (VariableDeclarationNode)kvp)
            .ToArray();
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="_currentScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetVariableDeclarationHierarchically(
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out VariableDeclarationNode? variableDeclarationStatementNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetVariableDeclarationNodeByScope(
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    out variableDeclarationStatementNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        variableDeclarationStatementNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        out VariableDeclarationNode? variableDeclarationNode)
    {
        variableDeclarationNode = null;
        foreach (var x in compilationUnit.NodeList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == variableIdentifierText)
            {
                variableDeclarationNode = (VariableDeclarationNode)x;
                break;
            }
        }
        
        if (variableDeclarationNode is null)
            return false;
        else
            return true;
    }
    
    public bool TryAddVariableDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        VariableDeclarationNode variableDeclarationNode)
    {
        VariableDeclarationNode? matchNode = null;
        foreach (var x in compilationUnit.NodeList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == variableIdentifierText)
            {
                matchNode = (VariableDeclarationNode)x;
                break;
            }
        }
        
        if (matchNode is null)
        {
            variableDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            compilationUnit.NodeList.Add(variableDeclarationNode);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void SetVariableDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        VariableDeclarationNode variableDeclarationNode)
    {
        VariableDeclarationNode? matchNode = null;
        int index = 0;
        for (; index < compilationUnit.NodeList.Count; index++)
        {
            var x = compilationUnit.NodeList[index];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == variableIdentifierText)
            {
                matchNode = (VariableDeclarationNode)x;
                break;
            }
        }
        
        if (index != -1)
        {
            variableDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            compilationUnit.NodeList[index] = variableDeclarationNode;
        }
    }
    
    public bool TryGetLabelDeclarationHierarchically(
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out LabelDeclarationNode? labelDeclarationNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetLabelDeclarationNodeByScope(
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    out labelDeclarationNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        labelDeclarationNode = null;
        return false;
    }
    
    public bool TryGetLabelDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string labelIdentifierText,
        out LabelDeclarationNode labelDeclarationNode)
    {
        labelDeclarationNode = null;
        foreach (var x in compilationUnit.NodeList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == labelIdentifierText)
            {
                labelDeclarationNode = (LabelDeclarationNode)x;
                break;
            }
        }
        
        if (labelDeclarationNode is null)
            return false;
        else
            return true;
    }
    
    public bool TryAddLabelDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string labelIdentifierText,
        LabelDeclarationNode labelDeclarationNode)
    {
        LabelDeclarationNode? matchNode = null;
        foreach (var x in compilationUnit.NodeList)
        {
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == labelIdentifierText)
            {
                matchNode = (LabelDeclarationNode)x;
                break;
            }
        }
        
        if (matchNode is null)
        {
            labelDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            compilationUnit.NodeList.Add(labelDeclarationNode);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void SetLabelDeclarationNodeByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string labelIdentifierText,
        LabelDeclarationNode labelDeclarationNode)
    {
        LabelDeclarationNode? matchNode = null;
        int index = 0;
        for (; index < compilationUnit.NodeList.Count; index++)
        {
            var x = compilationUnit.NodeList[index];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                GetIdentifierText(x, compilationUnit) == labelIdentifierText)
            {
                matchNode = (LabelDeclarationNode)x;
                break;
            }
        }

        if (index != -1)
        {
            labelDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            compilationUnit.NodeList[index] = labelDeclarationNode;
        }
    }
    
    
    /// <summary>
    /// !!!!
    /// This will be a DRY code nightmare for a moment.
    /// Everything past this point was copy and pasted from CSharpBinder.
    ///
    /// The issue is, while parsing you want to get the text from the TextEditorTextSpan
    /// as quickly as possible.
    ///
    /// But, if someone wants to look at an already completed CSharpCompilationUnit,
    /// there are a great deal of timing issues that arise here.
    /// Thus, you'd want to take caution when getting the text.
    ///
    /// So, instead of invoking TextEditorTextSpan.GetText(...)
    /// I want to directly:
    /// 'textEditorService.EditContext_GetText(sourceText.AsSpan(textSpan.StartInclusiveIndex, textSpan.Length));'
    /// !!!!
    ///
    /// TODO: don't pass the initial compilationUnit it is default.
    /// TODO: Are these adds being invoked separately?
    /// </summary>
    public string GetIdentifierText(ISyntaxNode node, CSharpCompilationUnit compilationUnit)
    {
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)node;
                if (typeDefinitionNode.ResourceUri == compilationUnit.ResourceUri && compilationUnit.ResourceUri == Compilation.ResourceUri)
                {
                    return Binder.TextEditorService.EditContext_GetText(Text.Slice(typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeDefinitionNode.TypeIdentifierToken.TextSpan.Length));
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.TextEditorService.EditContext_GetText(innerCompilationUnit.SourceText.AsSpan(typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeDefinitionNode.TypeIdentifierToken.TextSpan.Length));
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.TypeClauseNode:
            {
                var typeClauseNode = (TypeClauseNode)node;
                if (typeClauseNode.ExplicitDefinitionResourceUri == compilationUnit.ResourceUri && compilationUnit.ResourceUri == Compilation.ResourceUri)
                {
                    return Binder.TextEditorService.EditContext_GetText(Text.Slice(typeClauseNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeClauseNode.TypeIdentifierToken.TextSpan.Length));
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(typeClauseNode.ExplicitDefinitionResourceUri, out var innerCompilationUnit))
                        return Binder.TextEditorService.EditContext_GetText(innerCompilationUnit.SourceText.AsSpan(typeClauseNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeClauseNode.TypeIdentifierToken.TextSpan.Length));
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                if (functionDefinitionNode.ResourceUri == compilationUnit.ResourceUri && compilationUnit.ResourceUri == Compilation.ResourceUri)
                {
                    return Binder.TextEditorService.EditContext_GetText(Text.Slice(functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex, functionDefinitionNode.FunctionIdentifierToken.TextSpan.Length));
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.TextEditorService.EditContext_GetText(innerCompilationUnit.SourceText.AsSpan(functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex, functionDefinitionNode.FunctionIdentifierToken.TextSpan.Length));
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                var functionInvocationNode = (FunctionInvocationNode)node;
                if (functionInvocationNode.ResourceUri == compilationUnit.ResourceUri && compilationUnit.ResourceUri == Compilation.ResourceUri)
                {
                    return Binder.TextEditorService.EditContext_GetText(Text.Slice(functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.Length));
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(functionInvocationNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.TextEditorService.EditContext_GetText(innerCompilationUnit.SourceText.AsSpan(functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.Length));
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)node;
                if (variableDeclarationNode.ResourceUri == compilationUnit.ResourceUri && compilationUnit.ResourceUri == Compilation.ResourceUri)
                {
                    return Binder.TextEditorService.EditContext_GetText(Text.Slice(variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex, variableDeclarationNode.IdentifierToken.TextSpan.Length));
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.TextEditorService.EditContext_GetText(innerCompilationUnit.SourceText.AsSpan(variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex, variableDeclarationNode.IdentifierToken.TextSpan.Length));
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.VariableReferenceNode:
            {
                return Binder.TextEditorService.EditContext_GetText(Text.Slice(((VariableReferenceNode)node).VariableIdentifierToken.TextSpan.StartInclusiveIndex, ((VariableReferenceNode)node).VariableIdentifierToken.TextSpan.Length));
            }
            case SyntaxKind.LabelDeclarationNode:
            {
                return Binder.TextEditorService.EditContext_GetText(Text.Slice(((LabelDeclarationNode)node).IdentifierToken.TextSpan.StartInclusiveIndex, ((LabelDeclarationNode)node).IdentifierToken.TextSpan.Length));
            }
            case SyntaxKind.LabelReferenceNode:
            {
                return Binder.TextEditorService.EditContext_GetText(Text.Slice(((LabelReferenceNode)node).IdentifierToken.TextSpan.StartInclusiveIndex, ((LabelReferenceNode)node).IdentifierToken.TextSpan.Length));
            }
            default:
            {
                return string.Empty;
            }
        }
    }
}
