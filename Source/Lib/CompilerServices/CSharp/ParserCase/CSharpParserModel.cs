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
        
        Binder.CSharpParserModel_AddedNamespaceHashSet.Clear();
        
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
        typeClauseNode._isFabricated = false;
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
        namespaceClauseNode._isFabricated = false;
        
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
        
        functionInvocationNode._isFabricated = false;
        
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
    
        constructorInvocationExpressionNode._isFabricated = false;
        
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
        var namespaceString = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, namespaceStatementNode.IdentifierToken.TextSpan);
        if (namespaceString is null)
            return;
        
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
        
        var text = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, variableDeclarationNode.IdentifierToken.TextSpan);
        if (text is null)
            return;
            
        if (TryGetVariableDeclarationNodeByScope(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                variableDeclarationNode.IdentifierToken.TextSpan,
                out var existingVariableDeclarationNode))
        {
            if (existingVariableDeclarationNode.IsFabricated)
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
    
        var text = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, labelDeclarationNode.IdentifierToken.TextSpan);
        if (text is null)
            return;
        
        if (TryGetLabelDeclarationNodeByScope(
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
                    CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    text,
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
                text,
                labelDeclarationNode.IdentifierToken.TextSpan,
                labelDeclarationNode);
        }
    }

    public VariableReferenceNode ConstructAndBindVariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        bool shouldCreateSymbol = true)
    {
        var text = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, variableIdentifierToken.TextSpan);
        
        VariableReferenceNode? variableReferenceNode;

        if (text is not null && TryGetVariableDeclarationHierarchically(
                ResourceUri,
                Compilation,
                CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
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
        var functionInvocationIdentifierText = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan);
        if (functionInvocationIdentifierText is null)
            return;

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
                functionInvocationIdentifierText,
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
        var text = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, namespaceIdentifierToken.TextSpan);
        if (text is null)
            return;
    
        AddNamespaceToCurrentScope(text);
    }
    
    public readonly void BindTypeDefinitionNode(TypeDefinitionNode typeDefinitionNode, bool shouldOverwrite = false)
    {
        var typeIdentifierText = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, typeDefinitionNode.TypeIdentifierToken.TextSpan);
        var currentNamespaceStatementText = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, CurrentNamespaceStatementNode.IdentifierToken.TextSpan);
            
        if (typeIdentifierText is null || currentNamespaceStatementText is null)
        {
            return;
        }
        
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

    public readonly void AddNamespaceToCurrentScope(string namespaceString)
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
            /*for (int i = Compilation.NodeList.Count - 1; i >= 0; i--)
            {
                if (Compilation.NodeList[i].Unsafe_ParentIndexKey == CurrentCodeBlockOwner.Unsafe_SelfIndexKey)
                    Compilation.NodeList.RemoveAt(i);
            }*/
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
                var namespaceString = Binder.CSharpCompilerService.SafeGetText(ResourceUri.Value, namespaceStatementNode.IdentifierToken.TextSpan);
                if (namespaceString is null)
                    return;
                    
                AddNamespaceToCurrentScope(namespaceString);

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
    
    public readonly void SetOpenCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_StartInclusiveIndex)
    {
        codeBlockOwner.CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
    }
    
    public readonly void SetCloseCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_EndExclusiveIndex)
    {
        codeBlockOwner.CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public readonly bool TryGetTypeDefinitionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        TextEditorTextSpan typeIdentifierTextSpan,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetTypeDefinitionNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    typeIdentifierTextSpan,
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
    
    public readonly bool TryGetTypeDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string typeIdentifierText,
        TextEditorTextSpan typeIdentifierTextSpan,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        typeDefinitionNode = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = Binder.CodeBlockOwnerList[i];
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == typeIdentifierText.Length)
                {
                    if (otherTextSpan.CharIntSum == 0 ||
                        typeIdentifierTextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == typeIdentifierTextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, resourceUri, compilationUnit) == typeIdentifierText)
                        {
                            typeDefinitionNode = (TypeDefinitionNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
                }
            }
        }
        
        if (typeDefinitionNode is null)
        {
            if (scopeIndexKey == 0)
            {
                foreach (var x in ExternalTypeDefinitionList)
                {
                    var otherTextSpan = GetIdentifierTextSpan(x);
                    if (otherTextSpan.Length == typeIdentifierText.Length)
                    {
                        if (otherTextSpan.CharIntSum == 0 ||
                            typeIdentifierTextSpan.CharIntSum == 0 ||
                            otherTextSpan.CharIntSum == typeIdentifierTextSpan.CharIntSum)
                        {
                            if (GetIdentifierText(x, resourceUri, compilationUnit) == typeIdentifierText)
                            {
                                typeDefinitionNode = (TypeDefinitionNode)x;
                                break;
                            }
                        }
                        else
                        {
                            ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
    
    public readonly bool TryAddTypeDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string typeIdentifierText,
        TypeDefinitionNode typeDefinitionNode)
    {
        TypeDefinitionNode? matchNode = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = Binder.CodeBlockOwnerList[i];
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == typeIdentifierText.Length)
                {
                    if (otherTextSpan.CharIntSum == 0 ||
                        typeDefinitionNode.TypeIdentifierToken.TextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == typeDefinitionNode.TypeIdentifierToken.TextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, resourceUri, compilationUnit) == typeIdentifierText)
                        {
                            matchNode = (TypeDefinitionNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
                }
            }
        }
        
        if (matchNode is null)
        {
            Binder.CodeBlockOwnerList.Insert(
                compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList,
                typeDefinitionNode);
            ++compilationUnit.CountCodeBlockOwnerList;
            return true;
        }
        else
        {
            return false;
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
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public readonly bool TryGetFunctionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        TextEditorTextSpan identifierTextSpan,
        out FunctionDefinitionNode? functionDefinitionNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetFunctionDefinitionNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    identifierTextSpan,
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
    
    public readonly bool TryGetFunctionDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string functionIdentifierText,
        TextEditorTextSpan functionIdentifierTextSpan,
        out FunctionDefinitionNode functionDefinitionNode)
    {
        functionDefinitionNode = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = Binder.CodeBlockOwnerList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == functionIdentifierText.Length)
                {
                    if (otherTextSpan.CharIntSum == 0 ||
                        functionIdentifierTextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == functionIdentifierTextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, resourceUri, compilationUnit) == functionIdentifierText)
                        {
                            functionDefinitionNode = (FunctionDefinitionNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
    /// Search hierarchically through all the scopes, starting at the <see cref="_currentScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetVariableDeclarationHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        TextEditorTextSpan identifierTextSpan,
        out VariableDeclarationNode? variableDeclarationStatementNode)
    {
        var localScope = Binder.GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetVariableDeclarationNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
                    identifierTextSpan,
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
    
    public bool TryGetVariableDeclarationByPartialType(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        TextEditorTextSpan identifierTextSpan,
        TypeDefinitionNode typeDefinitionNode,
        out VariableDeclarationNode variableDeclarationNode)
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
                    if (Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri != resourceUri)
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
                        innerCompilationUnit = compilationUnit;
                        innerResourceUri = resourceUri;
                    }
                    
                    if (!innerCompilationUnit.IsDefault())
                    {
                        var innerScopeIndexKey = Binder.PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                    
                        if (TryGetVariableDeclarationNodeByScope(
                                innerResourceUri,
                                innerCompilationUnit,
                                innerScopeIndexKey,
                                variableIdentifierText,
                                identifierTextSpan,
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
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        TextEditorTextSpan variableIdentifierTextSpan,
        out VariableDeclarationNode? variableDeclarationNode,
        bool isRecursive = false)
    {
        variableDeclarationNode = null;
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var x = Binder.NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                var otherTextSpan = GetIdentifierTextSpan(x);
                if (otherTextSpan.Length == variableIdentifierText.Length)
                {
                    if (otherTextSpan.CharIntSum == 0 ||
                        variableIdentifierTextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == variableIdentifierTextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, resourceUri, compilationUnit) == variableIdentifierText)
                        {
                            variableDeclarationNode = (VariableDeclarationNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
                }
            }
        }
        
        if (variableDeclarationNode is null)
        {
            var codeBlockOwner = Binder.CodeBlockOwnerList[compilationUnit.IndexCodeBlockOwnerList + scopeIndexKey];
            
            if (!isRecursive && codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var typeDefinitionNode = (TypeDefinitionNode)codeBlockOwner;
                if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
                {
                    if (TryGetVariableDeclarationByPartialType(
                        resourceUri,
                        compilationUnit,
                        scopeIndexKey,
                        variableIdentifierText,
                        variableIdentifierTextSpan,
                        typeDefinitionNode,
                        out variableDeclarationNode))
                    {
                        return true;
                    }
                }
                
                if (typeDefinitionNode.InheritedTypeReference != default)
                {
                    string? identifierText;
                    TextEditorTextSpan identifierTextSpan;
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri == resourceUri)
                    {
                        innerCompilationUnit = Compilation;
                        innerResourceUri = resourceUri;
                        identifierTextSpan = typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan;
                        identifierText = Binder.CSharpCompilerService.SafeGetText(
                            innerResourceUri.Value,
                            typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                    }
                    else
                    {
                        if (Binder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri, out innerCompilationUnit))
                        {
                            identifierTextSpan = typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan;
                            identifierText = Binder.CSharpCompilerService.SafeGetText(
                                typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri.Value,
                                typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                            innerResourceUri = typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri;
                        }
                        else
                        {
                            identifierTextSpan = default;
                            identifierText = null;
                            innerResourceUri = default;
                        }
                    }
                
                    if (identifierText is not null)
                    {
                        if (TryGetTypeDefinitionHierarchically(
                                resourceUri,
                                compilationUnit,
                                scopeIndexKey,
                                identifierText,
                                identifierTextSpan,
                                out var inheritedTypeDefinitionNode))
                        {
                            var innerScopeIndexKey = inheritedTypeDefinitionNode.Unsafe_SelfIndexKey;
                    
                            if (TryGetVariableDeclarationNodeByScope(
                                    innerResourceUri,
                                    innerCompilationUnit,
                                    innerScopeIndexKey,
                                    variableIdentifierText,
                                    identifierTextSpan,
                                    out variableDeclarationNode,
                                    isRecursive: true))
                            {
                                return true;
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
                    if (otherTextSpan.CharIntSum == 0 ||
                        variableDeclarationNode.IdentifierToken.TextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == variableDeclarationNode.IdentifierToken.TextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, ResourceUri, Compilation) == variableIdentifierText)
                        {
                            matchNode = (VariableDeclarationNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
                if (GetIdentifierTextSpan(x).Length == variableIdentifierText.Length)
                {
                    if (GetIdentifierText(x, ResourceUri, Compilation) == variableIdentifierText)
                    {
                        matchNode = (VariableDeclarationNode)x;
                        break;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
        string identifierText,
        out LabelDeclarationNode? labelDeclarationNode)
    {
        int initialScopeIndexKey = CurrentCodeBlockOwner.Unsafe_SelfIndexKey;

        var localScope = Binder.GetScopeByScopeIndexKey(Compilation, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetLabelDeclarationNodeByScope(
                    localScope.Unsafe_SelfIndexKey,
                    identifierText,
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
        int scopeIndexKey,
        string labelIdentifierText,
        out LabelDeclarationNode labelDeclarationNode)
    {
        labelDeclarationNode = null;
        for (int i = Compilation.IndexNodeList; i < Compilation.IndexNodeList + Compilation.CountNodeList; i++)
        {
            var x = Binder.NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode)
            {
                if (GetIdentifierTextSpan(x).Length == labelIdentifierText.Length)
                {
                    if (GetIdentifierText(x, ResourceUri, Compilation) == labelIdentifierText)
                    {
                        labelDeclarationNode = (LabelDeclarationNode)x;
                        break;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
        string labelIdentifierText,
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
                if (otherTextSpan.Length == labelIdentifierText.Length)
                {
                    if (otherTextSpan.CharIntSum == 0 ||
                        labelIdentifierTextSpan.CharIntSum == 0 ||
                        otherTextSpan.CharIntSum == labelIdentifierTextSpan.CharIntSum)
                    {
                        if (GetIdentifierText(x, ResourceUri, Compilation) == labelIdentifierText)
                        {
                            matchNode = (LabelDeclarationNode)x;
                            break;
                        }
                    }
                    else
                    {
                        ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingCharIntSum;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
        string labelIdentifierText,
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
                if (GetIdentifierTextSpan(x).Length == labelIdentifierText.Length)
                {
                    if (GetIdentifierText(x, ResourceUri, Compilation) == labelIdentifierText)
                    {
                        matchNode = (LabelDeclarationNode)x;
                        break;
                    }
                }
                else
                {
                    ++Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AvoidStringLogicByCheckingLength;
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
