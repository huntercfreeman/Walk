using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.CompilerServices.CSharp.Facts;
using Walk.CompilerServices.CSharp.ParserCase;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.BinderCase;

public class CSharpBinder
{
    /// <summary>
    /// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
    ///
    /// TODO: Don't have this be public
    /// </summary>
    public readonly Dictionary<string, NamespaceGroup> _namespaceGroupMap = CSharpFacts.Namespaces.GetInitialBoundNamespaceStatementNodes();
    /// <summary>
    /// TODO: Don't have this be public
    /// </summary>
    public readonly Dictionary<string, TypeDefinitionNode> _allTypeDefinitions = new();
    private readonly NamespaceStatementNode _topLevelNamespaceStatementNode = CSharpFacts.Namespaces.GetTopLevelNamespaceStatementNode();
    
    public List<PartialTypeDefinitionEntry> PartialTypeDefinitionList { get; } = new();
    public List<MethodOverloadDefinitionEntry> MethodOverloadDefinitionList { get; } = new();
    public bool MethodOverload_ResourceUri_WasCleared { get; set; }
    
    /// <summary>
    /// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
    /// </summary>
    public IReadOnlyDictionary<string, NamespaceGroup> NamespaceGroupMap => _namespaceGroupMap;
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions => _allTypeDefinitions;
    
    /// <summary>
    /// CONFUSING: During a parse the "previous" CSharpCompilationUnit gets read from here...
    /// ...because the currently being parsed CSharpCompilationUnit has not been written to this map yet.
    /// </summary>
    public Dictionary<ResourceUri, CSharpCompilationUnit> __CompilationUnitMap { get; } = new();
    
    public NamespacePrefixTree NamespacePrefixTree { get; } = new();
    
    public NamespaceStatementNode TopLevelNamespaceStatementNode => _topLevelNamespaceStatementNode;
    
    public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> CSharpParserModel_ParseChildScopeStack { get; } = new();
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode? ExpressionNode)> CSharpParserModel_ExpressionList { get; set; } = new();
    public List<SyntaxKind> CSharpParserModel_TryParseExpressionSyntaxKindList { get; } = new();
    public HashSet<string> CSharpParserModel_ClearedPartialDefinitionHashSet { get; } = new();
    public List<TypeDefinitionNode> CSharpParserModel_ExternalTypeDefinitionList { get; } = new();
    public CSharpStatementBuilder CSharpParserModel_StatementBuilder { get; } = new();
    
    public TokenWalker CSharpParserModel_TokenWalker { get; } = new(Array.Empty<SyntaxToken>(), useDeferredParsing: true);
    
    /// <summary>
    /// This is cleared at the start of a new parse, inside the CSharpParserModel constructor.
    /// </summary>
    public HashSet<string> CSharpParserModel_AddedNamespaceHashSet { get; } = new();
    
    public List<GenericParameterEntry> GenericParameterEntryList { get; } = new();
    public List<FunctionParameterEntry> FunctionParameterEntryList { get; } = new();
    public List<FunctionArgumentEntry> FunctionArgumentEntryList { get; } = new();
    public List<ISyntaxNode> AmbiguousParenthesizedExpressionNodeChildList { get; } = new();
    public List<VariableDeclarationNode> LambdaExpressionNodeChildList { get; } = new();
    public List<FunctionInvocationParameterMetadata> FunctionInvocationParameterMetadataList { get; } = new();
    public Dictionary<string, Dictionary<int, (ResourceUri ResourceUri, int StartInclusiveIndex)>> SymbolIdToExternalTextSpanMap { get; } = new();
    public List<Walk.TextEditor.RazorLib.CompilerServices.TextEditorDiagnostic> DiagnosticList { get; } = new();
    public List<Symbol> SymbolList { get; } = new();
    public List<ISyntaxNode> NodeList { get; } = new();
    public List<ICodeBlockOwner> CodeBlockOwnerList { get; } = new();
    
    internal const int POOL_TYPE_CLAUSE_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<TypeClauseNode> Pool_TypeClauseNode_Queue = new();
    
    internal const int POOL_VARIABLE_REFERENCE_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<VariableReferenceNode> Pool_VariableReferenceNode_Queue = new();
    
    // When parsing Walk.sln solution wide:
    //
    // Count of 3 => Pool_NamespaceClauseNode_%: 81.5%
    // Count of 4 => Pool_NamespaceClauseNode_%: 89.9%
    // Count of 5 => Pool_NamespaceClauseNode_%: 97.9%
    // Count of 6 => Pool_NamespaceClauseNode_%: 98.0%
    // Count of 7 => Pool_NamespaceClauseNode_%: 98.1%
    // Count of 8 => Pool_NamespaceClauseNode_%: 98.2%
    // Count of 9 => Pool_NamespaceClauseNode_%: 98.2%
    // Count of 18 => Pool_NamespaceClauseNode_%: 98.8%
    // Count of 50 => Pool_NamespaceClauseNode_%: 100.0%
    // Count of 25 => Pool_NamespaceClauseNode_%: 99.3%
    //
    // Likely you don't actually have 25 namespace clause nodes one after another.
    //
    // Probably just have an edge case that results in no return, but it is occurring so minimally
    // that you can just increase the count in the pool and observe a % hit increase that is oddly high.
    //
    // I don't commonly use explicit namespace qualification.
    // Thus I would probably pick '5' since that is where the value slows down greatly.
    //
    // But, I also might not have a fair measurement of what the impact of '6' would be,
    // due to me not commonly using explicit namespace qualification.
    //
    // Therefore, I'll go 1 higher than what I'd pick.
    // So, I'd pick '5' thus go 1 higher and pick '6'.
    // 
    internal const int POOL_NAMESPACE_CLAUSE_NODE_MAX_COUNT = 6;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<NamespaceClauseNode> Pool_NamespaceClauseNode_Queue = new();
    
    internal const int POOL_AMBIGUOUS_IDENTIFIER_EXPRESSION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<AmbiguousIdentifierExpressionNode> Pool_AmbiguousIdentifierExpressionNode_Queue = new();
    
    internal const int POOL_CONSTRUCTOR_INVOCATION_EXPRESSION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<ConstructorInvocationExpressionNode> Pool_ConstructorInvocationExpressionNode_Queue = new();
    
    internal const int POOL_FUNCTION_INVOCATION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<FunctionInvocationNode> Pool_FunctionInvocationNode_Queue = new();
        
    public BadExpressionNode Shared_BadExpressionNode { get; } = new BadExpressionNode(
        CSharpFacts.Types.Void.ToTypeReference(),
        EmptyExpressionNode.Empty,
        EmptyExpressionNode.Empty);
    
    public TextEditorService TextEditorService { get; set; }
    public CSharpCompilerService CSharpCompilerService { get; set; }
    
    public GlobalCodeBlockNode GlobalCodeBlockNode { get; } = new GlobalCodeBlockNode();
    
    public CSharpBinder(TextEditorService textEditorService, CSharpCompilerService cSharpCompilerService)
    {
        TextEditorService = textEditorService;
        CSharpCompilerService = cSharpCompilerService;
        
        _allTypeDefinitions.Add("void", CSharpFacts.Types.Void);
        _allTypeDefinitions.Add("int", CSharpFacts.Types.Int);
        _allTypeDefinitions.Add("char", CSharpFacts.Types.Char);
        _allTypeDefinitions.Add("string", CSharpFacts.Types.String);
        _allTypeDefinitions.Add("bool", CSharpFacts.Types.Bool);
        _allTypeDefinitions.Add("var", CSharpFacts.Types.Var);
    
        for (int i = 0; i < POOL_TYPE_CLAUSE_NODE_MAX_COUNT; i++)
        {
            Pool_TypeClauseNode_Queue.Enqueue(new TypeClauseNode(
                typeIdentifier: default,
                openAngleBracketToken: default,
        		indexGenericParameterEntryList: -1,
                countGenericParameterEntryList: 0,
        		closeAngleBracketToken: default,
                isKeywordType: false));
        }
        
        for (int i = 0; i < POOL_VARIABLE_REFERENCE_NODE_MAX_COUNT; i++)
        {
            Pool_VariableReferenceNode_Queue.Enqueue(new VariableReferenceNode(
                variableIdentifierToken: default,
                variableDeclarationNode: default));
        }
        
        for (int i = 0; i < POOL_NAMESPACE_CLAUSE_NODE_MAX_COUNT; i++)
        {
            Pool_NamespaceClauseNode_Queue.Enqueue(new NamespaceClauseNode(
                identifierToken: default));
        }
        
        for (int i = 0; i < POOL_AMBIGUOUS_IDENTIFIER_EXPRESSION_NODE_MAX_COUNT; i++)
        {
            Pool_AmbiguousIdentifierExpressionNode_Queue.Enqueue(new AmbiguousIdentifierExpressionNode(
                token: default,
                openAngleBracketToken: default,
                indexGenericParameterEntryList: -1,
                countGenericParameterEntryList: 0,
                closeAngleBracketToken: default,
                resultTypeReference: CSharpFacts.Types.Void.ToTypeReference()));
        }
        
        for (int i = 0; i < POOL_FUNCTION_INVOCATION_NODE_MAX_COUNT; i++)
        {
            Pool_FunctionInvocationNode_Queue.Enqueue(new FunctionInvocationNode(
                functionInvocationIdentifierToken: default,        
                openAngleBracketToken: default,
                indexGenericParameterEntryList: -1,
                countGenericParameterEntryList: 0,
                closeAngleBracketToken: default,
                openParenthesisToken: default,
                indexFunctionParameterEntryList: -1,
                countFunctionParameterEntryList: 0,
                closeParenthesisToken: default,
                resultTypeReference: CSharpFacts.Types.Void.ToTypeReference()));
        }
        
        for (int i = 0; i < POOL_CONSTRUCTOR_INVOCATION_EXPRESSION_NODE_MAX_COUNT; i++)
        {
            Pool_ConstructorInvocationExpressionNode_Queue.Enqueue(new ConstructorInvocationExpressionNode(
                newKeywordToken: default,
                typeReference: default,
                openParenthesisToken: default,
                indexFunctionParameterEntryList: -1,
                countFunctionParameterEntryList: 0,
                closeParenthesisToken: default));
        }
        
        Task.Run(async () =>
        {
            await Task.Delay(25_000);
            
            Console.WriteLine($"Pool_NamespaceClauseNode_Hit: {CSharpParserModel.Pool_NamespaceClauseNode_Hit}");
            Console.WriteLine($"Pool_NamespaceClauseNode_Miss: {CSharpParserModel.Pool_NamespaceClauseNode_Miss}");
            Console.WriteLine($"Pool_NamespaceClauseNode_Return: {CSharpParserModel.Pool_NamespaceClauseNode_Return}");
            Console.WriteLine($"Pool_NamespaceClauseNode_%: {((double)CSharpParserModel.Pool_NamespaceClauseNode_Hit / (CSharpParserModel.Pool_NamespaceClauseNode_Hit + CSharpParserModel.Pool_NamespaceClauseNode_Miss)):P1}");
        });
    }
    
    /// <summary><see cref="FinalizeCompilationUnit"/></summary>
    public void StartCompilationUnit(ResourceUri resourceUri)
    {
        foreach (var namespaceGroupNodeKvp in _namespaceGroupMap)
        {
            for (int i = namespaceGroupNodeKvp.Value.NamespaceStatementNodeList.Count - 1; i >= 0; i--)
            {
                if (namespaceGroupNodeKvp.Value.NamespaceStatementNodeList[i].ResourceUri == resourceUri)
                    namespaceGroupNodeKvp.Value.NamespaceStatementNodeList.RemoveAt(i);
            }
        }
    }

    /// <summary><see cref="StartCompilationUnit"/></summary>
    public void FinalizeCompilationUnit(ResourceUri resourceUri, CSharpCompilationUnit compilationUnit)
    {
        UpsertCompilationUnit(resourceUri, compilationUnit);
        
        while (Pool_TypeClauseNode_Queue.Count > POOL_TYPE_CLAUSE_NODE_MAX_COUNT)
        {
            _ = Pool_TypeClauseNode_Queue.Dequeue();
        }
        
        while (Pool_VariableReferenceNode_Queue.Count > POOL_VARIABLE_REFERENCE_NODE_MAX_COUNT)
        {
            _  = Pool_VariableReferenceNode_Queue.Dequeue();
        }
        
        while (Pool_NamespaceClauseNode_Queue.Count > POOL_NAMESPACE_CLAUSE_NODE_MAX_COUNT)
        {
            _  = Pool_NamespaceClauseNode_Queue.Dequeue();
        }
        
        while (Pool_AmbiguousIdentifierExpressionNode_Queue.Count > POOL_AMBIGUOUS_IDENTIFIER_EXPRESSION_NODE_MAX_COUNT)
        {
            _  = Pool_AmbiguousIdentifierExpressionNode_Queue.Dequeue();
        }
        
        while (Pool_FunctionInvocationNode_Queue.Count > POOL_FUNCTION_INVOCATION_NODE_MAX_COUNT)
        {
            _  = Pool_FunctionInvocationNode_Queue.Dequeue();
        }
        
        while (Pool_ConstructorInvocationExpressionNode_Queue.Count > POOL_CONSTRUCTOR_INVOCATION_EXPRESSION_NODE_MAX_COUNT)
        {
            _  = Pool_ConstructorInvocationExpressionNode_Queue.Dequeue();
        }
    }
    
    /// <summary>This also clears any pooled lists.</summary>
    public void ClearAllCompilationUnits()
    {
        __CompilationUnitMap.Clear();
        
        DiagnosticList.Clear();
        SymbolList.Clear();
        FunctionInvocationParameterMetadataList.Clear();
        CodeBlockOwnerList.Clear();
        NodeList.Clear();
    }
    
    /// <summary>TextEditorEditContext is required for thread safety.</summary>
    public void UpsertCompilationUnit(ResourceUri resourceUri, CSharpCompilationUnit compilationUnit)
    {
        if (__CompilationUnitMap.ContainsKey(resourceUri))
            __CompilationUnitMap[resourceUri] = compilationUnit;
        else
            __CompilationUnitMap.Add(resourceUri, compilationUnit);
    }
    
    /// <summary>TextEditorEditContext is required for thread safety.</summary>
    public bool RemoveCompilationUnit(ResourceUri resourceUri)
    {
        return __CompilationUnitMap.Remove(resourceUri);
    }

    /// <summary>
    /// Do not invoke this when re-parsing the same file.
    /// 
    /// Instead, only invoke this when the file is deleted,
    /// and should no longer be included in the binder.
    /// </summary>
    public void ClearStateByResourceUri(ResourceUri resourceUri)
    {
        foreach (var namespaceGroupNodeKvp in _namespaceGroupMap)
        {
            for (int i = namespaceGroupNodeKvp.Value.NamespaceStatementNodeList.Count - 1; i >= 0; i--)
            {
                if (namespaceGroupNodeKvp.Value.NamespaceStatementNodeList[i].ResourceUri == resourceUri)
                    namespaceGroupNodeKvp.Value.NamespaceStatementNodeList.RemoveAt(i);
            }
        }

        __CompilationUnitMap.Remove(resourceUri);
    }

    public ICodeBlockOwner? GetScope(CSharpCompilationUnit compilationUnit, TextEditorTextSpan textSpan)
    {
        return GetScopeByPositionIndex(compilationUnit, textSpan.StartInclusiveIndex);
    }
    
    public ICodeBlockOwner? GetScopeByPositionIndex(CSharpCompilationUnit compilationUnit, int positionIndex)
    {
        var min = int.MaxValue;
        ICodeBlockOwner? codeBlockOwner = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var scope = CodeBlockOwnerList[i];
            
            if (scope.Scope_StartInclusiveIndex <= positionIndex &&
                // Global Scope awkwardly has '-1' ending index exclusive (2023-10-15)
                (scope.Scope_EndExclusiveIndex >= positionIndex || scope.Scope_EndExclusiveIndex == -1))
            {
                var distance = positionIndex - scope.Scope_StartInclusiveIndex;
                if (distance < min)
                {
                    min = distance;
                    codeBlockOwner = scope;
                }
            }
        }
    
        return codeBlockOwner;
    }

    public ICodeBlockOwner? GetScopeByScopeIndexKey(CSharpCompilationUnit compilationUnit, int scopeIndexKey)
    {
        if (scopeIndexKey < 0)
            return null;

        if (scopeIndexKey < compilationUnit.CountCodeBlockOwnerList)
        {
            var isValid = true;

            if (isValid)
                return CodeBlockOwnerList[compilationUnit.IndexCodeBlockOwnerList + scopeIndexKey];
        }
        
        return null;
    }
    
    public TypeDefinitionNode[] GetTypeDefinitionNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        var typeDefinitionNodeList = new List<TypeDefinitionNode>();
    
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var kvp = CodeBlockOwnerList[i];
            
            if (kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                typeDefinitionNodeList.Add((TypeDefinitionNode)kvp);
        }
        
        return typeDefinitionNodeList.ToArray();
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetTypeDefinitionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out TypeDefinitionNode? typeDefinitionNode)
    {
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetTypeDefinitionNodeByScope(
                    resourceUri,
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
                localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
            }
        }

        typeDefinitionNode = null;
        return false;
    }
    
    public bool TryGetTypeDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string typeIdentifierText,
        out TypeDefinitionNode typeDefinitionNode)
    {
        typeDefinitionNode = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = CodeBlockOwnerList[i];
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                GetIdentifierText(x, resourceUri, compilationUnit) == typeIdentifierText)
            {
                typeDefinitionNode = (TypeDefinitionNode)x;
                break;
            }
        }
        
        if (typeDefinitionNode is null)
            return false;
        else
            return true;
    }
    
    public FunctionDefinitionNode[] GetFunctionDefinitionNodesByScope(
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey)
    {
        List<FunctionDefinitionNode> functionDefinitionNodeList = new();
    
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var kvp = CodeBlockOwnerList[i];
            
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
    public bool TryGetFunctionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out FunctionDefinitionNode? functionDefinitionNode)
    {
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetFunctionDefinitionNodeByScope(
                    resourceUri,
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
                localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        functionDefinitionNode = null;
        return false;
    }
    
    public bool TryGetFunctionDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string functionIdentifierText,
        out FunctionDefinitionNode functionDefinitionNode)
    {
        functionDefinitionNode = null;
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = CodeBlockOwnerList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.FunctionDefinitionNode &&
                GetIdentifierText(x, resourceUri, compilationUnit) == functionIdentifierText)
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
    
    public IEnumerable<VariableDeclarationNode> GetVariableDeclarationNodesByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        List<VariableDeclarationNode>? variableDeclarationNodeList = null)
    {
        var isRecursive = variableDeclarationNodeList is not null;
        variableDeclarationNodeList ??= new List<VariableDeclarationNode>();
        
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var kvp = NodeList[i];
            
            if (kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                variableDeclarationNodeList.Add((VariableDeclarationNode)kvp);
        }

        if (!isRecursive && scopeIndexKey < compilationUnit.CountCodeBlockOwnerList)
        {
            var codeBlockOwner = CodeBlockOwnerList[compilationUnit.IndexCodeBlockOwnerList + scopeIndexKey];
            if (codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var typeDefinitionNode = (TypeDefinitionNode)codeBlockOwner;
                if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
                {
                    int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
                    while (positionExclusive < PartialTypeDefinitionList.Count)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                        {
                            CSharpCompilationUnit innerCompilationUnit;
                            
                            if (PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
                            {
                                if (PartialTypeDefinitionList[positionExclusive].ResourceUri != resourceUri)
                                {
                                    if (__CompilationUnitMap.TryGetValue(PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
                                        innerCompilationUnit = temporaryCompilationUnit;
                                    else
                                        innerCompilationUnit = default;
                                }
                                else
                                {
                                    innerCompilationUnit = compilationUnit;
                                }
                                
                                if (!innerCompilationUnit.IsDefault())
                                {
                                    var innerScopeIndexKey = PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                                    GetVariableDeclarationNodesByScope(
                                        PartialTypeDefinitionList[positionExclusive].ResourceUri,
                                        innerCompilationUnit,
                                        innerScopeIndexKey,
                                        variableDeclarationNodeList);
                                }
                            }
                            
                            positionExclusive++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if (typeDefinitionNode.InheritedTypeReference != default)
                {
                    string? identifierText;
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri == resourceUri)
                    {
                        innerCompilationUnit = compilationUnit;
                        innerResourceUri = resourceUri;
                        identifierText = CSharpCompilerService.UnsafeGetText(innerResourceUri.Value, typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                    }
                    else
                    {
                        if (__CompilationUnitMap.TryGetValue(typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri, out innerCompilationUnit))
                        {
                            innerResourceUri = typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri;
                            identifierText = CSharpCompilerService.UnsafeGetText(innerResourceUri.Value, typeDefinitionNode.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                        }
                        else
                        {
                            identifierText = null;
                            innerResourceUri = default;
                        }
                    }
                
                    if (identifierText is not null)
                    {
                        var innerScopeIndexKey = scopeIndexKey;
                        if (typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri != resourceUri &&
                            typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionTextSpan != default)
                        {
                            var scope = GetScopeByPositionIndex(innerCompilationUnit, typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionTextSpan.StartInclusiveIndex);
                            if (scope is not null)
                            {
                                innerScopeIndexKey = scope.Unsafe_SelfIndexKey;
                            }
                        }
                    
                        if (TryGetTypeDefinitionHierarchically(
                                innerResourceUri,
                                innerCompilationUnit,
                                innerScopeIndexKey,
                                identifierText,
                                out var inheritedTypeDefinitionNode))
                        {
                            innerScopeIndexKey = inheritedTypeDefinitionNode.Unsafe_SelfIndexKey;
                            GetVariableDeclarationNodesByScope(
                                typeDefinitionNode.InheritedTypeReference.ExplicitDefinitionResourceUri,
                                innerCompilationUnit,
                                innerScopeIndexKey,
                                variableDeclarationNodeList);
                        }
                    }
                }
            }
        }
        
        return variableDeclarationNodeList;
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
        out VariableDeclarationNode? variableDeclarationStatementNode)
    {
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetVariableDeclarationNodeByScope(
                    resourceUri,
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
                localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        variableDeclarationStatementNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationByPartialType(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        TypeDefinitionNode typeDefinitionNode,
        out VariableDeclarationNode variableDeclarationNode)
    {
        if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].ResourceUri != resourceUri)
                        {
                            if (__CompilationUnitMap.TryGetValue(PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
                            {
                                innerCompilationUnit = temporaryCompilationUnit;
                                innerResourceUri = PartialTypeDefinitionList[positionExclusive].ResourceUri;
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
                            var innerScopeIndexKey = PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                        
                            if (TryGetVariableDeclarationNodeByScope(
                                    innerResourceUri,
                                    innerCompilationUnit,
                                    innerScopeIndexKey,
                                    variableIdentifierText,
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
        }
        
        variableDeclarationNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string variableIdentifierText,
        out VariableDeclarationNode variableDeclarationNode,
        bool isRecursive = false)
    {
        VariableDeclarationNode? matchNode = null;
        
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var x = NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                GetIdentifierText(x, resourceUri, compilationUnit) == variableIdentifierText)
            {
                matchNode = (VariableDeclarationNode)x;
                break;
            }
        }
        
        if (matchNode is null)
        {
            var codeBlockOwner = CodeBlockOwnerList[compilationUnit.IndexCodeBlockOwnerList + scopeIndexKey];
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
                        typeDefinitionNode,
                        out variableDeclarationNode))
                    {
                        return true;
                    }
                }
            }
        
            variableDeclarationNode = null;
            return false;
        }
        else
        {
            variableDeclarationNode = (VariableDeclarationNode)matchNode;
            return true;
        }
    }
    
    public bool TryGetLabelDeclarationHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeIndexKey,
        string identifierText,
        out LabelDeclarationNode? labelDeclarationNode)
    {
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

        while (localScope is not null)
        {
            if (TryGetLabelDeclarationNodeByScope(
                    resourceUri,
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
                localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        labelDeclarationNode = null;
        return false;
    }
    
    public bool TryGetLabelDeclarationNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeIndexKey,
        string labelIdentifierText,
        out LabelDeclarationNode labelDeclarationNode)
    {
        LabelDeclarationNode? matchNode = null;
        
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var x = NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == scopeIndexKey &&
                x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                GetIdentifierText(x, resourceUri, compilationUnit) == labelIdentifierText)
            {
                matchNode = (LabelDeclarationNode)x;
            }
        }
    
        if (matchNode is null)
        {
            labelDeclarationNode = null;
            return false;
        }
        else
        {
            labelDeclarationNode = (LabelDeclarationNode)matchNode;
            return true;
        }
    }
    
    public Symbol? GetSymbol(CSharpCompilationUnit compilationUnit, TextEditorTextSpan textSpan, IReadOnlyList<Symbol> symbolList)
    {
        // Try to find a symbol at that cursor position.
        var foundSymbol = (Symbol?)null;
        
        foreach (var symbol in symbolList)
        {
            if (textSpan.StartInclusiveIndex >= symbol.TextSpan.StartInclusiveIndex &&
                textSpan.StartInclusiveIndex < symbol.TextSpan.EndExclusiveIndex)
            {
                foundSymbol = symbol;
                break;
            }
        }
        
        return foundSymbol;
    }
    
    /// <summary>
    /// If the 'syntaxKind' is unknown then a possible way of determining it is to invoke <see cref="GetSymbol"/>
    /// and use the symbol's syntaxKind.
    ///
    /// Argument 'getTextResult': avoid cached string from 'textSpan.GetText()' if it is calculatable on the fly another way.
    /// </summary>
    public ISyntaxNode? GetDefinitionNode(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        TextEditorTextSpan textSpan,
        SyntaxKind syntaxKind,
        Symbol? symbol = null,
        string? getTextResult = null)
    {
        var scope = GetScope(compilationUnit, textSpan);

        if (scope is null)
            return null;

        var externalSyntaxKind = SyntaxKind.VariableDeclarationNode;
        
        switch (syntaxKind)
        {
            case SyntaxKind.VariableDeclarationNode:
            case SyntaxKind.VariableReferenceNode:
            case SyntaxKind.VariableSymbol:
            case SyntaxKind.PropertySymbol:
            case SyntaxKind.FieldSymbol:
            case SyntaxKind.EnumMemberSymbol:
            {
                if (TryGetVariableDeclarationHierarchically(
                        resourceUri,
                        compilationUnit,
                        scope.Unsafe_SelfIndexKey,
                        getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan),
                        out var variableDeclarationStatementNode)
                    && variableDeclarationStatementNode is not null)
                {
                    return variableDeclarationStatementNode;
                }
                
                externalSyntaxKind = SyntaxKind.VariableDeclarationNode;
                break;
            }
            case SyntaxKind.FunctionInvocationNode:
            case SyntaxKind.FunctionDefinitionNode:
            case SyntaxKind.FunctionSymbol:
            {
                if (TryGetFunctionHierarchically(
                             resourceUri,
                             compilationUnit,
                             scope.Unsafe_SelfIndexKey,
                             getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan),
                             out var functionDefinitionNode)
                         && functionDefinitionNode is not null)
                {
                    if (functionDefinitionNode.IndexMethodOverloadDefinition != -1 &&
                        compilationUnit.CompilationUnitKind == Walk.TextEditor.RazorLib.CompilerServices.CompilationUnitKind.IndividualFile_AllData &&
                        compilationUnit.IndexFunctionInvocationParameterMetadataList != -1 &&
                        compilationUnit.CountFunctionInvocationParameterMetadataList != 0 &&
                        symbol is not null)
                    {
                        var functionParameterList = new List<FunctionInvocationParameterMetadata>();
                        
                        for (int i = compilationUnit.IndexFunctionInvocationParameterMetadataList; i < compilationUnit.IndexFunctionInvocationParameterMetadataList + compilationUnit.CountFunctionInvocationParameterMetadataList; i++)
                        {
                            var entry = FunctionInvocationParameterMetadataList[i];
                            if (entry.IdentifierStartInclusiveIndex == symbol.Value.TextSpan.StartInclusiveIndex)
                            {
                                functionParameterList.Add(entry);
                            }
                        }
                        
                        for (int i = functionDefinitionNode.IndexMethodOverloadDefinition; i < MethodOverloadDefinitionList.Count; i++)
                        {
                            var entry = MethodOverloadDefinitionList[i];
                            
                            if (__CompilationUnitMap.TryGetValue(entry.ResourceUri, out var innerCompilationUnit))
                            {
                                var innerFunctionDefinitionNode = (FunctionDefinitionNode)CodeBlockOwnerList[innerCompilationUnit.IndexCodeBlockOwnerList + entry.ScopeIndexKey];
                                
                                if (innerFunctionDefinitionNode.CountFunctionArgumentEntryList == functionParameterList.Count)
                                {
                                    for (int parameterIndex = innerFunctionDefinitionNode.IndexFunctionArgumentEntryList; parameterIndex < innerFunctionDefinitionNode.IndexFunctionArgumentEntryList + innerFunctionDefinitionNode.CountFunctionArgumentEntryList; parameterIndex++)
                                    {
                                        var argument = FunctionArgumentEntryList[parameterIndex];
                                        var parameter = functionParameterList[parameterIndex];
                                        
                                        string parameterTypeText;
                                        
                                        if (parameter.TypeReference.ExplicitDefinitionTextSpan != default &&
                                            __CompilationUnitMap.TryGetValue(parameter.TypeReference.ExplicitDefinitionResourceUri, out var parameterCompilationUnit))
                                        {
                                            parameterTypeText = CSharpCompilerService.UnsafeGetText(
                                                parameter.TypeReference.ExplicitDefinitionResourceUri.Value,
                                                parameter.TypeReference.TypeIdentifierToken.TextSpan);
                                        }
                                        else
                                        {
                                            parameterTypeText = CSharpCompilerService.UnsafeGetText(
                                                resourceUri.Value,
                                                parameter.TypeReference.TypeIdentifierToken.TextSpan);
                                        }
                                        
                                        if (ArgumentModifierEqualsParameterModifier(argument.ArgumentModifierKind, parameter.ParameterModifierKind) &&
                                            CSharpCompilerService.UnsafeGetText(entry.ResourceUri.Value, argument.TypeReference.TypeIdentifierToken.TextSpan) ==
                                                parameterTypeText)
                                        {
                                            return innerFunctionDefinitionNode;
                                        }
                                    }
                                }
                            }
                        }
                    }
                
                    return functionDefinitionNode;
                }
                
                externalSyntaxKind = SyntaxKind.FunctionDefinitionNode;
                break;
            }
            case SyntaxKind.TypeClauseNode:
            case SyntaxKind.TypeDefinitionNode:
            case SyntaxKind.TypeSymbol:
            case SyntaxKind.ConstructorSymbol:
            {
                if (TryGetTypeDefinitionHierarchically(
                             resourceUri,
                             compilationUnit,
                             scope.Unsafe_SelfIndexKey,
                             getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan),
                             out var typeDefinitionNode) &&
                         typeDefinitionNode is not null)
                {
                    return typeDefinitionNode;
                }
                
                externalSyntaxKind = SyntaxKind.TypeDefinitionNode;
                break;
            }
            case SyntaxKind.NamespaceSymbol:
            {
                    if (NamespacePrefixTree.__Root.Children.TryGetValue(
                        CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan),
                        out var namespacePrefixNode))
                {
                    return new NamespaceClauseNode(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
                }
                
                if (symbol is not null)
                {
                    var fullNamespaceName = CSharpCompilerService.UnsafeGetText(resourceUri.Value, symbol.Value.TextSpan);
                    var splitResult = fullNamespaceName.Split('.');
                    
                    int position = 0;
                    
                    namespacePrefixNode = NamespacePrefixTree.__Root;
                    
                    var success = true;
                    
                    while (position < splitResult.Length)
                    {
                        if (!namespacePrefixNode.Children.TryGetValue(splitResult[position++], out namespacePrefixNode))
                        {
                            success = false;
                            break;
                        }
                    }
                    
                    if (success)
                    {
                        return new NamespaceClauseNode(
                            new SyntaxToken(SyntaxKind.IdentifierToken, textSpan),
                            namespacePrefixNode,
                            startOfMemberAccessChainPositionIndex: textSpan.StartInclusiveIndex);
                    }
                }
                break;
            }
        }

        if (symbol is not null)
        {
            if (SymbolIdToExternalTextSpanMap.TryGetValue(resourceUri.Value, out var symbolIdToExternalTextSpanMap))
            {
                if (symbolIdToExternalTextSpanMap.TryGetValue(symbol.Value.SymbolId, out var definitionTuple))
                {
                    if (__CompilationUnitMap.TryGetValue(definitionTuple.ResourceUri, out var innerCompilationUnit))
                    {
                        var innerResourceUri = definitionTuple.ResourceUri;
                    
                        return GetDefinitionNode(
                            innerResourceUri,
                            innerCompilationUnit,
                            new TextEditorTextSpan(
                                definitionTuple.StartInclusiveIndex,
                                definitionTuple.StartInclusiveIndex + 1,
                                default),
                            externalSyntaxKind,
                            symbol: symbol,
                            getTextResult: CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan));
                    }
                }
            }
        }

        return null;
    }
    
    private bool ArgumentModifierEqualsParameterModifier(ArgumentModifierKind argumentModifier, ParameterModifierKind parameterModifier)
    {
        if (argumentModifier == ArgumentModifierKind.Out && parameterModifier != ParameterModifierKind.Out)
            return false;
        if (argumentModifier == ArgumentModifierKind.In && parameterModifier != ParameterModifierKind.In)
            return false;
        if (argumentModifier == ArgumentModifierKind.Ref && parameterModifier != ParameterModifierKind.Ref)
            return false;
        if (argumentModifier == ArgumentModifierKind.Params && parameterModifier != ParameterModifierKind.Params)
            return false;
        
        return true;
    }

    public ISyntaxNode? GetSyntaxNode(CSharpCompilationUnit compilationUnit, int positionIndex, CSharpCompilationUnit? compilerServiceResource)
    {
        // TODO: Re-implement this given the changes to how nodes are stored.
        return null;
    }
    
    public ISyntaxNode? GetChildNodeOrSelfByPositionIndex(ISyntaxNode node, int positionIndex)
    {
        switch (node.SyntaxKind)
        {
            case SyntaxKind.VariableDeclarationNode:
            
                var variableDeclarationNode = (VariableDeclarationNode)node;
            
                if (variableDeclarationNode.TypeReference.TypeIdentifierToken.ConstructorWasInvoked)
                {
                    if (variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan.StartInclusiveIndex <= positionIndex &&
                        variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan.EndExclusiveIndex >= positionIndex)
                    {
                        return new TypeClauseNode(variableDeclarationNode.TypeReference);
                    }
                    else if (variableDeclarationNode.TypeReference.OpenAngleBracketToken.ConstructorWasInvoked)
                    {
                        for (int i = variableDeclarationNode.TypeReference.IndexGenericParameterEntryList;
                             i < variableDeclarationNode.TypeReference.IndexGenericParameterEntryList + variableDeclarationNode.TypeReference.CountGenericParameterEntryList;
                             i++)
                        {
                            var entry = GenericParameterEntryList[i];
                            
                            if (entry.TypeReference.TypeIdentifierToken.TextSpan.StartInclusiveIndex <= positionIndex &&
                                entry.TypeReference.TypeIdentifierToken.TextSpan.EndExclusiveIndex >= positionIndex)
                            {
                                return new TypeClauseNode(entry.TypeReference);
                            }
                        }
                    }
                }
                
                goto default;
            default:
                return node;
        }
    }
    
    /// <summary>
    /// TODO: In 'GetDefinitionNode(...)' The positionIndex to determine IScope is the same that is used to determine the 'name' of the ISyntaxNode...
    ///       ...This should likely be changed, because function argument goto definition won't work if done from the argument listing, rather than the code block of the function.
    ///       This method will act as a temporary work around.
    /// </summary>
    /*public ISyntaxNode? GetFallbackNode(CSharpCompilationUnit compilationUnit, int positionIndex, ICodeBlockOwner codeBlockOwner)
    {
        if (compilationUnit is null)
            return null;
        
        // Try to find a symbol at that cursor position.
        IReadOnlyList<Symbol> symbolList = compilationUnit?.SymbolList ?? Array.Empty<Symbol>();
        var foundSymbol = (Symbol?)null;
        
        foreach (var symbol in symbolList)
        {
            if (positionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                positionIndex < symbol.TextSpan.EndExclusiveIndex)
            {
                foundSymbol = symbol;
                break;
            }
        }
        
        if (foundSymbol is null)
            return null;
            
        var currentSyntaxKind = foundSymbol.Value.SyntaxKind;
        
        switch (currentSyntaxKind)
        {
            case SyntaxKind.VariableDeclarationNode:
            case SyntaxKind.VariableReferenceNode:
            case SyntaxKind.VariableSymbol:
            case SyntaxKind.PropertySymbol:
            case SyntaxKind.FieldSymbol:
            {
                if (TryGetVariableDeclarationHierarchically(
                        compilationUnit,
                        codeBlockOwner.Unsafe_SelfIndexKey,
                        foundSymbol.Value.TextSpan.GetText(compilationUnit.SourceText, TextEditorService),
                        out var variableDeclarationStatementNode)
                    && variableDeclarationStatementNode is not null)
                {
                    return variableDeclarationStatementNode;
                }
                
                return null;
            }
        }

        return null;
    }*/
    
    /// <summary>
    /// If the provided syntaxNode's SyntaxKind is not recognized, then (-1, -1) is returned.
    ///
    /// Otherwise, this method is meant to understand all of the ISyntaxToken
    /// that the node encompasses.
    ///
    /// With this knowledge, the method can determine the ISyntaxToken that starts, and ends the node
    /// within the source code.
    ///
    /// Then, it returns the indices from the start and end token.
    ///
    /// The ISyntaxNode instances are in a large enough count that it was decided not
    /// to make this an instance method on each ISyntaxNode.
    ///
    /// ========================================================================
    /// There is no overhead per-object-instance for adding a method to a class.
    /// https://stackoverflow.com/a/48861218/14847452
    /// 
    ///     "Yes, C#/.Net methods require memory on per-AppDomain basis, there is no per-instance cost of the methods/properties.
    ///     
    ///     Cost comes from:
    ///     
    ///     methods metadata (part of type) and IL. I'm not sure how long IL stays loaded as it really only needed to JIT so my guess it is loaded as needed and discarded.
    ///     after method is JITed machine code stays till AppDomain is unloaded (or if compiled as neutral till process terminates)
    ///     So instantiating 1 or 50 objects with 50 methods will not require different amount of memory for methods."
    /// ========================================================================
    ///
    /// But, while there is no overhead to having this be on each implementation of 'ISyntaxNode',
    /// it is believed to still belong in the IBinder.
    ///
    /// This is because each language needs to have control over the various nodes.
    /// As one node in C# is not necessarily read the same as it would be by a python 'ICompilerService'.
    ///
    /// The goal with the ISyntaxNode implementations seems to be:
    /// - Keep them as generalized as possible.
    /// - Any specific details should be provided by the IBinder.
    /// </summary>
    public (int StartInclusiveIndex, int EndExclusiveIndex) GetNodePositionIndices(ISyntaxNode syntaxNode)
    {
        switch (syntaxNode.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)syntaxNode;
                
                if (typeDefinitionNode.TypeIdentifierToken.ConstructorWasInvoked)
                    return (typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex, typeDefinitionNode.TypeIdentifierToken.TextSpan.EndExclusiveIndex);
                
                goto default;
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)syntaxNode;
                
                if (functionDefinitionNode.FunctionIdentifierToken.ConstructorWasInvoked)
                    return (functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex, functionDefinitionNode.FunctionIdentifierToken.TextSpan.EndExclusiveIndex);
                
                goto default;
            }
            case SyntaxKind.ConstructorDefinitionNode:
            {
                var constructorDefinitionNode = (ConstructorDefinitionNode)syntaxNode;
                
                if (constructorDefinitionNode.FunctionIdentifier.ConstructorWasInvoked)
                    return (constructorDefinitionNode.FunctionIdentifier.TextSpan.StartInclusiveIndex, constructorDefinitionNode.FunctionIdentifier.TextSpan.EndExclusiveIndex);
                
                goto default;
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)syntaxNode;
                
                int? startInclusiveIndex = null;
                int? endExclusiveIndex = null;
                
                if (variableDeclarationNode.TypeReference.TypeIdentifierToken.ConstructorWasInvoked &&
                    !variableDeclarationNode.TypeReference.IsImplicit)
                {
                    startInclusiveIndex = variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan.StartInclusiveIndex;
                    endExclusiveIndex = variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan.EndExclusiveIndex;
                }
                
                if (variableDeclarationNode.IdentifierToken.ConstructorWasInvoked)
                {
                    startInclusiveIndex ??= variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex;
                    endExclusiveIndex = variableDeclarationNode.IdentifierToken.TextSpan.EndExclusiveIndex;
                }
                
                if (startInclusiveIndex is not null && endExclusiveIndex is not null)
                    return (startInclusiveIndex.Value, endExclusiveIndex.Value);
                
                goto default;
            }
            case SyntaxKind.VariableReferenceNode:
            {
                var variableReferenceNode = (VariableReferenceNode)syntaxNode;
                
                if (variableReferenceNode.VariableIdentifierToken.ConstructorWasInvoked)
                    return (variableReferenceNode.VariableIdentifierToken.TextSpan.StartInclusiveIndex, variableReferenceNode.VariableIdentifierToken.TextSpan.EndExclusiveIndex);
                
                goto default;
            }
            default:
            {
                return (-1, -1);
            }
        }
    }
    
    public string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri, CSharpCompilationUnit compilationUnit)
    {
        string sourceText;
        string resourceUriValue;
    
        switch (node.SyntaxKind)
        {
            case SyntaxKind.TypeDefinitionNode:
            {
                var typeDefinitionNode = (TypeDefinitionNode)node;
                if (typeDefinitionNode.ResourceUri == resourceUri)
                {
                    resourceUriValue = resourceUri.Value;
                }
                else
                {
                    if (__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        resourceUriValue = typeDefinitionNode.ResourceUri.Value;
                    else
                        return string.Empty;
                }
                return CSharpCompilerService.UnsafeGetText(resourceUriValue, typeDefinitionNode.TypeIdentifierToken.TextSpan);
            }
            case SyntaxKind.TypeClauseNode:
            {
                var typeClauseNode = (TypeClauseNode)node;
                if (typeClauseNode.ExplicitDefinitionResourceUri == resourceUri)
                {
                    resourceUriValue = resourceUri.Value;
                }
                else
                {
                    if (__CompilationUnitMap.TryGetValue(typeClauseNode.ExplicitDefinitionResourceUri, out var innerCompilationUnit))
                        resourceUriValue = typeClauseNode.ExplicitDefinitionResourceUri.Value;
                    else
                        return string.Empty;
                }
                return CSharpCompilerService.UnsafeGetText(resourceUriValue, typeClauseNode.TypeIdentifierToken.TextSpan);
            }
            case SyntaxKind.FunctionDefinitionNode:
            {
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                if (functionDefinitionNode.ResourceUri == resourceUri)
                {
                    resourceUriValue = resourceUri.Value;
                }
                else
                {
                    if (__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var innerCompilationUnit))
                        resourceUriValue = functionDefinitionNode.ResourceUri.Value;
                    else
                        return string.Empty;
                }
                return CSharpCompilerService.UnsafeGetText(resourceUriValue, functionDefinitionNode.FunctionIdentifierToken.TextSpan);
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                var functionInvocationNode = (FunctionInvocationNode)node;
                if (functionInvocationNode.ResourceUri == resourceUri)
                {
                    resourceUriValue = resourceUri.Value;
                }
                else
                {
                    if (__CompilationUnitMap.TryGetValue(functionInvocationNode.ResourceUri, out var innerCompilationUnit))
                        resourceUriValue = functionInvocationNode.ResourceUri.Value;
                    else
                        return string.Empty;
                }
                return CSharpCompilerService.UnsafeGetText(resourceUriValue, functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan);
            }
            case SyntaxKind.VariableDeclarationNode:
            {
                var variableDeclarationNode = (VariableDeclarationNode)node;
                if (variableDeclarationNode.ResourceUri == resourceUri)
                {
                    resourceUriValue = resourceUri.Value;
                }
                else
                {
                    if (__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var innerCompilationUnit))
                        resourceUriValue = variableDeclarationNode.ResourceUri.Value;
                    else
                        return string.Empty;
                }
                return CSharpCompilerService.UnsafeGetText(resourceUriValue, variableDeclarationNode.IdentifierToken.TextSpan);
            }
            case SyntaxKind.VariableReferenceNode:
            {
                var variableReferenceNode = (VariableReferenceNode)node;
                return CSharpCompilerService.UnsafeGetText(resourceUri.Value, variableReferenceNode.VariableIdentifierToken.TextSpan);
            }
            case SyntaxKind.LabelDeclarationNode:
            {
                var labelDeclarationNode = (LabelDeclarationNode)node;
                return CSharpCompilerService.UnsafeGetText(resourceUri.Value, labelDeclarationNode.IdentifierToken.TextSpan);
            }
            case SyntaxKind.LabelReferenceNode:
            {
                var labelReferenceNode = (LabelReferenceNode)node;
                return CSharpCompilerService.UnsafeGetText(resourceUri.Value, labelReferenceNode.IdentifierToken.TextSpan);
            }
            default:
            {
                return string.Empty;
            }
        }
    }
    
    public SyntaxToken GetNameToken(ISyntaxNode node)
    {
        switch (node.SyntaxKind)
        {
            case SyntaxKind.VariableDeclarationNode:
                var variableDeclarationNode = (VariableDeclarationNode)node;
                return variableDeclarationNode.IdentifierToken;
            case SyntaxKind.FunctionDefinitionNode:
                var functionDefinitionNode = (FunctionDefinitionNode)node;
                return functionDefinitionNode.FunctionIdentifierToken;
            case SyntaxKind.TypeDefinitionNode:
                var innerTypeDefinitionNode = (TypeDefinitionNode)node;
                return innerTypeDefinitionNode.TypeIdentifierToken;
            case SyntaxKind.TypeClauseNode:
                var innerTypeClauseNode = (TypeClauseNode)node;
                return innerTypeClauseNode.TypeIdentifierToken;
            case SyntaxKind.VariableReferenceNode:
                var innerVariableReferenceNode = (VariableReferenceNode)node;
                return innerVariableReferenceNode.VariableIdentifierToken;
            case SyntaxKind.FunctionInvocationNode:
                var innerFunctionInvocationNode = (FunctionInvocationNode)node;
                return innerFunctionInvocationNode.FunctionInvocationIdentifierToken;
            default:
                return default;
        }
    }
    
    public IEnumerable<ISyntaxNode> GetMemberList_TypeDefinitionNode(TypeDefinitionNode typeDefinitionNode)
    {
        if (typeDefinitionNode.Unsafe_SelfIndexKey == -1 ||
            !__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var compilationUnit))
        {
            return Array.Empty<ISyntaxNode>();
        }
        
        var syntaxNodeList = new List<ISyntaxNode>();
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = CodeBlockOwnerList[i];
        
            if (x.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
                (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                 x.SyntaxKind == SyntaxKind.FunctionDefinitionNode))
            {
                syntaxNodeList.Add(x);
            }
        }
        
        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var x = NodeList[i];
            
            if (x.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
                x.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                syntaxNodeList.Add(x);
            }
        }
        
        /*if (typeDefinitionNode.IndexFunctionArgumentEntryList != -1)
        {
            for (int i = typeDefinitionNode.IndexFunctionArgumentEntryList; i < typeDefinitionNode.IndexFunctionArgumentEntryList + typeDefinitionNode.CountFunctionArgumentEntryList; i++)
            {
                syntaxNodeList.Add(FunctionArgumentEntryList[i].VariableDeclarationNode);
            }
        }*/
        
        if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].ResourceUri != typeDefinitionNode.ResourceUri)
                        {
                            if (__CompilationUnitMap.TryGetValue(PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
                            {
                                innerCompilationUnit = temporaryCompilationUnit;
                                innerResourceUri = PartialTypeDefinitionList[positionExclusive].ResourceUri;
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
                            innerResourceUri = typeDefinitionNode.ResourceUri;
                        }
                        
                        if (!innerCompilationUnit.IsDefault())
                        {
                            var innerScopeIndexKey = PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                            
                            for (int i = innerCompilationUnit.IndexCodeBlockOwnerList; i < innerCompilationUnit.IndexCodeBlockOwnerList + innerCompilationUnit.CountCodeBlockOwnerList; i++)
                            {
                                var x = CodeBlockOwnerList[i];
                                
                                if (x.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                                    (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                                     x.SyntaxKind == SyntaxKind.FunctionDefinitionNode))
                                {
                                    syntaxNodeList.Add(x);
                                }
                            }
                            
                            for (int i = innerCompilationUnit.IndexNodeList; i < innerCompilationUnit.IndexNodeList + innerCompilationUnit.CountNodeList; i++)
                            {
                                var x = NodeList[i];
                                
                                if (x.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                                    x.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                                {
                                    syntaxNodeList.Add(x);
                                }
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
        }
        
        return syntaxNodeList;
    }
    
    private readonly List<ISyntaxNode> _getMemberList = new();
    
    internal List<ISyntaxNode> Internal_GetMemberList_TypeDefinitionNode(TypeDefinitionNode typeDefinitionNode)
    {
        _getMemberList.Clear();
    
        if (typeDefinitionNode.Unsafe_SelfIndexKey == -1 ||
            !__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var compilationUnit))
        {
            return _getMemberList;
        }
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var codeBlockOwner = CodeBlockOwnerList[i];
            
            if (codeBlockOwner.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
                (codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                 codeBlockOwner.SyntaxKind == SyntaxKind.FunctionDefinitionNode))
            {
                _getMemberList.Add(codeBlockOwner);
            }
        }

        for (int i = compilationUnit.IndexNodeList; i < compilationUnit.IndexNodeList + compilationUnit.CountNodeList; i++)
        {
            var node = NodeList[i];
            
            if (node.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
                node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                _getMemberList.Add(node);
            }
        }
        
        /*if (typeDefinitionNode.IndexFunctionArgumentEntryList != -1)
        {
            for (int i = typeDefinitionNode.IndexFunctionArgumentEntryList; i < typeDefinitionNode.IndexFunctionArgumentEntryList + typeDefinitionNode.CountFunctionArgumentEntryList; i++)
            {
                var entry = FunctionArgumentEntryList[i];
                _getMemberList.Add(entry.VariableDeclarationNode);
            }
        }*/
        
        if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].ResourceUri != typeDefinitionNode.ResourceUri)
                        {
                            if (__CompilationUnitMap.TryGetValue(PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
                            {
                                innerCompilationUnit = temporaryCompilationUnit;
                                innerResourceUri = PartialTypeDefinitionList[positionExclusive].ResourceUri;
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
                            innerResourceUri = typeDefinitionNode.ResourceUri;
                        }
                        
                        if (!innerCompilationUnit.IsDefault())
                        {
                            var partialTypeDefinition = PartialTypeDefinitionList[positionExclusive];
                            var innerScopeIndexKey = partialTypeDefinition.ScopeIndexKey;
                            
                            if (partialTypeDefinition.ScopeIndexKey < innerCompilationUnit.CountCodeBlockOwnerList)
                            {
                                var innerCodeBlockOwner = CodeBlockOwnerList[innerCompilationUnit.IndexCodeBlockOwnerList + partialTypeDefinition.ScopeIndexKey];
                                
                                if (innerCodeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                                {
                                    var innerTypeDefinitionNode = (TypeDefinitionNode)innerCodeBlockOwner;
                                
                                    // TODO: Don't duplicate this.
                                    for (int i = innerCompilationUnit.IndexCodeBlockOwnerList; i < innerCompilationUnit.IndexCodeBlockOwnerList + innerCompilationUnit.CountCodeBlockOwnerList; i++)
                                    {
                                        var codeBlockOwner = CodeBlockOwnerList[i];
                                    
                                        if (codeBlockOwner.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                                            (codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                                             codeBlockOwner.SyntaxKind == SyntaxKind.FunctionDefinitionNode))
                                        {
                                            _getMemberList.Add(codeBlockOwner);
                                        }
                                    }
                                    
                                    for (int i = innerCompilationUnit.IndexNodeList; i < innerCompilationUnit.IndexNodeList + innerCompilationUnit.CountNodeList; i++)
                                    {
                                        var node = NodeList[i];
                                        
                                        if (node.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                                            node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                                        {
                                            _getMemberList.Add(node);
                                        }
                                    }
                                    
                                    /*if (innerTypeDefinitionNode.IndexFunctionArgumentEntryList != -1)
                                    {
                                        for (int i = innerTypeDefinitionNode.IndexFunctionArgumentEntryList; i < innerTypeDefinitionNode.IndexFunctionArgumentEntryList + innerTypeDefinitionNode.CountFunctionArgumentEntryList; i++)
                                        {
                                            var entry = FunctionArgumentEntryList[i];
                                            _getMemberList.Add(entry.VariableDeclarationNode);
                                        }
                                    }*/
                                }
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
        }
        
        return _getMemberList;
    }
    
    /// <summary>
    /// <see cref="GetTopLevelTypeDefinitionNodes"/> provides a collection
    /// which contains all top level type definitions of the <see cref="NamespaceStatementNode"/>.
    /// </summary>
    public IEnumerable<TypeDefinitionNode> GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
    {
        if (namespaceStatementNode.Unsafe_SelfIndexKey == -1 ||
            !__CompilationUnitMap.TryGetValue(namespaceStatementNode.ResourceUri, out var compilationUnit))
        {
            return Array.Empty<TypeDefinitionNode>();
        }

        var typeDefinitionNodeList = new List<TypeDefinitionNode>();
        
        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var x = CodeBlockOwnerList[i];
            
            if (x.Unsafe_ParentIndexKey == namespaceStatementNode.Unsafe_SelfIndexKey && x.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                typeDefinitionNodeList.Add((TypeDefinitionNode)x);
        }

        return typeDefinitionNodeList;
    }
    
    /// <summary>
    /// <see cref="GetTopLevelTypeDefinitionNodes"/> provides a collection
    /// which contains all top level type definitions of the namespace.
    /// <br/><br/>
    /// This is to say that, any type definitions which are nested, would not
    /// be in this collection.
    /// </summary>
    public IEnumerable<TypeDefinitionNode> GetTopLevelTypeDefinitionNodes_NamespaceGroup(NamespaceGroup namespaceGroup)
    {
        return namespaceGroup.NamespaceStatementNodeList
            .SelectMany(x => GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(x));
    }
    
    private readonly List<TypeDefinitionNode> _getTopLevelTypeDefinitionNodes = new();
    
    /// <summary>Object-allocation-less version of the public version for internal use.</summary>
    internal List<TypeDefinitionNode> Internal_GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(
        NamespaceStatementNode namespaceStatementNode,
        bool shouldClear)
    {
        if (shouldClear)
        {
            // This allows Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(...) to "select many".
            _getTopLevelTypeDefinitionNodes.Clear();
        }
    
        if (namespaceStatementNode.Unsafe_SelfIndexKey == -1 ||
            !__CompilationUnitMap.TryGetValue(namespaceStatementNode.ResourceUri, out var compilationUnit))
        {
            return _getTopLevelTypeDefinitionNodes;
        }

        for (int i = compilationUnit.IndexCodeBlockOwnerList; i < compilationUnit.IndexCodeBlockOwnerList + compilationUnit.CountCodeBlockOwnerList; i++)
        {
            var codeBlockOwner = CodeBlockOwnerList[i];
        
            if (codeBlockOwner.Unsafe_ParentIndexKey == namespaceStatementNode.Unsafe_SelfIndexKey && codeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                _getTopLevelTypeDefinitionNodes.Add((TypeDefinitionNode)codeBlockOwner);
        }
        
        return _getTopLevelTypeDefinitionNodes;
    }
    
    /// <summary>Object-allocation-less version of the public version for internal use.</summary>
    internal List<TypeDefinitionNode> Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(NamespaceGroup namespaceGroup)
    {
        _getTopLevelTypeDefinitionNodes.Clear();
    
        foreach (var namespaceStatementNode in namespaceGroup.NamespaceStatementNodeList)
        {
            _ = Internal_GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(namespaceStatementNode, shouldClear: false);
        }
        
        return _getTopLevelTypeDefinitionNodes;
    }
}
