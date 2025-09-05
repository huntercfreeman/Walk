using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.CompilerServices.CSharp.Facts;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.Extensions.CompilerServices.Syntax.Values;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.CompilerServices.CSharp.BinderCase;

public partial class CSharpBinder
{
    /// <summary>
    /// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
    ///
    /// TODO: Don't have this be public
    /// </summary>
    public readonly List<NamespaceGroup> _namespaceGroupList = new();
    //private readonly NamespaceStatementNode _topLevelNamespaceStatementNode;
    
    public List<PartialTypeDefinitionValue> PartialTypeDefinitionList { get; } = new();
    //public List<MethodOverloadDefinition> MethodOverloadDefinitionList { get; } = new();
    //public bool MethodOverload_ResourceUri_WasCleared { get; set; }
    
    /// <summary>
    /// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
    /// </summary>
    public List<NamespaceGroup> NamespaceGroupMap => _namespaceGroupList;
    //public List<TypeDefinitionNode> AllTypeDefinitionList { get; } = new();
    
    /// <summary>
    /// CONFUSING: During a parse the "previous" CSharpCompilationUnit gets read from here...
    /// ...because the currently being parsed CSharpCompilationUnit has not been written to this map yet.
    /// </summary>
    public Dictionary<ResourceUri, CSharpCompilationUnit> __CompilationUnitMap { get; } = new();
    
    // public NamespacePrefixTree NamespacePrefixTree { get; } = new();
    
    public NamespaceStatementNode TopLevelNamespaceStatementNode { get; private set; }
    
    public List<GenericParameter> GenericParameterList { get; } = new();
    public List<FunctionParameter> FunctionParameterList { get; } = new();
    public List<FunctionArgument> FunctionArgumentList { get; } = new();
    public List<FunctionInvocationParameterMetadata> FunctionInvocationParameterMetadataList { get; } = new();
    public List<NamespaceContribution> NamespaceContributionList { get; } = new();
    public Dictionary<string, Dictionary<int, (ResourceUri ResourceUri, int StartInclusiveIndex)>> SymbolIdToExternalTextSpanMap { get; } = new();
    public List<Walk.TextEditor.RazorLib.CompilerServices.TextEditorDiagnostic> DiagnosticList { get; } = new();
    public List<Symbol> SymbolList { get; } = new();
    public List<Scope> ScopeList { get; } = new();

    public List<SyntaxNodeValue> NodeList { get; } = new();
    public List<TypeDefinitionTraits> TypeDefinitionTraitsList { get; } = new();
    public List<FunctionDefinitionTraits> FunctionDefinitionTraitsList { get; } = new();
    public List<ConstructorDefinitionTraits> ConstructorDefinitionTraitsList { get; } = new();
    public List<VariableDeclarationTraits> VariableDeclarationTraitsList { get; } = new();
    
    public TextEditorService TextEditorService { get; set; }
    public CSharpCompilerService CSharpCompilerService { get; set; }
    
    public GlobalCodeBlockNode GlobalCodeBlockNode { get; } = new GlobalCodeBlockNode();
    
    public CSharpBinder(TextEditorService textEditorService, CSharpCompilerService cSharpCompilerService)
    {
        TopLevelNamespaceStatementNode = new NamespaceStatementNode(
            new(SyntaxKind.UnrecognizedTokenKeyword, new(0, 0, 0)),
            new(SyntaxKind.IdentifierToken, new(0, 0, 0)),
            ResourceUri.Empty);

        _namespaceGroupList.Add(
            new NamespaceGroup(
                charIntSum: 0,
                new List<NamespaceStatementNode>
                {
                    TopLevelNamespaceStatementNode
                }));

        TextEditorService = textEditorService;
        CSharpCompilerService = cSharpCompilerService;
        
        /*AllTypeDefinitionList.Add(CSharpFacts.Types.Void);
        AllTypeDefinitionList.Add(CSharpFacts.Types.Int);
        AllTypeDefinitionList.Add(CSharpFacts.Types.Char);
        AllTypeDefinitionList.Add(CSharpFacts.Types.String);
        AllTypeDefinitionList.Add(CSharpFacts.Types.Bool);
        AllTypeDefinitionList.Add(CSharpFacts.Types.Var);*/
    
        for (int i = 0; i < POOL_BINARY_EXPRESSION_NODE_MAX_COUNT; i++)
        {
            Pool_BinaryExpressionNode_Queue.Enqueue(new BinaryExpressionNode(
                leftOperandTypeReference: default,
                operatorToken: default,
                rightOperandTypeReference: default,
                resultTypeReference: default));
        }
    
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
            Pool_AmbiguousIdentifierExpressionNode_Queue.Enqueue(new AmbiguousIdentifierNode(
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
            Pool_ConstructorInvocationExpressionNode_Queue.Enqueue(new ConstructorInvocationNode(
                newKeywordToken: default,
                typeReference: default,
                openParenthesisToken: default,
                indexFunctionParameterEntryList: -1,
                countFunctionParameterEntryList: 0,
                closeParenthesisToken: default));
        }
        
        /*_ = Task.Run(async () =>
        {
            await Task.Delay(10_000);
            
            Console.WriteLine($"HIT: {CSharpParserModel.POOL_BinaryExpressionNode_HIT}");
            Console.WriteLine($"MISS: {CSharpParserModel.POOL_BinaryExpressionNode_MISS}");
            Console.WriteLine($"RETURN: {CSharpParserModel.POOL_BinaryExpressionNode_RETURN}");
            Console.WriteLine($"%: {((double)CSharpParserModel.POOL_BinaryExpressionNode_HIT / (CSharpParserModel.POOL_BinaryExpressionNode_HIT + CSharpParserModel.POOL_BinaryExpressionNode_MISS)):P2}");
        });*/
    }
    
    /*public NamespacePrefixNode? FindPrefix(NamespacePrefixNode start, TextEditorTextSpan textSpan, string absolutePathString)
    {
        var findTuple = NamespacePrefixTree.FindRange(
            start,
            textSpan.CharIntSum);
            
        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var node = start.Children[i];
            if (CSharpCompilerService.SafeCompareTextSpans(
                    absolutePathString,
                    textSpan,
                    node.ResourceUri.Value,
                    node.TextSpan))
            {
                return node;
            }
        }
        
        return null;
    }*/
    
    /*public (NamespacePrefixNode? Node, int InsertionIndex) FindPrefix_WithInsertionIndex(NamespacePrefixNode start, TextEditorTextSpan textSpan, string absolutePathString)
    {
        var findTuple = NamespacePrefixTree.FindRange(
            start,
            textSpan.CharIntSum);
            
        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var node = start.Children[i];
            if (CSharpCompilerService.SafeCompareTextSpans(
                    absolutePathString,
                    textSpan,
                    node.ResourceUri.Value,
                    node.TextSpan))
            {
                return (node, findTuple.InsertionIndex);
            }
        }
        
        return (null, findTuple.InsertionIndex);
    }*/
    
    public (NamespaceGroup TargetGroup, int GroupIndex) FindNamespaceGroup_Reversed_WithMatchedIndex(
        ResourceUri resourceUri,
        TextEditorTextSpan textSpan)
    {
        var findTuple = NamespaceGroup_FindRange(textSpan);
    
        for (int groupIndex = findTuple.EndIndex - 1; groupIndex >= findTuple.StartIndex; groupIndex--)
        {
            var targetGroup = _namespaceGroupList[groupIndex];
            if (targetGroup.NamespaceStatementNodeList.Count != 0)
            {
                var sampleNamespaceStatementNode = targetGroup.NamespaceStatementNodeList[0];
                if (CSharpCompilerService.SafeCompareTextSpans(
                    resourceUri.Value,
                    textSpan,
                    sampleNamespaceStatementNode.ResourceUri.Value,
                    sampleNamespaceStatementNode.IdentifierToken.TextSpan))
                {
                    return (targetGroup, groupIndex);
                }
            }
        }
        
        return (default, -1);
    }

    /// <summary>(inclusive, exclusive, this is the index at which you'd insert the text span)</summary>
    /*public (int StartIndex, int EndIndex, int InsertionIndex) TypeDefinition_FindRange(TextEditorTextSpan textSpan)
    {
        var startIndex = -1;
        var endIndex = -1;
        var insertionIndex = AllTypeDefinitionList.Count;

        for (int i = 0; i < AllTypeDefinitionList.Count; i++)
        {
            var node = AllTypeDefinitionList[i];

            if (node.TypeIdentifierToken.TextSpan.CharIntSum == textSpan.CharIntSum)
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
            endIndex = AllTypeDefinitionList.Count;

        return (startIndex, endIndex, insertionIndex);
    }*/
    
    /// <summary>(inclusive, exclusive, this is the index at which you'd insert the text span)</summary>
    public (int StartIndex, int EndIndex, int InsertionIndex) NamespaceGroup_FindRange(TextEditorTextSpan textSpan)
    {
        var startIndex = -1;
        var endIndex = -1;
        var insertionIndex = _namespaceGroupList.Count;

        for (int i = 0; i < _namespaceGroupList.Count; i++)
        {
            var node = _namespaceGroupList[i];

            if (node.CharIntSum == textSpan.CharIntSum)
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
            endIndex = _namespaceGroupList.Count;

        return (startIndex, endIndex, insertionIndex);
    }
    
    /// <summary>(inclusive, exclusive, this is the index at which you'd insert the text span)</summary>
    public (int StartIndex, int EndIndex, int InsertionIndex) AddedNamespaceList_FindRange(TextEditorTextSpan textSpan)
    {
        var startIndex = -1;
        var endIndex = -1;
        var insertionIndex = CSharpParserModel_AddedNamespaceList.Count;

        for (int i = 0; i < CSharpParserModel_AddedNamespaceList.Count; i++)
        {
            var node = CSharpParserModel_AddedNamespaceList[i];

            if (node.CharIntSum == textSpan.CharIntSum)
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
            endIndex = CSharpParserModel_AddedNamespaceList.Count;

        return (startIndex, endIndex, insertionIndex);
    }
    
    public bool CheckAlreadyAddedNamespace(
        ResourceUri resourceUri,
        TextEditorTextSpan textSpan)
    {
        var findTuple = AddedNamespaceList_FindRange(textSpan);

        for (int i = findTuple.StartIndex; i < findTuple.EndIndex; i++)
        {
            var target = CSharpParserModel_AddedNamespaceList[i];
            if (CSharpCompilerService.SafeCompareTextSpans(
                    resourceUri.Value,
                    textSpan,
                    resourceUri.Value,
                    target))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary><see cref="FinalizeCompilationUnit"/></summary>
    public void StartCompilationUnit(ResourceUri resourceUri)
    {
        if (__CompilationUnitMap.TryGetValue(resourceUri, out var previousCompilationUnit))
        {
            for (int i = previousCompilationUnit.NamespaceContributionOffset + previousCompilationUnit.NamespaceContributionLength - 1; i >= previousCompilationUnit.NamespaceContributionOffset; i--)
            {
                var namespaceContributionEntry = NamespaceContributionList[i];
                
                var tuple = FindNamespaceGroup_Reversed_WithMatchedIndex(
                    resourceUri,
                    namespaceContributionEntry.TextSpan);
                
                if (tuple.TargetGroup.ConstructorWasInvoked)
                {
                    for (int removeIndex = tuple.TargetGroup.NamespaceStatementNodeList.Count - 1; removeIndex >= 0; removeIndex--)
                    {
                        if (tuple.TargetGroup.NamespaceStatementNodeList[removeIndex].ResourceUri == resourceUri)
                        {
                            tuple.TargetGroup.NamespaceStatementNodeList.RemoveAt(removeIndex);
                            if (tuple.TargetGroup.NamespaceStatementNodeList.Count == 0)
                            {
                                _namespaceGroupList.RemoveAt(tuple.GroupIndex);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary><see cref="StartCompilationUnit"/></summary>
    public void FinalizeCompilationUnit(ResourceUri resourceUri, CSharpCompilationUnit compilationUnit)
    {
        UpsertCompilationUnit(resourceUri, compilationUnit);
        
        while (Pool_BinaryExpressionNode_Queue.Count > POOL_BINARY_EXPRESSION_NODE_MAX_COUNT)
        {
            _ = Pool_BinaryExpressionNode_Queue.Dequeue();
        }
        
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
        ScopeList.Clear();
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
        throw new NotImplementedException();
    }

    public Scope GetScope(CSharpCompilationUnit compilationUnit, TextEditorTextSpan textSpan)
    {
        return GetScopeByPositionIndex(compilationUnit, textSpan.StartInclusiveIndex);
    }
    
    public Scope GetScopeByPositionIndex(CSharpCompilationUnit compilationUnit, int positionIndex)
    {
        var min = int.MaxValue;
        var selfScopeSubIndex = -1;
        
        for (int i = compilationUnit.ScopeOffset; i < compilationUnit.ScopeOffset + compilationUnit.ScopeLength; i++)
        {
            var scope = ScopeList[i];
            
            if (scope.Scope_StartInclusiveIndex <= positionIndex &&
                // Global Scope awkwardly has '-1' ending index exclusive (2023-10-15)
                (scope.Scope_EndExclusiveIndex >= positionIndex || scope.Scope_EndExclusiveIndex == -1))
            {
                var distance = positionIndex - scope.Scope_StartInclusiveIndex;
                if (distance < min)
                {
                    min = distance;
                    selfScopeSubIndex = scope.SelfScopeSubIndex;
                }
            }
        }
    
        if (selfScopeSubIndex == -1)
            return default;
        else
            return ScopeList[compilationUnit.ScopeOffset + selfScopeSubIndex];
    }

    public Scope GetScopeByOffset(CSharpCompilationUnit compilationUnit, int scopeSubIndex)
    {
        if (scopeSubIndex < 0)
            return default;

        if (scopeSubIndex < compilationUnit.ScopeLength)
            return ScopeList[compilationUnit.ScopeOffset + scopeSubIndex];
        
        return default;
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetTypeDefinitionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeSubIndex,
        string identifierText,
        out SyntaxNodeValue typeDefinitionNode)
    {
        var localScope = GetScopeByOffset(compilationUnit, initialScopeSubIndex);

        while (!localScope.IsDefault())
        {
            if (TryGetTypeDefinitionNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.SelfScopeSubIndex,
                    identifierText,
                    out typeDefinitionNode))
            {
                return true;
            }

            if (localScope.ParentScopeSubIndex == -1)
                localScope = default;
            else
            {
                localScope = GetScopeByOffset(compilationUnit, localScope.ParentScopeSubIndex);
            }
        }

        typeDefinitionNode = default;
        return false;
    }
    
    public bool TryGetTypeDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        string typeIdentifierText,
        out SyntaxNodeValue typeDefinitionNode)
    {
        typeDefinitionNode = default;
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            if (node.ParentScopeSubIndex == scopeSubIndex &&
                node.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                CSharpCompilerService.UnsafeGetText(node.ResourceUri.Value, node.IdentifierToken.TextSpan) == typeIdentifierText)
            {
                typeDefinitionNode = node;
                break;
            }
        }
        
        if (typeDefinitionNode.IsDefault())
            return false;
        else
            return true;
    }
    
    public List<string> GetFunctionDefinitionNamesByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex)
    {
        List<string> functionDefinitionNameList = new();
    
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == scopeSubIndex && node.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                functionDefinitionNameList.Add(CSharpCompilerService.UnsafeGetText(resourceUri.Value, node.IdentifierToken.TextSpan));
        }
        
        return functionDefinitionNameList;
    }
    
    /// <summary>
    /// Search hierarchically through all the scopes, starting at the <see cref="initialScope"/>.<br/><br/>
    /// If a match is found, then set the out parameter to it and return true.<br/><br/>
    /// If none of the searched scopes contained a match then set the out parameter to null and return false.
    /// </summary>
    public bool TryGetFunctionHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeSubIndex,
        string identifierText,
        out SyntaxNodeValue functionDefinitionNode)
    {
        var localScope = GetScopeByOffset(compilationUnit, initialScopeSubIndex);

        while (!localScope.IsDefault())
        {
            if (TryGetFunctionDefinitionNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.SelfScopeSubIndex,
                    identifierText,
                    out functionDefinitionNode))
            {
                return true;
            }

            if (localScope.ParentScopeSubIndex == -1)
                localScope = default;
            else
                localScope = GetScopeByOffset(compilationUnit, localScope.ParentScopeSubIndex);
        }

        functionDefinitionNode = default;
        return false;
    }
    
    public bool TryGetFunctionDefinitionNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        string functionIdentifierText,
        out SyntaxNodeValue functionDefinitionNode)
    {
        functionDefinitionNode = default;
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == scopeSubIndex &&
                node.SyntaxKind == SyntaxKind.FunctionDefinitionNode &&
                CSharpCompilerService.UnsafeGetText(node.ResourceUri.Value, node.IdentifierToken.TextSpan) == functionIdentifierText)
            {
                functionDefinitionNode = node;
                break;
            }
        }
        
        if (functionDefinitionNode.IsDefault())
            return false;
        else
            return true;
    }
    
    public List<string> GetVariableDeclarationNodesByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        List<string>? variableDeclarationNodeList = null)
    {
        var isRecursive = variableDeclarationNodeList is not null;
        variableDeclarationNodeList ??= new List<string>();
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == scopeSubIndex && node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                variableDeclarationNodeList.Add(CSharpCompilerService.UnsafeGetText(resourceUri.Value, node.IdentifierToken.TextSpan));
        }

        if (!isRecursive && scopeSubIndex < compilationUnit.ScopeLength)
        {
            var node = NodeList[compilationUnit.NodeOffset + ScopeList[compilationUnit.ScopeOffset + scopeSubIndex].NodeSubIndex];
            if (node.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var typeDefinitionNode = node;
                var typeDefinitionMetadata = TypeDefinitionTraitsList[typeDefinitionNode.TraitsIndex];
                if (typeDefinitionMetadata.IndexPartialTypeDefinition != -1)
                {
                    int positionExclusive = typeDefinitionMetadata.IndexPartialTypeDefinition;
                    while (positionExclusive < PartialTypeDefinitionList.Count)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionMetadata.IndexPartialTypeDefinition)
                        {
                            CSharpCompilationUnit innerCompilationUnit;
                            
                            if (PartialTypeDefinitionList[positionExclusive].ScopeSubIndex != -1)
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
                                    var innerScopeSubIndex = PartialTypeDefinitionList[positionExclusive].ScopeSubIndex;
                                    GetVariableDeclarationNodesByScope(
                                        PartialTypeDefinitionList[positionExclusive].ResourceUri,
                                        innerCompilationUnit,
                                        innerScopeSubIndex,
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
                
                if (!typeDefinitionMetadata.InheritedTypeReference.IsDefault())
                {
                    string? identifierText;
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionResourceUri == resourceUri)
                    {
                        innerCompilationUnit = compilationUnit;
                        innerResourceUri = resourceUri;
                        identifierText = CSharpCompilerService.UnsafeGetText(innerResourceUri.Value, typeDefinitionMetadata.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                    }
                    else
                    {
                        if (__CompilationUnitMap.TryGetValue(typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionResourceUri, out innerCompilationUnit))
                        {
                            innerResourceUri = typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionResourceUri;
                            identifierText = CSharpCompilerService.UnsafeGetText(innerResourceUri.Value, typeDefinitionMetadata.InheritedTypeReference.TypeIdentifierToken.TextSpan);
                        }
                        else
                        {
                            identifierText = null;
                            innerResourceUri = default;
                        }
                    }
                
                    if (identifierText is not null)
                    {
                        var innerScopeSubIndex = scopeSubIndex;
                        if (typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionResourceUri != resourceUri &&
                            typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionTextSpan != default)
                        {
                            var scope = GetScopeByPositionIndex(innerCompilationUnit, typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionTextSpan.StartInclusiveIndex);
                            if (!scope.IsDefault())
                            {
                                innerScopeSubIndex = scope.SelfScopeSubIndex;
                            }
                        }
                    
                        if (TryGetTypeDefinitionHierarchically(
                                innerResourceUri,
                                innerCompilationUnit,
                                innerScopeSubIndex,
                                identifierText,
                                out var inheritedTypeDefinitionNode))
                        {
                            innerScopeSubIndex = inheritedTypeDefinitionNode.SelfScopeSubIndex;
                            GetVariableDeclarationNodesByScope(
                                typeDefinitionMetadata.InheritedTypeReference.ExplicitDefinitionResourceUri,
                                innerCompilationUnit,
                                innerScopeSubIndex,
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
        int initialScopeSubIndex,
        string identifierText,
        out SyntaxNodeValue variableDeclarationStatementNode)
    {
        var localScope = GetScopeByOffset(compilationUnit, initialScopeSubIndex);

        while (!localScope.IsDefault())
        {
            if (TryGetVariableDeclarationNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.SelfScopeSubIndex,
                    identifierText,
                    out variableDeclarationStatementNode))
            {
                return true;
            }

            if (localScope.ParentScopeSubIndex == -1)
                localScope = default;
            else
                localScope = GetScopeByOffset(compilationUnit, localScope.ParentScopeSubIndex);
        }

        variableDeclarationStatementNode = default;
        return false;
    }
    
    public bool TryGetVariableDeclarationByPartialType(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        string variableIdentifierText,
        SyntaxNodeValue typeDefinitionNode,
        out SyntaxNodeValue variableDeclarationNode)
    {
        var typeMetadata = TypeDefinitionTraitsList[typeDefinitionNode.TraitsIndex];
        
        if (typeMetadata.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeMetadata.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeMetadata.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeSubIndex != -1)
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
                            var innerScopeIndexKey = PartialTypeDefinitionList[positionExclusive].ScopeSubIndex;
                        
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
        
        variableDeclarationNode = default;
        return false;
    }
    
    public bool TryGetVariableDeclarationNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        string variableIdentifierText,
        out SyntaxNodeValue variableDeclarationNode,
        bool isRecursive = false)
    {
        SyntaxNodeValue matchNode = default;
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == scopeSubIndex &&
                node.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                CSharpCompilerService.UnsafeGetText(node.ResourceUri.Value, node.IdentifierToken.TextSpan) == variableIdentifierText)
            {
                matchNode = node;
                break;
            }
        }
        
        if (matchNode.IsDefault())
        {
            var scope = ScopeList[compilationUnit.ScopeOffset + scopeSubIndex];
            if (!isRecursive && scope.OwnerSyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                var typeDefinitionNode = NodeList[compilationUnit.NodeOffset + scope.NodeSubIndex];
                var typeMetadata = TypeDefinitionTraitsList[typeDefinitionNode.TraitsIndex];
                if (typeMetadata.IndexPartialTypeDefinition != -1)
                {
                    if (TryGetVariableDeclarationByPartialType(
                        resourceUri,
                        compilationUnit,
                        scopeSubIndex,
                        variableIdentifierText,
                        typeDefinitionNode,
                        out variableDeclarationNode))
                    {
                        return true;
                    }
                }
            }
        
            variableDeclarationNode = default;
            return false;
        }
        else
        {
            variableDeclarationNode = matchNode;
            return true;
        }
    }
    
    public bool TryGetLabelDeclarationHierarchically(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int initialScopeSubIndex,
        string identifierText,
        out SyntaxNodeValue labelDeclarationNode)
    {
        var localScope = GetScopeByOffset(compilationUnit, initialScopeSubIndex);

        while (!localScope.IsDefault())
        {
            if (TryGetLabelDeclarationNodeByScope(
                    resourceUri,
                    compilationUnit,
                    localScope.SelfScopeSubIndex,
                    identifierText,
                    out labelDeclarationNode))
            {
                return true;
            }

            if (localScope.ParentScopeSubIndex == -1)
                localScope = default;
            else
                localScope = GetScopeByOffset(compilationUnit, localScope.ParentScopeSubIndex);
        }

        labelDeclarationNode = default;
        return false;
    }
    
    public bool TryGetLabelDeclarationNodeByScope(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        int scopeSubIndex,
        string labelIdentifierText,
        out SyntaxNodeValue labelDeclarationNode)
    {
        SyntaxNodeValue matchNode = default;
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == scopeSubIndex &&
                node.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                CSharpCompilerService.UnsafeGetText(node.ResourceUri.Value, node.IdentifierToken.TextSpan) == labelIdentifierText)
            {
                matchNode = node;
            }
        }
    
        if (matchNode.IsDefault())
        {
            labelDeclarationNode = default;
            return false;
        }
        else
        {
            labelDeclarationNode = matchNode;
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
    public SyntaxNodeValue GetDefinitionNodeValue(
        ResourceUri resourceUri,
        CSharpCompilationUnit compilationUnit,
        TextEditorTextSpan textSpan,
        SyntaxKind syntaxKind,
        Symbol? symbol = null,
        string? getTextResult = null)
    {
        var scope = GetScope(compilationUnit, textSpan);

        if (scope.IsDefault())
            return default;

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
                var text = getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan);
            
                if (text is not null &&
                    TryGetVariableDeclarationHierarchically(
                        resourceUri,
                        compilationUnit,
                        scope.SelfScopeSubIndex,
                        text,
                        out var variableDeclarationStatementNode))
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
                var text = getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan);
                
                if (text is not null && 
                    TryGetFunctionHierarchically(
                        resourceUri,
                        compilationUnit,
                        scope.SelfScopeSubIndex,
                        text,
                        out var functionDefinitionNode))
                {
                    /*if (functionDefinitionNode.IndexMethodOverloadDefinition != -1 &&
                        compilationUnit.CompilationUnitKind == Walk.TextEditor.RazorLib.CompilerServices.CompilationUnitKind.IndividualFile_AllData &&
                        compilationUnit.FunctionInvocationParameterMetadataOffset != -1 &&
                        compilationUnit.FunctionInvocationParameterMetadataLength != 0 &&
                        symbol is not null)
                    {
                        var functionParameterList = new List<FunctionInvocationParameterMetadata>();
                        
                        for (int i = compilationUnit.FunctionInvocationParameterMetadataOffset; i < compilationUnit.FunctionInvocationParameterMetadataOffset + compilationUnit.FunctionInvocationParameterMetadataLength; i++)
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
                                var innerFunctionDefinitionNode = 
                                    (FunctionDefinitionNode)NodeList[innerCompilationUnit.NodeOffset + ScopeList[innerCompilationUnit.ScopeOffset + entry.ScopeIndexKey].NodeSubIndex];
                                
                                if (innerFunctionDefinitionNode.CountFunctionArgumentEntryList == functionParameterList.Count)
                                {
                                    for (int parameterIndex = innerFunctionDefinitionNode.IndexFunctionArgumentEntryList; parameterIndex < innerFunctionDefinitionNode.IndexFunctionArgumentEntryList + innerFunctionDefinitionNode.CountFunctionArgumentEntryList; parameterIndex++)
                                    {
                                        var argument = FunctionArgumentEntryList[parameterIndex];
                                        var parameter = functionParameterList[parameterIndex];
                                        
                                        string? parameterTypeText;
                                        
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
                                        
                                        if (parameterTypeText is not null &&
                                            ArgumentModifierEqualsParameterModifier(argument.ArgumentModifierKind, parameter.ParameterModifierKind) &&
                                            CSharpCompilerService.UnsafeGetText(entry.ResourceUri.Value, argument.TypeReference.TypeIdentifierToken.TextSpan) ==
                                                parameterTypeText)
                                        {
                                            return innerFunctionDefinitionNode;
                                        }
                                    }
                                }
                            }
                        }
                    }*/
                
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
                var text = getTextResult ?? CSharpCompilerService.UnsafeGetText(resourceUri.Value, textSpan);
            
                if (TryGetTypeDefinitionHierarchically(
                        resourceUri,
                        compilationUnit,
                        scope.SelfScopeSubIndex,
                        text,
                        out var typeDefinitionNode))
                {
                    return typeDefinitionNode;
                }
                
                externalSyntaxKind = SyntaxKind.TypeDefinitionNode;
                break;
            }
            case SyntaxKind.NamespaceSymbol:
            {
                /*var matchedPrefix = FindPrefix(NamespacePrefixTree.__Root, textSpan, resourceUri.Value);
                if (matchedPrefix is not null)
                {
                    return default;
                    //return new NamespaceClauseNode(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
                }

                if (symbol is not null)
                {
                    var fullNamespaceName = CSharpCompilerService.UnsafeGetText(resourceUri.Value, symbol.Value.TextSpan);
                    if (fullNamespaceName is not null)
                    {
                        var splitResult = fullNamespaceName.Split('.');
                        
                        int position = 0;
                        
                        var namespacePrefixNode = NamespacePrefixTree.__Root;
                        
                        var success = true;
                        
                        while (position < splitResult.Length)
                        {
                            int charIntSum = 0;
                            foreach (var character in splitResult[position])
                            {
                                charIntSum += (int)character;
                            }
                            
                            // TODO: This doesn't work because of the textSpan not being the split but the whole.
                            var node = FindPrefix(namespacePrefixNode, textSpan, resourceUri.Value);
                            if (node is null)
                            {
                                success = false;
                                break;
                            }
                            else
                            {
                                namespacePrefixNode = node;
                            }
                        }
                        
                        if (success)
                        {
                            return default;
                            /*return new NamespaceClauseNode(
                                new SyntaxToken(SyntaxKind.IdentifierToken, textSpan),
                                namespacePrefixNode,
                                startOfMemberAccessChainPositionIndex: textSpan.StartInclusiveIndex);*//*
                        }
                    }
                }*/
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
                    
                        return GetDefinitionNodeValue(
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

        return default;
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
                            var entry = GenericParameterList[i];
                            
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
                        codeBlockOwner.SelfIndexKey,
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
    
    public IEnumerable<SyntaxNodeValue> GetMemberList_TypeDefinitionNode(SyntaxNodeValue typeDefinitionNode)
    {
        var typeDefinitionMetadata = TypeDefinitionTraitsList[typeDefinitionNode.TraitsIndex];
        return GetMemberList_TypeDefinitionNode(typeDefinitionNode.ResourceUri, typeDefinitionNode.SelfScopeSubIndex, typeDefinitionMetadata.IndexPartialTypeDefinition);
    }
    
    public IEnumerable<SyntaxNodeValue> GetMemberList_TypeDefinitionNode(TypeDefinitionNode typeDefinitionNode)
    {
        return GetMemberList_TypeDefinitionNode(typeDefinitionNode.ResourceUri, typeDefinitionNode.SelfScopeSubIndex, typeDefinitionNode.IndexPartialTypeDefinition);
    }
    
    public IEnumerable<SyntaxNodeValue> GetMemberList_TypeDefinitionNode(
        ResourceUri typeDefinitionResourceUri,
        int typeDefinitionSelfScopeSubIndex,
        int typeDefinitionIndexPartialTypeDefinition)
    {
        if (typeDefinitionSelfScopeSubIndex == -1 ||
            !__CompilationUnitMap.TryGetValue(typeDefinitionResourceUri, out var compilationUnit))
        {
            return Array.Empty<SyntaxNodeValue>();
        }
        
        var syntaxNodeList = new List<SyntaxNodeValue>();
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == typeDefinitionSelfScopeSubIndex &&
                (node.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                 node.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
                 node.SyntaxKind == SyntaxKind.VariableDeclarationNode))
            {
                syntaxNodeList.Add(node);
            }
        }
        
        /*if (typeDefinitionNode.IndexFunctionArgumentEntryList != -1)
        {
            for (int i = typeDefinitionNode.IndexFunctionArgumentEntryList; i < typeDefinitionNode.IndexFunctionArgumentEntryList + typeDefinitionNode.CountFunctionArgumentEntryList; i++)
            {
                syntaxNodeList.Add(FunctionArgumentEntryList[i].VariableDeclarationNode);
            }
        }*/
        
        if (typeDefinitionIndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionIndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionIndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeSubIndex != -1)
                    {
                        if (PartialTypeDefinitionList[positionExclusive].ResourceUri != typeDefinitionResourceUri)
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
                            innerResourceUri = typeDefinitionResourceUri;
                        }
                        
                        if (!innerCompilationUnit.IsDefault())
                        {
                            var innerScopeSubIndex = PartialTypeDefinitionList[positionExclusive].ScopeSubIndex;
                            
                            for (int i = innerCompilationUnit.NodeOffset; i < innerCompilationUnit.NodeOffset + innerCompilationUnit.NodeLength; i++)
                            {
                                var x = NodeList[i];
                                
                                if (x.ParentScopeSubIndex == innerScopeSubIndex &&
                                    (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                                     x.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
                                     x.SyntaxKind == SyntaxKind.VariableDeclarationNode))
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
    
    private readonly List<SyntaxNodeValue> _getMemberList = new();
    
    internal List<SyntaxNodeValue> Internal_GetMemberList_TypeDefinitionNode(SyntaxNodeValue typeDefinitionNode)
    {
        _getMemberList.Clear();
    
        if (typeDefinitionNode.SelfScopeSubIndex == -1 ||
            !__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var compilationUnit))
        {
            return _getMemberList;
        }
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == typeDefinitionNode.SelfScopeSubIndex &&
                (node.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                 node.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
                 node.SyntaxKind == SyntaxKind.VariableDeclarationNode))
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
        
        var typeDefinitionTraits = TypeDefinitionTraitsList[typeDefinitionNode.TraitsIndex];
        if (typeDefinitionTraits.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionTraits.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionTraits.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit innerCompilationUnit;
                    ResourceUri innerResourceUri;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeSubIndex != -1)
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
                            var innerScopeSubIndex = partialTypeDefinition.ScopeSubIndex;
                            
                            if (partialTypeDefinition.ScopeSubIndex < innerCompilationUnit.ScopeLength)
                            {
                                var innerCodeBlockOwner = NodeList[innerCompilationUnit.NodeOffset + ScopeList[innerCompilationUnit.ScopeOffset + partialTypeDefinition.ScopeSubIndex].NodeSubIndex];
                                
                                if (innerCodeBlockOwner.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                                {
                                    var innerTypeDefinitionNode = innerCodeBlockOwner;
                                
                                    for (int i = innerCompilationUnit.NodeOffset; i < innerCompilationUnit.NodeOffset + innerCompilationUnit.NodeLength; i++)
                                    {
                                        var node = NodeList[i];
                                        
                                        if (node.ParentScopeSubIndex == innerScopeSubIndex &&
                                            (node.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                                             node.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
                                             node.SyntaxKind == SyntaxKind.VariableDeclarationNode))
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
    public IEnumerable<SyntaxNodeValue> GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(NamespaceStatementNode namespaceStatementNode)
    {
        if (namespaceStatementNode.SelfScopeSubIndex == -1 ||
            !__CompilationUnitMap.TryGetValue(namespaceStatementNode.ResourceUri, out var compilationUnit))
        {
            return Array.Empty<SyntaxNodeValue>();
        }

        var typeDefinitionNodeList = new List<SyntaxNodeValue>();
        
        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
            
            if (node.ParentScopeSubIndex == namespaceStatementNode.SelfScopeSubIndex && node.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                typeDefinitionNodeList.Add((SyntaxNodeValue)node);
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
    public IEnumerable<SyntaxNodeValue> GetTopLevelTypeDefinitionNodes_NamespaceGroup(NamespaceGroup namespaceGroup)
    {
        return namespaceGroup.NamespaceStatementNodeList
            .SelectMany(GetTopLevelTypeDefinitionNodes_NamespaceStatementNode);
    }
    
    private readonly List<SyntaxNodeValue> _getTopLevelTypeDefinitionNodes = new();

    /// <summary>Object-allocation-less version of the public version for internal use.</summary>
    internal List<SyntaxNodeValue> Internal_GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(
        NamespaceStatementNode namespaceStatementNode,
        bool shouldClear)
    {
        if (shouldClear)
        {
            // This allows Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(...) to "select many".
            _getTopLevelTypeDefinitionNodes.Clear();
        }
    
        if (namespaceStatementNode.SelfScopeSubIndex == -1 ||
            !__CompilationUnitMap.TryGetValue(namespaceStatementNode.ResourceUri, out var compilationUnit))
        {
            return _getTopLevelTypeDefinitionNodes;
        }

        for (int i = compilationUnit.NodeOffset; i < compilationUnit.NodeOffset + compilationUnit.NodeLength; i++)
        {
            var node = NodeList[i];
        
            if (node.ParentScopeSubIndex == namespaceStatementNode.SelfScopeSubIndex && node.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                _getTopLevelTypeDefinitionNodes.Add(node);
        }
        
        return _getTopLevelTypeDefinitionNodes;
    }
    
    /// <summary>Object-allocation-less version of the public version for internal use.</summary>
    internal List<SyntaxNodeValue> Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(NamespaceGroup namespaceGroup)
    {
        _getTopLevelTypeDefinitionNodes.Clear();
    
        foreach (var namespaceStatementNode in namespaceGroup.NamespaceStatementNodeList)
        {
            _ = Internal_GetTopLevelTypeDefinitionNodes_NamespaceStatementNode(namespaceStatementNode, shouldClear: false);
        }
        
        return _getTopLevelTypeDefinitionNodes;
    }
}
