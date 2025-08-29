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
        ResourceUri resourceUri,
        ref CSharpCompilationUnit compilationUnit,
        ref CSharpLexerOutput lexerOutput)
    {
        Binder = binder;
        Compilation = ref compilationUnit;
        CurrentCodeBlockOwner = binder.GlobalCodeBlockNode;
        CurrentNamespaceStatementNode = binder.TopLevelNamespaceStatementNode;
        ResourceUri = resourceUri;
    
        TokenWalker = Binder.CSharpParserModel_TokenWalker;
        TokenWalker.Reinitialize(lexerOutput.SyntaxTokenList);
        
        ForceParseExpressionInitialPrimaryExpression = EmptyExpressionNode.Empty;
        
        StatementBuilder = Binder.CSharpParserModel_StatementBuilder;
        
        ParseChildScopeStack = Binder.CSharpParserModel_ParseChildScopeStack;
        ParseChildScopeStack.Clear();
        
        ExpressionList = Binder.CSharpParserModel_ExpressionList;
        ExpressionList.Clear();
        ExpressionList.Add((SyntaxKind.EndOfFileToken, null));
        ExpressionList.Add((SyntaxKind.CloseBraceToken, null));
        ExpressionList.Add((SyntaxKind.StatementDelimiterToken, null));
        
        TryParseExpressionSyntaxKindList = Binder.CSharpParserModel_TryParseExpressionSyntaxKindList;
        TryParseExpressionSyntaxKindList.Clear();
        
        ClearedPartialDefinitionHashSet = Binder.CSharpParserModel_ClearedPartialDefinitionHashSet;
        ClearedPartialDefinitionHashSet.Clear();
        
        Binder.MethodOverload_ResourceUri_WasCleared = false;
        
        Binder.CSharpParserModel_AddedNamespaceList.Clear();
        
        ExternalTypeDefinitionList = Binder.CSharpParserModel_ExternalTypeDefinitionList;
        ExternalTypeDefinitionList.Clear();
        ExternalTypeDefinitionList.AddRange(CSharpFacts.Types.TypeDefinitionNodes);
        
        Binder.AmbiguousParenthesizedExpressionNodeChildList.Clear();
        Binder.LambdaExpressionNodeChildList.Clear();
        
        if (Compilation.CompilationUnitKind == CompilationUnitKind.IndividualFile_AllData)
        {
            if (Binder.SymbolIdToExternalTextSpanMap.TryGetValue(ResourceUri.Value, out var symbolIdToExternalTextSpanMap))
                symbolIdToExternalTextSpanMap.Clear();
            else 
                Binder.SymbolIdToExternalTextSpanMap.Add(ResourceUri.Value, new());
        }
        else
        {
            Binder.SymbolIdToExternalTextSpanMap.Remove(ResourceUri.Value);
        }
        
        Compilation.IndexDiagnosticList = Binder.DiagnosticList.Count;
        
        Compilation.IndexSymbolList = Binder.SymbolList.Count;
        
        Compilation.IndexNodeList = Binder.NodeList.Count;
    }
    
    public TokenWalker TokenWalker { get; }
    public CSharpStatementBuilder StatementBuilder { get; set; }
    
    public ResourceUri ResourceUri { get; }

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
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode? ExpressionNode)> ExpressionList { get; set; }
    
    public List<TypeDefinitionNode> ExternalTypeDefinitionList { get; }
    
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
    public ref CSharpCompilationUnit Compilation;

    public ICodeBlockOwner CurrentCodeBlockOwner { get; set; }
    public NamespaceStatementNode CurrentNamespaceStatementNode { get; set; }
    public TypeReference MostRecentLeftHandSideAssignmentExpressionTypeClauseNode { get; set; } = CSharpFacts.Types.Void.ToTypeReference();
    
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
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TypeClauseNode instance's:
    /// - TypeIdentifierToken
    /// Thus, the Return_TypeClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly TypeClauseNode Rent_TypeClauseNode()
    {
        if (Binder.Pool_TypeClauseNode_Queue.TryDequeue(out var typeClauseNode))
        {
            return typeClauseNode;
        }
        
        return new TypeClauseNode(
            typeIdentifier: default,
            openAngleBracketToken: default,
    		indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
    		closeAngleBracketToken: default,
            isKeywordType: false);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TypeClauseNode instance's:
    /// - TypeIdentifierToken
    /// Thus, the Return_TypeClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_TypeClauseNode(TypeClauseNode typeClauseNode)
    {
        typeClauseNode.OpenAngleBracketToken = default;
        typeClauseNode.IndexGenericParameterEntryList = -1;
        typeClauseNode.CountGenericParameterEntryList = 0;
        typeClauseNode.CloseAngleBracketToken = default;
        typeClauseNode.IsKeywordType = false;
        typeClauseNode.TypeKind = TypeKind.None;
        typeClauseNode.HasQuestionMark = false;
        typeClauseNode.ArrayRank = 0;
        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //typeClauseNode._isFabricated = false;
        typeClauseNode.IsParsingGenericParameters = false;
        typeClauseNode.ExplicitDefinitionTextSpan = default;
        typeClauseNode.ExplicitDefinitionResourceUri = default;
    
        Binder.Pool_TypeClauseNode_Queue.Enqueue(typeClauseNode);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly VariableReferenceNode Rent_VariableReferenceNode()
    {
        if (Binder.Pool_VariableReferenceNode_Queue.TryDequeue(out var variableReferenceNode))
        {
            return variableReferenceNode;
        }

        return new VariableReferenceNode(
            variableIdentifierToken: default,
            variableDeclarationNode: default);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_VariableReferenceNode(VariableReferenceNode variableReferenceNode)
    {
        variableReferenceNode.VariableDeclarationNode = default;
        variableReferenceNode._isFabricated = false;
    
        Binder.Pool_VariableReferenceNode_Queue.Enqueue(variableReferenceNode);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly VariableReference Return_VariableReferenceNode_ToStruct(VariableReferenceNode variableReferenceNode)
    {
        var variableReference = new VariableReference(variableReferenceNode);
        Return_VariableReferenceNode(variableReferenceNode);
        return variableReference;
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned NamespaceClauseNode instance's:
    /// - IdentifierToken
    /// Thus, the Return_NamespaceClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly NamespaceClauseNode Rent_NamespaceClauseNode()
    {
        if (Binder.Pool_NamespaceClauseNode_Queue.TryDequeue(out var namespaceClauseNode))
        {
            return namespaceClauseNode;
        }

        return new NamespaceClauseNode(
            identifierToken: default);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned NamespaceClauseNode instance's:
    /// - IdentifierToken
    /// Thus, the Return_NamespaceClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_NamespaceClauseNode(NamespaceClauseNode namespaceClauseNode)
    {
        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //namespaceClauseNode._isFabricated = false;

        namespaceClauseNode.NamespacePrefixNode = null;
        namespaceClauseNode.PreviousNamespaceClauseNode = null;
        namespaceClauseNode.StartOfMemberAccessChainPositionIndex = default;
    
        Binder.Pool_NamespaceClauseNode_Queue.Enqueue(namespaceClauseNode);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned AmbiguousIdentifierExpressionNode instance's:
    /// - Token
    /// - FollowsMemberAccessToken
    /// Thus, the Return_AmbiguousIdentifierExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly AmbiguousIdentifierExpressionNode Rent_AmbiguousIdentifierExpressionNode()
    {
        if (Binder.Pool_AmbiguousIdentifierExpressionNode_Queue.TryDequeue(out var ambiguousIdentifierExpressionNode))
        {
            return ambiguousIdentifierExpressionNode;
        }
        
        return new AmbiguousIdentifierExpressionNode(
            token: default,
            openAngleBracketToken: default,
            indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
            closeAngleBracketToken: default,
            resultTypeReference: CSharpFacts.Types.Void.ToTypeReference());
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned AmbiguousIdentifierExpressionNode instance's:
    /// - Token
    /// - FollowsMemberAccessToken
    /// Thus, the Return_AmbiguousIdentifierExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_AmbiguousIdentifierExpressionNode(AmbiguousIdentifierExpressionNode ambiguousIdentifierExpressionNode)
    {
        ambiguousIdentifierExpressionNode.OpenAngleBracketToken = default;
        ambiguousIdentifierExpressionNode.IndexGenericParameterEntryList = -1;
        ambiguousIdentifierExpressionNode.CountGenericParameterEntryList = 0;
        ambiguousIdentifierExpressionNode.CloseAngleBracketToken = default;
        
        ambiguousIdentifierExpressionNode.ResultTypeReference = CSharpFacts.Types.Void.ToTypeReference();
        ambiguousIdentifierExpressionNode.HasQuestionMark = false;
    
        Binder.Pool_AmbiguousIdentifierExpressionNode_Queue.Enqueue(ambiguousIdentifierExpressionNode);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned FunctionInvocationNode instance's:
    /// - FunctionInvocationIdentifierToken
    /// Thus, the Return_FunctionInvocationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly FunctionInvocationNode Rent_FunctionInvocationNode()
    {
        if (Binder.Pool_FunctionInvocationNode_Queue.TryDequeue(out var functionInvocationNode))
        {
            return functionInvocationNode;
        }

        return new FunctionInvocationNode(
            functionInvocationIdentifierToken: default,        
            openAngleBracketToken: default,
            indexGenericParameterEntryList: -1,
            countGenericParameterEntryList: 0,
            closeAngleBracketToken: default,
            openParenthesisToken: default,
            indexFunctionParameterEntryList: -1,
            countFunctionParameterEntryList: 0,
            closeParenthesisToken: default,
            resultTypeReference: CSharpFacts.Types.Void.ToTypeReference());
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned FunctionInvocationNode instance's:
    /// - FunctionInvocationIdentifierToken
    /// Thus, the Return_FunctionInvocationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_FunctionInvocationNode(FunctionInvocationNode functionInvocationNode)
    {
        functionInvocationNode.OpenAngleBracketToken = default;
        functionInvocationNode.IndexGenericParameterEntryList = -1;
        functionInvocationNode.CountGenericParameterEntryList = 0;
        functionInvocationNode.CloseAngleBracketToken = default;
        
        functionInvocationNode.OpenParenthesisToken = default;
        functionInvocationNode.IndexFunctionParameterEntryList = -1;
        functionInvocationNode.CountFunctionParameterEntryList = 0;
        functionInvocationNode.CloseParenthesisToken = default;
        
        functionInvocationNode.ResultTypeReference = CSharpFacts.Types.Void.ToTypeReference();
        
        functionInvocationNode.ResourceUri = default;
        functionInvocationNode.ExplicitDefinitionTextSpan = default;

        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //functionInvocationNode._isFabricated = false;

        functionInvocationNode.IsParsingFunctionParameters = false;
        functionInvocationNode.IsParsingGenericParameters = false;
    
        Binder.Pool_FunctionInvocationNode_Queue.Enqueue(functionInvocationNode);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned ConstructorInvocationExpressionNode instance's:
    /// - NewKeywordToken
    /// Thus, the Return_ConstructorInvocationExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly ConstructorInvocationExpressionNode Rent_ConstructorInvocationExpressionNode()
    {
        if (Binder.Pool_ConstructorInvocationExpressionNode_Queue.TryDequeue(out var constructorInvocationExpressionNode))
        {
            return constructorInvocationExpressionNode;
        }

        return new ConstructorInvocationExpressionNode(
            newKeywordToken: default,
            typeReference: default,
            openParenthesisToken: default,
            indexFunctionParameterEntryList: -1,
            countFunctionParameterEntryList: 0,
            closeParenthesisToken: default);
    }
    
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned ConstructorInvocationExpressionNode instance's:
    /// - NewKeywordToken
    /// Thus, the Return_ConstructorInvocationExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_ConstructorInvocationExpressionNode(ConstructorInvocationExpressionNode constructorInvocationExpressionNode)
    {
        constructorInvocationExpressionNode.ResultTypeReference = default;
        
        constructorInvocationExpressionNode.OpenParenthesisToken = default;
        constructorInvocationExpressionNode.IndexFunctionParameterEntryList = -1;
        constructorInvocationExpressionNode.CountFunctionParameterEntryList = 0;
        constructorInvocationExpressionNode.CloseParenthesisToken = default;
    
        constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Unset;
    
        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //constructorInvocationExpressionNode._isFabricated = false;
        
        constructorInvocationExpressionNode.IsParsingFunctionParameters = false;
        
        Binder.Pool_ConstructorInvocationExpressionNode_Queue.Enqueue(constructorInvocationExpressionNode);
    }
    
    public readonly ICodeBlockOwner? GetParent(
        ICodeBlockOwner codeBlockOwner,
        Walk.CompilerServices.CSharp.CompilerServiceCase.CSharpCompilationUnit cSharpCompilationUnit)
    {
        if (codeBlockOwner.Unsafe_ParentIndexKey == -1)
            return null;
            
        return Binder.CodeBlockOwnerList[Compilation.IndexCodeBlockOwnerList + codeBlockOwner.Unsafe_ParentIndexKey];
    }
    
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
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
                SyntaxKind.DiscardSymbol,
                GetNextSymbolId(),
                identifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None,
                }));
        ++Compilation.CountSymbolList;
    }
    
    public void BindFunctionDefinitionNode(FunctionDefinitionNode functionDefinitionNode)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
            SyntaxKind.FunctionSymbol,
            GetNextSymbolId(),
            functionDefinitionNode.FunctionIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Function
            }));
        ++Compilation.CountSymbolList;
    }
    
    public readonly void BindNamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
    {
        var namespaceContributionEntry = new NamespaceContributionEntry(namespaceStatementNode.IdentifierToken.TextSpan);
        Binder.NamespaceContributionList.Add(namespaceContributionEntry);
        ++Compilation.CountNamespaceContributionList;

        var findTuple = Binder.NamespaceGroup_FindRange(namespaceContributionEntry);

        var foundGroup = false;

        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var targetGroup = Binder._namespaceGroupList[i];

            if (targetGroup.NamespaceStatementNodeList.Count > 0)
            {
                var sampleNamespaceStatementNode = targetGroup.NamespaceStatementNodeList[0];
                
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                    ResourceUri.Value,
                    namespaceStatementNode.IdentifierToken.TextSpan,
                    sampleNamespaceStatementNode.ResourceUri.Value,
                    sampleNamespaceStatementNode.IdentifierToken.TextSpan))
                {
                    targetGroup.NamespaceStatementNodeList.Add(namespaceStatementNode);
                    foundGroup = true;
                }
            }
        }

        if (!foundGroup)
        {
            Binder._namespaceGroupList.Add(new NamespaceGroup(
                namespaceStatementNode.IdentifierToken.TextSpan.CharIntSum,
                new List<NamespaceStatementNode> { namespaceStatementNode }));
        }
    }
    
    public void BindVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode, bool shouldCreateVariableSymbol = true)
    {
        if (shouldCreateVariableSymbol)
            CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind);
        
        var text = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, variableDeclarationNode.IdentifierToken.TextSpan);
        if (text is null)
            return;
            
        if (TryGetVariableDeclarationNodeByScope(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                ResourceUri,
                variableDeclarationNode.IdentifierToken.TextSpan,
                out var existingVariableDeclarationNode))
        {
            if (existingVariableDeclarationNode is null || existingVariableDeclarationNode.IsFabricated)
            {
                // Overwrite the fabricated definition with a real one
                //
                // TODO: Track one or many declarations?...
                // (if there is an error where something is defined twice for example)
                SetVariableDeclarationNodeByScope(
                    text,
                    variableDeclarationNode);
            }

            /*Binder.DiagnosticList.Insert(
                Compilation.IndexDiagnosticList + Compilation.CountDiagnosticList,
                ...);
            Compilation.CountDiagnosticList++;
            
            DiagnosticHelper.ReportAlreadyDefinedVariable(
                Compilation.__DiagnosticList,
                variableDeclarationNode.IdentifierToken.TextSpan,
                text);*/
        }
        else
        {
            _ = TryAddVariableDeclarationNodeByScope(
                text,
                variableDeclarationNode);
        }
    }
    
    public void BindLabelDeclarationNode(LabelDeclarationNode labelDeclarationNode)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
                SyntaxKind.LabelSymbol,
                GetNextSymbolId(),
                labelDeclarationNode.IdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None
                }));
        ++Compilation.CountSymbolList;
    
        if (TryGetLabelDeclarationNodeByScope(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                ResourceUri,
                labelDeclarationNode.IdentifierToken.TextSpan,
                out var existingLabelDeclarationNode))
        {
            if (existingLabelDeclarationNode.IsFabricated)
            {
                // Overwrite the fabricated definition with a real one
                //
                // TODO: Track one or many declarations?...
                // (if there is an error where something is defined twice for example)
                SetLabelDeclarationNodeByScope(
                    CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    labelDeclarationNode.IdentifierToken.TextSpan,
                    labelDeclarationNode);
            }

            /*Binder.DiagnosticList.Insert(
                Compilation.IndexDiagnosticList + Compilation.CountDiagnosticList,
                ...);
            Compilation.CountDiagnosticList++;

            DiagnosticHelper.ReportAlreadyDefinedLabel(
                Compilation.__DiagnosticList,
                labelDeclarationNode.IdentifierToken.TextSpan,
                text);*/
        }
        else
        {
            _ = TryAddLabelDeclarationNodeByScope(
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                labelDeclarationNode.IdentifierToken.TextSpan,
                labelDeclarationNode);
        }
    }

    public VariableReferenceNode ConstructAndBindVariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        bool shouldCreateSymbol = true)
    {
        VariableReferenceNode? variableReferenceNode;

        if (TryGetVariableDeclarationHierarchically(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                ResourceUri,
                variableIdentifierToken.TextSpan,
                out var variableDeclarationNode)
            && variableDeclarationNode is not null)
        {
            variableReferenceNode = Rent_VariableReferenceNode();
            variableReferenceNode.VariableIdentifierToken = variableIdentifierToken;
            variableReferenceNode.VariableDeclarationNode = variableDeclarationNode;
        }
        else
        {
            variableDeclarationNode = new VariableDeclarationNode(
                CSharpFacts.Types.Var.ToTypeReference(),
                variableIdentifierToken,
                VariableKind.Local,
                false,
                ResourceUri)
            {
                IsFabricated = true,
            };

            variableReferenceNode = Rent_VariableReferenceNode();
            variableReferenceNode.VariableIdentifierToken = variableIdentifierToken;
            variableReferenceNode.VariableDeclarationNode = variableDeclarationNode;
        }

        if (shouldCreateSymbol)
            CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, variableDeclarationNode.VariableKind);
            
        return variableReferenceNode;
    }
    
    public void BindLabelReferenceNode(LabelReferenceNode labelReferenceNode)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
                SyntaxKind.LabelSymbol,
                GetNextSymbolId(),
                labelReferenceNode.IdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.None
                }));
        ++Compilation.CountSymbolList;
    }

    public void BindConstructorDefinitionIdentifierToken(SyntaxToken identifierToken)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
            SyntaxKind.ConstructorSymbol,
            GetNextSymbolId(),
            identifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Type
            }));
        ++Compilation.CountSymbolList;
    }

    public void BindFunctionInvocationNode(FunctionInvocationNode functionInvocationNode)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
            SyntaxKind.FunctionSymbol,
            GetNextSymbolId(),
            functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.Function
            }));
        ++Compilation.CountSymbolList;

        if (TryGetFunctionHierarchically(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                ResourceUri,
                functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan,
                out var functionDefinitionNode) &&
            functionDefinitionNode is not null)
        {
            functionInvocationNode.ResultTypeReference = functionDefinitionNode.ReturnTypeReference;
        }
    }

    public void BindNamespaceReference(SyntaxToken namespaceIdentifierToken)
    {
        Binder.SymbolList.Insert(
            Compilation.IndexSymbolList + Compilation.CountSymbolList,
            new Symbol(
            SyntaxKind.NamespaceSymbol,
            GetNextSymbolId(),
            namespaceIdentifierToken.TextSpan with
            {
                DecorationByte = (byte)GenericDecorationKind.None
            }));
        ++Compilation.CountSymbolList;
    }

    public void BindTypeClauseNode(TypeClauseNode typeClauseNode)
    {
        if (!typeClauseNode.IsKeywordType)
        {
            Binder.SymbolList.Insert(
                Compilation.IndexSymbolList + Compilation.CountSymbolList,
                new Symbol(
                SyntaxKind.TypeSymbol,
                GetNextSymbolId(),
                typeClauseNode.TypeIdentifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.Type
                }));
            ++Compilation.CountSymbolList;
        }
    }
    
    public void BindTypeIdentifier(SyntaxToken identifierToken)
    {
        if (identifierToken.SyntaxKind == SyntaxKind.IdentifierToken)
        {
            Binder.SymbolList.Insert(
                Compilation.IndexSymbolList + Compilation.CountSymbolList,
                new Symbol(
                SyntaxKind.TypeSymbol,
                GetNextSymbolId(),
                identifierToken.TextSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.Type
                }));
            ++Compilation.CountSymbolList;
        }
    }

    public readonly void BindUsingStatementTuple(SyntaxToken usingKeywordToken, SyntaxToken namespaceIdentifierToken)
    {
        AddNamespaceToCurrentScope(namespaceIdentifierToken.TextSpan);
    }
    
    public readonly void BindTypeDefinitionNode(TypeDefinitionNode typeDefinitionNode, bool shouldOverwrite = false)
    {
        var typeIdentifierText = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, typeDefinitionNode.TypeIdentifierToken.TextSpan);
        var currentNamespaceStatementText = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, CurrentNamespaceStatementNode.IdentifierToken.TextSpan);
            
        if (typeIdentifierText is null || currentNamespaceStatementText is null)
        {
            return;
        }

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

        codeBlockOwner.Unsafe_SelfIndexKey = Compilation.CountCodeBlockOwnerList;
        Binder.CodeBlockOwnerList.Insert(
            Compilation.IndexCodeBlockOwnerList + Compilation.CountCodeBlockOwnerList,
            codeBlockOwner);
        ++Compilation.CountCodeBlockOwnerList;

        var parent = GetParent(codeBlockOwner, Compilation);
        
        var parentScopeDirection = parent?.ScopeDirectionKind ?? ScopeDirectionKind.Both;
        if (parentScopeDirection == ScopeDirectionKind.Both)
            codeBlockOwner.PermitCodeBlockParsing = false;
    
        CurrentCodeBlockOwner = codeBlockOwner;
        
        OnBoundScopeCreatedAndSetAsCurrent(codeBlockOwner, Compilation);
    }

    /// <summary>(inclusive, exclusive, this is the index at which you'd insert the text span)</summary>
    public readonly (int StartIndex, int EndIndex, int InsertionIndex) AddedNamespaceList_FindRange(NamespaceContributionEntry namespaceContributionEntry)
    {
        var startIndex = -1;
        var endIndex = -1;
        var insertionIndex = Binder.CSharpParserModel_AddedNamespaceList.Count;

        for (int i = 0; i < Binder.CSharpParserModel_AddedNamespaceList.Count; i++)
        {
            var node = Binder.CSharpParserModel_AddedNamespaceList[i];

            if (node.CharIntSum == namespaceContributionEntry.TextSpan.CharIntSum)
            {
                if (startIndex == -1)
                    startIndex = i;
            }
            else if (startIndex != -1)
            {
                endIndex = i;
                insertionIndex = i;
                break;
            }
        }

        if (startIndex != -1 && endIndex == -1)
            endIndex = Binder.CSharpParserModel_AddedNamespaceList.Count;

        return (startIndex, endIndex, insertionIndex);
    }

    public readonly void AddNamespaceToCurrentScope(TextEditorTextSpan textSpan)
    {
        var namespaceContributionEntry = new NamespaceContributionEntry(textSpan);

        var findTuple = AddedNamespaceList_FindRange(namespaceContributionEntry);

        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var target = Binder.CSharpParserModel_AddedNamespaceList[i];
            if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                    ResourceUri.Value,
                    textSpan,
                    ResourceUri.Value,
                    target))
            {
                return;
            }
        }
        
        findTuple = Binder.NamespaceGroup_FindRange(namespaceContributionEntry);

        // Absolutely be certain to double check that you did the SafeCompareTextSpans at every step
        // you missed one here.
        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var targetGroup = Binder._namespaceGroupList[i];

            if (targetGroup.NamespaceStatementNodeList.Count > 0)
            {
                var sampleNamespaceStatementNode = targetGroup.NamespaceStatementNodeList[0];
                
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                        ResourceUri.Value,
                        textSpan,
                        sampleNamespaceStatementNode.ResourceUri.Value,
                        sampleNamespaceStatementNode.IdentifierToken.TextSpan))
                {
                    var typeDefinitionNodeList = Binder.Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(targetGroup);
                    foreach (var typeDefinitionNode in typeDefinitionNodeList)
                    {
                        ExternalTypeDefinitionList.Add(typeDefinitionNode);
                    }
                    break;
                }
            }
        }
    }

    public void CloseScope(TextEditorTextSpan textSpan)
    {
        // Check if it is the global scope, if so return early.
        if (CurrentCodeBlockOwner.Unsafe_SelfIndexKey == 0)
            return;
        
        /*if (Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_MinimumLocalsData &&
            (CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
             CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.ArbitraryCodeBlockNode))
        {
            for (int i = Compilation.NodeList.Count - 1; i >= 0; i--)
            {
                if (Compilation.NodeList[i].Unsafe_ParentIndexKey == CurrentCodeBlockOwner.Unsafe_SelfIndexKey)
                    Compilation.NodeList.RemoveAt(i);
            }
        }*/
        
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
                Binder.SymbolList.Insert(
                    Compilation.IndexSymbolList + Compilation.CountSymbolList,
                    new Symbol(
                        SyntaxKind.FieldSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Field
                        }));
                ++Compilation.CountSymbolList;
                break;
            case VariableKind.Property:
                Binder.SymbolList.Insert(
                    Compilation.IndexSymbolList + Compilation.CountSymbolList,
                    new Symbol(
                        SyntaxKind.PropertySymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Property
                        }));
                ++Compilation.CountSymbolList;
                break;
            case VariableKind.EnumMember:
                Binder.SymbolList.Insert(
                    Compilation.IndexSymbolList + Compilation.CountSymbolList,
                    new Symbol(
                        SyntaxKind.EnumMemberSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Property
                        }));
                ++Compilation.CountSymbolList;
                break;
            case VariableKind.Local:
                goto default;
            case VariableKind.Closure:
                goto default;
            default:
                Binder.SymbolList.Insert(
                    Compilation.IndexSymbolList + Compilation.CountSymbolList,
                    new Symbol(
                        SyntaxKind.VariableSymbol,
                        symbolId,
                        identifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Variable
                        }));
                ++Compilation.CountSymbolList;
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
                AddNamespaceToCurrentScope(namespaceStatementNode.IdentifierToken.TextSpan);

                BindNamespaceStatementNode((NamespaceStatementNode)codeBlockOwner);
                return;
            case SyntaxKind.LambdaExpressionNode:
                var lambdaExpressionNode = (LambdaExpressionNode)codeBlockOwner;
                for (int i = lambdaExpressionNode.IndexLambdaExpressionNodeChildList; i < lambdaExpressionNode.IndexLambdaExpressionNodeChildList + lambdaExpressionNode.CountLambdaExpressionNodeChildList; i++)
                {
                    BindVariableDeclarationNode(Binder.LambdaExpressionNodeChildList[i]);
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
                
                if (typeDefinitionNode.IndexGenericParameterEntryList != -1)
                {
                    for (int i = typeDefinitionNode.IndexGenericParameterEntryList;
                         i < typeDefinitionNode.IndexGenericParameterEntryList + typeDefinitionNode.CountGenericParameterEntryList;
                         i++)
                    {
                        var entry = Binder.GenericParameterEntryList[i];
                        
                        BindTypeDefinitionNode(
                            new TypeDefinitionNode(
                                AccessModifierKind.Public,
                                hasPartialModifier: false,
                                StorageModifierKind.Class,
                                entry.TypeReference.TypeIdentifierToken,
                                entry.TypeReference.OpenAngleBracketToken,
                                entry.TypeReference.IndexGenericParameterEntryList,
                                entry.TypeReference.CountGenericParameterEntryList,
                                entry.TypeReference.CloseAngleBracketToken,
                                openParenthesisToken: default,
                                indexFunctionArgumentEntryList: -1,
                                countFunctionArgumentEntryList: 0,
                                closeParenthesisToken: default,
                                inheritedTypeReference: TypeFacts.NotApplicable.ToTypeReference(),
                                string.Empty,
                                ResourceUri));
                    }
                }
                
                return;
        }
    }
    
    /// <summary>
    /// 'definition...' argument name pattern:
    /// This implies the "presumed" value. The invoker must provide a
    /// place to search. This isn't the invoker saying they know where the definition is.
    /// Instead, the invoker is saying, I think the definition is somewhere
    /// in this 'definitionResourceUri', 'defintionCompilationUnit' 'definitionInitialScopeIndexKey'.
    /// 
    /// 'reference...' argument name pattern:
    /// This implies the resourceUri and textSpan combo that prompted you to try getting the respective
    /// TypeDefinitionNode.
    /// These arguments are used to compare character by character the definition's
    /// textSpan to see if they are equal.
    /// 
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public readonly bool TryGetTypeDefinitionHierarchically(
        ResourceUri definitionResourceUri,
        CSharpCompilationUnit definitionCompilationUnit,
        int definitionInitialScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(definitionCompilationUnit, definitionInitialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetTypeDefinitionNodeByScope(
                    definitionResourceUri,
                    definitionCompilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    referenceResourceUri,
                    referenceTextSpan,
                    out typeDefinitionNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
            {
                localScope = Binder.GetScopeByScopeIndexKey(definitionCompilationUnit, localScope.Unsafe_ParentIndexKey);
            }
        }

        typeDefinitionNode = null;
        return false;
    }
    
    public readonly bool TryGetTypeDefinitionNodeByScope(
        ResourceUri definitionResourceUri,
        CSharpCompilationUnit definitionCompilationUnit,
        int definitionScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        typeDefinitionNode = null;
        
        for (int i = definitionCompilationUnit.IndexCodeBlockOwnerList; i < definitionCompilationUnit.IndexCodeBlockOwnerList + definitionCompilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = Binder.CodeBlockOwnerList[i];
            if (x.Unsafe_ParentIndexKey == definitionScopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                        referenceResourceUri.Value, referenceTextSpan, definitionResourceUri.Value, GetIdentifierTextSpan(x)))
                {
                    typeDefinitionNode = (TypeDefinitionNode)x;
                    break;
                }
            }
        }
        
        if (typeDefinitionNode is null)
        {
            if (definitionScopeIndexKey == 0)
            {
                foreach (var externalDefinitionNode in ExternalTypeDefinitionList)
                {
                    if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                            referenceResourceUri.Value, referenceTextSpan, externalDefinitionNode.ResourceUri.Value, GetIdentifierTextSpan(externalDefinitionNode)))
                    {
                        typeDefinitionNode = externalDefinitionNode;
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
    
    public readonly FunctionDefinitionNode[] GetFunctionDefinitionNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        List<FunctionDefinitionNode> functionDefinitionNodeList = new();
    
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var kvp = Binder.CodeBlockOwnerList[i];
            
            if (kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                functionDefinitionNodeList.Add((FunctionDefinitionNode)kvp);
        }
        
        return functionDefinitionNodeList.ToArray();
    }

    /// <summary>
    /// 'definition...' argument name pattern:
    /// This implies the "presumed" value. The invoker must provide a
    /// place to search. This isn't the invoker saying they know where the definition is.
    /// Instead, the invoker is saying, I think the definition is somewhere
    /// in this 'definitionResourceUri', 'defintionCompilationUnit' 'definitionInitialScopeIndexKey'.
    /// 
    /// 'reference...' argument name pattern:
    /// This implies the resourceUri and textSpan combo that prompted you to try getting the respective
    /// TypeDefinitionNode.
    /// These arguments are used to compare character by character the definition's
    /// textSpan to see if they are equal.
    /// 
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public readonly bool TryGetFunctionHierarchically(
        ResourceUri definitionResourceUri,
        CSharpCompilationUnit definitionCompilationUnit,
        int definitionInitialScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out FunctionDefinitionNode? functionDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(definitionCompilationUnit, definitionInitialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetFunctionDefinitionNodeByScope(
                    definitionResourceUri,
                    definitionCompilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    referenceResourceUri,
                    referenceTextSpan,
                    out functionDefinitionNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(definitionCompilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        functionDefinitionNode = null;
        return false;
    }
    
    public readonly bool TryGetFunctionDefinitionNodeByScope(
        ResourceUri definitionResourceUri,
        CSharpCompilationUnit definitionCompilationUnit,
        int definitionScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out FunctionDefinitionNode functionDefinitionNode)
    {
        functionDefinitionNode = null;
        
        for (int i = definitionCompilationUnit.IndexCodeBlockOwnerList; i < definitionCompilationUnit.IndexCodeBlockOwnerList + definitionCompilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = Binder.CodeBlockOwnerList[i];
            
            if (x.Unsafe_ParentIndexKey == definitionScopeIndexKey &&
                x.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
            {
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(referenceResourceUri.Value, referenceTextSpan, definitionResourceUri.Value, GetIdentifierTextSpan(x)))
                {
                    functionDefinitionNode = (FunctionDefinitionNode)x;
                    break;
                }
            }
        }
        
        if (functionDefinitionNode is null)
            return false;
        else
            return true;
    }
    
    public readonly VariableDeclarationNode[] GetVariableDeclarationNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        var variableDeclarationNodeList = new List<VariableDeclarationNode>();
        
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var kvp = Binder.NodeList[i];
            
            if (kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                variableDeclarationNodeList.Add((VariableDeclarationNode)kvp);
        }
        
        return variableDeclarationNodeList.ToArray();
    }

    /// <summary>
    /// 'definition...' argument name pattern:
    /// This implies the "presumed" value. The invoker must provide a
    /// place to search. This isn't the invoker saying they know where the definition is.
    /// Instead, the invoker is saying, I think the definition is somewhere
    /// in this 'definitionResourceUri', 'defintionCompilationUnit' 'definitionInitialScopeIndexKey'.
    /// 
    /// 'reference...' argument name pattern:
    /// This implies the resourceUri and textSpan combo that prompted you to try getting the respective
    /// TypeDefinitionNode.
    /// These arguments are used to compare character by character the definition's
    /// textSpan to see if they are equal.
    /// 
    /// Search hierarchically through all the scopes, starting at the <see cref="_currentScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetVariableDeclarationHierarchically(
        ResourceUri declarationResourceUri,
        CSharpCompilationUnit declarationCompilationUnit,
        int declarationInitialScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out VariableDeclarationNode? variableDeclarationStatementNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(declarationCompilationUnit, declarationInitialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetVariableDeclarationNodeByScope(
                    declarationResourceUri,
                    declarationCompilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    referenceResourceUri,
                    referenceTextSpan,
                    out variableDeclarationStatementNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(declarationCompilationUnit, localScope.Unsafe_ParentIndexKey);
        }
        
        variableDeclarationStatementNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationByPartialType(
        ResourceUri declarationResourceUri,
        CSharpCompilationUnit declarationCompilationUnit,
        int declarationScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        TypeDefinitionNode typeDefinitionNode,
        out VariableDeclarationNode? variableDeclarationNode)
    {
        int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
        while (positionExclusive < Binder.PartialTypeDefinitionList.Count)
        {
            if (Binder.PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
            {
                CSharpCompilationUnit innerCompilationUnit;
                ResourceUri innerResourceUri;
                
                if (Binder.PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
                {
                    if (Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri != declarationResourceUri)
                    {
                        if (Binder.__CompilationUnitMap.TryGetValue(Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
                        {
                            innerCompilationUnit = temporaryCompilationUnit;
                            innerResourceUri = Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri;
                        }
                        else
                        {
                            innerCompilationUnit = default;
                            innerResourceUri = default;
                        }
                    }
                    else
                    {
                        innerCompilationUnit = declarationCompilationUnit;
                        innerResourceUri = declarationResourceUri;
                    }
                    
                    if (!innerCompilationUnit.IsDefault())
                    {
                        var innerScopeIndexKey = Binder.PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                    
                        if (TryGetVariableDeclarationNodeByScope(
                                innerResourceUri,
                                innerCompilationUnit,
                                innerScopeIndexKey,
                                referenceResourceUri,
                                referenceTextSpan,
                                out variableDeclarationNode,
                                isRecursive: true))
                        {
                            return true;
                        }
                    }
                }
                
                positionExclusive++;
            }
            else
            {
                break;
            }
        }
        
        variableDeclarationNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationNodeByScope(
        ResourceUri declarationResourceUri,
        CSharpCompilationUnit declarationCompilationUnit,
        int declarationScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out VariableDeclarationNode? variableDeclarationNode,
        bool isRecursive = false)
    {
        variableDeclarationNode = null;
        for (int i = declarationCompilationUnit.IndexNodeList; i < declarationCompilationUnit.IndexNodeList + declarationCompilationUnit.CountNodeList; i++)
        {
            var node = Binder.NodeList[i];
            
            if (node.Unsafe_ParentIndexKey == declarationScopeIndexKey &&
                node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(
                        referenceResourceUri.Value, referenceTextSpan, declarationResourceUri.Value, GetIdentifierTextSpan(node)))
                {
                    variableDeclarationNode = (VariableDeclarationNode)node;
                    break;
                }
            }
        }
        
        if (variableDeclarationNode is null)
        {
            var codeBlockOwner = Binder.CodeBlockOwnerList[declarationCompilationUnit.IndexCodeBlockOwnerList + declarationScopeIndexKey];
            
            if (!isRecursive && codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var typeDefinitionNode = (TypeDefinitionNode)codeBlockOwner;
                if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
                {
                    if (TryGetVariableDeclarationByPartialType(
                            declarationResourceUri,
                            declarationCompilationUnit,
                            declarationScopeIndexKey,
                            referenceResourceUri,
                            referenceTextSpan,
                            typeDefinitionNode,
                            out variableDeclarationNode))
                    {
                        return true;
                    }
                }
                
                if (typeDefinitionNode.InheritedTypeReference != default)
                {
                    TextEditorTextSpan typeDefinitionTextSpan;
                    CSharpCompilationUnit typeDefinitionCompilationUnit;
                    ResourceUri typeDefinitionResourceUri;
                    
                    if (typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri == referenceResourceUri)
                    {
                        typeDefinitionCompilationUnit = Compilation;
                        typeDefinitionResourceUri = referenceResourceUri;
                        typeDefinitionTextSpan = typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan;
                    }
                    else
                    {
                        if (Binder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri, out typeDefinitionCompilationUnit))
                        {
                            typeDefinitionTextSpan = typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan;
                            typeDefinitionResourceUri = typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri;
                        }
                        else
                        {
                            typeDefinitionTextSpan = default;
                            typeDefinitionResourceUri = default;
                        }
                    }

                    var typeDefinitionScopeIndexKey = Binder.GetScopeByPositionIndex(typeDefinitionCompilationUnit, typeDefinitionTextSpan.StartInclusiveIndex);

                    if (typeDefinitionScopeIndexKey is not null)
                    {
                        if (TryGetTypeDefinitionHierarchically(
                                typeDefinitionResourceUri,
                                typeDefinitionCompilationUnit,
                                typeDefinitionScopeIndexKey.Unsafe_SelfIndexKey,
                                typeDefinitionResourceUri,
                                typeDefinitionTextSpan,
                                out var inheritedTypeDefinitionNode))
                        {
                            if (inheritedTypeDefinitionNode is not null)
                            {
                                var innerScopeIndexKey = inheritedTypeDefinitionNode.Unsafe_SelfIndexKey;

                                if (TryGetVariableDeclarationNodeByScope(
                                        typeDefinitionResourceUri,
                                        typeDefinitionCompilationUnit,
                                        innerScopeIndexKey,
                                        referenceResourceUri,
                                        referenceTextSpan,
                                        out variableDeclarationNode,
                                        isRecursive: true))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        
            return false;
        }
        else
        {
            return true;
        }
    }
    
    public bool TryAddVariableDeclarationNodeByScope(
        string variableIdentifierText,
        VariableDeclarationNode variableDeclarationNode)
    {
        var scopeIndexKey = CurrentCodeBlockOwner.Unsafe_SelfIndexKey;

        VariableDeclarationNode? matchNode = null;
        for (int i = Compilation.IndexNodeList; i < Compilation.IndexNodeList + Compilation.CountNodeList; i++)
        {
            var x = Binder.NodeList[i];
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == variableIdentifierText.Length)
                {
                    // It was validated that neither CharIntSum is 0 here so removing the checks
                    if (otherTextSpan.CharIntSum == variableDeclarationNode.IdentifierToken.TextSpan.CharIntSum)
                    {
                        if (CompareIdentifierText(x, ResourceUri, Compilation, variableIdentifierText))
                        {
                            matchNode = (VariableDeclarationNode)x;
                            break;
                        }
                    }
                }
            }
        }
        
        if (matchNode is null)
        {
            variableDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            
            Binder.NodeList.Insert(
                Compilation.IndexNodeList + Compilation.CountNodeList,
                variableDeclarationNode);
            ++Compilation.CountNodeList;
            
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public readonly void SetVariableDeclarationNodeByScope(
        string variableIdentifierText,
        VariableDeclarationNode variableDeclarationNode)
    {
        int scopeIndexKey = CurrentCodeBlockOwner.Unsafe_SelfIndexKey;
        
        VariableDeclarationNode? matchNode = null;
        int index = Compilation.IndexNodeList;
        for (; index < Compilation.IndexNodeList + Compilation.CountNodeList; index++)
        {
            var x = Binder.NodeList[index];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == variableIdentifierText.Length)
                {
                    // It was validated that neither CharIntSum is 0 here so removing the checks
                    if (otherTextSpan.CharIntSum == variableDeclarationNode.IdentifierToken.TextSpan.CharIntSum)
                    {
                        if (CompareIdentifierText(x, ResourceUri, Compilation, variableIdentifierText))
                        {
                            matchNode = (VariableDeclarationNode)x;
                            break;
                        }
                    }
                }
            }
        }
        
        if (index != -1)
        {
            variableDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            Binder.NodeList[index] = variableDeclarationNode;
        }
    }
    
    public readonly bool TryGetLabelDeclarationHierarchically(
        ResourceUri declarationResourceUri,
        CSharpCompilationUnit declarationCompilationUnit,
        int declarationScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out LabelDeclarationNode? labelDeclarationNode)
    {
        int initialScopeIndexKey = CurrentCodeBlockOwner.Unsafe_SelfIndexKey;

        var localScope = Binder.GetScopeByScopeIndexKey(Compilation, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetLabelDeclarationNodeByScope(
                    declarationResourceUri,
                    declarationCompilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    referenceResourceUri,
                    referenceTextSpan,
                    out labelDeclarationNode))
            {
                return true;
            }

            if (localScope.Unsafe_ParentIndexKey == -1)
                localScope = default;
            else
                localScope = Binder.GetScopeByScopeIndexKey(Compilation, localScope.Unsafe_ParentIndexKey);
        }
        
        labelDeclarationNode = null;
        return false;
    }
    
    public readonly bool TryGetLabelDeclarationNodeByScope(
        ResourceUri declarationResourceUri,
        CSharpCompilationUnit declarationCompilationUnit,
        int declarationScopeIndexKey,
        ResourceUri referenceResourceUri,
        TextEditorTextSpan referenceTextSpan,
        out LabelDeclarationNode labelDeclarationNode)
    {
        labelDeclarationNode = null;
        for (int i = Compilation.IndexNodeList; i < Compilation.IndexNodeList + Compilation.CountNodeList; i++)
        {
            var x = Binder.NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == declarationScopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode)
            {
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(referenceResourceUri.Value, referenceTextSpan, declarationResourceUri.Value, GetIdentifierTextSpan(x)))
                {
                    labelDeclarationNode = (LabelDeclarationNode)x;
                    break;
                }
            }
        }
        
        if (labelDeclarationNode is null)
            return false;
        else
            return true;
    }
    
    public bool TryAddLabelDeclarationNodeByScope(
        int scopeIndexKey,
        TextEditorTextSpan labelIdentifierTextSpan,
        LabelDeclarationNode labelDeclarationNode)
    {
        LabelDeclarationNode? matchNode = null;
        for (var i = Compilation.IndexNodeList; i < Compilation.IndexNodeList + Compilation.CountNodeList; i++)
        {
            var x = Binder.NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(ResourceUri.Value, labelIdentifierTextSpan, ResourceUri.Value, GetIdentifierTextSpan(x)))
                {
                    matchNode = (LabelDeclarationNode)x;
                    break;
                }
            }
        }
        
        if (matchNode is null)
        {
            labelDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            
            Binder.NodeList.Insert(
                Compilation.IndexNodeList + Compilation.CountNodeList,
                labelDeclarationNode);
            ++Compilation.CountNodeList;
            
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public readonly void SetLabelDeclarationNodeByScope(
        int scopeIndexKey,
        TextEditorTextSpan referenceTextSpan,
        LabelDeclarationNode labelDeclarationNode)
    {
        LabelDeclarationNode? matchNode = null;
        int index = Compilation.IndexNodeList;
        for (; index < Compilation.IndexNodeList + Compilation.CountNodeList; index++)
        {
            var x = Binder.NodeList[index];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (Binder.CSharpCompilerService.SafeCompareTextSpans(ResourceUri.Value, referenceTextSpan, ResourceUri.Value, GetIdentifierTextSpan(x)))
                {
                    matchNode = (LabelDeclarationNode)x;
                    break;
                }
            }
        }

        if (index != -1)
        {
            labelDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
            Binder.NodeList[index] = labelDeclarationNode;
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
    ///
    /// !!!! TODO: This method reads files other than the one being parsed without a textspan bounds check? This should be changed.
    /// </summary>
    public readonly string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri, CSharpCompilationUnit compilationUnit)
    {
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)node;
                if (typeDefinitionNode.ResourceUri == resourceUri && resourceUri == ResourceUri)
                {
                    return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, typeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty;
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.CSharpCompilerService.SafeGetText(typeDefinitionNode.ResourceUri.Value, typeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty;
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.TypeClauseNode:
            {
                var typeClauseNode = (TypeClauseNode)node;
                if (typeClauseNode.ExplicitDefinitionResourceUri == resourceUri && resourceUri == ResourceUri)
                {
                    return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, typeClauseNode.TypeIdentifierToken.TextSpan) ?? string.Empty;
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(typeClauseNode.ExplicitDefinitionResourceUri, out var innerCompilationUnit))
                        return Binder.CSharpCompilerService.SafeGetText(typeClauseNode.ExplicitDefinitionResourceUri.Value, typeClauseNode.TypeIdentifierToken.TextSpan) ?? string.Empty;
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                if (functionDefinitionNode.ResourceUri == resourceUri && resourceUri == ResourceUri)
                {
                    return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, functionDefinitionNode.FunctionIdentifierToken.TextSpan) ?? string.Empty;
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.CSharpCompilerService.SafeGetText(functionDefinitionNode.ResourceUri.Value, functionDefinitionNode.FunctionIdentifierToken.TextSpan) ?? string.Empty;
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                var functionInvocationNode = (FunctionInvocationNode)node;
                if (functionInvocationNode.ResourceUri == resourceUri && resourceUri == ResourceUri)
                {
                    return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan) ?? string.Empty;
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(functionInvocationNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.CSharpCompilerService.SafeGetText(functionInvocationNode.ResourceUri.Value, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan) ?? string.Empty;
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)node;
                if (variableDeclarationNode.ResourceUri == resourceUri && resourceUri == ResourceUri)
                {
                    return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, variableDeclarationNode.IdentifierToken.TextSpan) ?? string.Empty;
                }
                else
                {
                    if (Binder.__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var innerCompilationUnit))
                        return Binder.CSharpCompilerService.SafeGetText(variableDeclarationNode.ResourceUri.Value, variableDeclarationNode.IdentifierToken.TextSpan) ?? string.Empty;
                    else
                        return string.Empty;
                }
            }
            case SyntaxKind.VariableReferenceNode:
            {
                return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, ((VariableReferenceNode)node).VariableIdentifierToken.TextSpan) ?? string.Empty;
            }
            case SyntaxKind.LabelDeclarationNode:
            {
                return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, ((LabelDeclarationNode)node).IdentifierToken.TextSpan) ?? string.Empty;
            }
            case SyntaxKind.LabelReferenceNode:
            {
                return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, ((LabelReferenceNode)node).IdentifierToken.TextSpan) ?? string.Empty;
            }
            default:
            {
                return string.Empty;
            }
        }
    }
    
    public readonly bool CompareIdentifierText(
        ISyntaxNode node,
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        string identifierText)
    {
        string absolutePathString = resourceUri.Value;
        TextEditorTextSpan textSpan;
        
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)node;
                textSpan = typeDefinitionNode.TypeIdentifierToken.TextSpan;
                absolutePathString = typeDefinitionNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.TypeClauseNode:
            {
                var typeClauseNode = (TypeClauseNode)node;
                textSpan = typeClauseNode.TypeIdentifierToken.TextSpan;
                absolutePathString = typeClauseNode.ExplicitDefinitionResourceUri.Value;
                break;
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                textSpan = functionDefinitionNode.FunctionIdentifierToken.TextSpan;
                absolutePathString = functionDefinitionNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                var functionInvocationNode = (FunctionInvocationNode)node;
                textSpan = functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan;
                absolutePathString = functionInvocationNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)node;
                textSpan = variableDeclarationNode.IdentifierToken.TextSpan;
                absolutePathString = variableDeclarationNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.VariableReferenceNode:
            {
                textSpan = ((VariableReferenceNode)node).VariableIdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            case SyntaxKind.LabelDeclarationNode:
            {
                textSpan = ((LabelDeclarationNode)node).IdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            case SyntaxKind.LabelReferenceNode:
            {
                textSpan = ((LabelReferenceNode)node).IdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            default:
            {
                return false;
            }
        }
        
        return Binder.CSharpCompilerService.SafeCompareText(
            absolutePathString,
            identifierText,
            textSpan);
    }

    public readonly bool CompareTextSpans(
        ISyntaxNode node,
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        TextEditorTextSpan identifierTextSpan)
    {
        string absolutePathString = resourceUri.Value;
        TextEditorTextSpan textSpan;
        
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)node;
                textSpan = typeDefinitionNode.TypeIdentifierToken.TextSpan;
                absolutePathString = typeDefinitionNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.TypeClauseNode:
            {
                var typeClauseNode = (TypeClauseNode)node;
                textSpan = typeClauseNode.TypeIdentifierToken.TextSpan;
                absolutePathString = typeClauseNode.ExplicitDefinitionResourceUri.Value;
                break;
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                textSpan = functionDefinitionNode.FunctionIdentifierToken.TextSpan;
                absolutePathString = functionDefinitionNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                var functionInvocationNode = (FunctionInvocationNode)node;
                textSpan = functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan;
                absolutePathString = functionInvocationNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)node;
                textSpan = variableDeclarationNode.IdentifierToken.TextSpan;
                absolutePathString = variableDeclarationNode.ResourceUri.Value;
                break;
            }
            case SyntaxKind.VariableReferenceNode:
            {
                textSpan = ((VariableReferenceNode)node).VariableIdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            case SyntaxKind.LabelDeclarationNode:
            {
                textSpan = ((LabelDeclarationNode)node).IdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            case SyntaxKind.LabelReferenceNode:
            {
                textSpan = ((LabelReferenceNode)node).IdentifierToken.TextSpan;
                absolutePathString = ResourceUri.Value;
                break;
            }
            default:
            {
                return false;
            }
        }
        
        return Binder.CSharpCompilerService.SafeCompareTextSpans(
            ResourceUri.Value,
            identifierTextSpan,
            absolutePathString,
            textSpan);
    }
    
    public readonly TextEditorTextSpan GetIdentifierTextSpan(ISyntaxNode node)
    {
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
                return ((TypeDefinitionNode)node).TypeIdentifierToken.TextSpan;
            case SyntaxKind.TypeClauseNode:
                return ((TypeClauseNode)node).TypeIdentifierToken.TextSpan;
            case SyntaxKind.FunctionDefinitionNode:
                return ((FunctionDefinitionNode)node).FunctionIdentifierToken.TextSpan;
            case SyntaxKind.FunctionInvocationNode:
                return ((FunctionInvocationNode)node).FunctionInvocationIdentifierToken.TextSpan;
            case SyntaxKind.VariableDeclarationNode:
                return ((VariableDeclarationNode)node).IdentifierToken.TextSpan;
            case SyntaxKind.VariableReferenceNode:
                return ((VariableReferenceNode)node).VariableIdentifierToken.TextSpan;
            case SyntaxKind.LabelDeclarationNode:
                return ((LabelDeclarationNode)node).IdentifierToken.TextSpan;
            case SyntaxKind.LabelReferenceNode:
                return ((LabelReferenceNode)node).IdentifierToken.TextSpan;
            default:
                return default;
        }
    }
    
    public readonly string GetTextSpanText(TextEditorTextSpan textSpan)
    {
        return Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, textSpan) ?? string.Empty;
    }

    public readonly TypeClauseNode ToTypeClause(TypeDefinitionNode typeDefinitionNode)
    {
        var typeClauseNode = Rent_TypeClauseNode();
        typeClauseNode.TypeIdentifierToken = typeDefinitionNode.TypeIdentifierToken;
        typeClauseNode.IsKeywordType = typeDefinitionNode.IsKeywordType;
            
        typeClauseNode.ExplicitDefinitionTextSpan = typeDefinitionNode.TypeIdentifierToken.TextSpan;
        typeClauseNode.ExplicitDefinitionResourceUri = typeDefinitionNode.ResourceUri;
        
        return typeClauseNode;
    }
}
