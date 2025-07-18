using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices;
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
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode ExpressionNode)> CSharpParserModel_ExpressionList { get; set; } = new();
    public List<SyntaxKind> CSharpParserModel_TryParseExpressionSyntaxKindList { get; } = new();
    public HashSet<string> CSharpParserModel_ClearedPartialDefinitionHashSet { get; } = new();
    
    public TokenWalker CSharpParserModel_TokenWalker { get; } = new(Array.Empty<SyntaxToken>(), useDeferredParsing: true);
    
    public AmbiguousIdentifierExpressionNode CSharpParserModel_AmbiguousIdentifierExpressionNode { get; } = new AmbiguousIdentifierExpressionNode(
		default,
        genericParameterListing: default,
        CSharpFacts.Types.Void.ToTypeReference());
        
    public TypeClauseNode CSharpParserModel_TypeClauseNode { get; } = new TypeClauseNode(
		typeIdentifier: default,
		genericParameterListing: default,
		isKeywordType: false);
		
	public VariableReferenceNode CSharpParserModel_VariableReferenceNode { get; } = new VariableReferenceNode(
		variableIdentifierToken: default,
		variableDeclarationNode: null);
	
	public BadExpressionNode Shared_BadExpressionNode { get; } = new BadExpressionNode(
		CSharpFacts.Types.Void.ToTypeReference(),
		EmptyExpressionNode.Empty,
		EmptyExpressionNode.Empty);
    
    public List<ISyntax> CSharpStatementBuilder_ChildList { get; } = new();
    public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> CSharpStatementBuilder_ParseLambdaStatementScopeStack { get; } = new();
    
    public TextEditorService TextEditorService { get; set; }
    
    public GlobalCodeBlockNode GlobalCodeBlockNode { get; } = new GlobalCodeBlockNode();
    
    public CSharpBinder(TextEditorService textEditorService)
    {
    	TextEditorService = textEditorService;
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
	public void FinalizeCompilationUnit(CSharpCompilationUnit compilationUnit)
	{
		UpsertCompilationUnit(compilationUnit);
	}
	
	/// <summary>TextEditorEditContext is required for thread safety.</summary>
    public void UpsertCompilationUnit(CSharpCompilationUnit compilationUnit)
    {
		if (__CompilationUnitMap.ContainsKey(compilationUnit.ResourceUri))
    		__CompilationUnitMap[compilationUnit.ResourceUri] = compilationUnit;
    	else
    		__CompilationUnitMap.Add(compilationUnit.ResourceUri, compilationUnit);
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
        var possibleScopes = compilationUnit.CodeBlockOwnerList.Where(x =>
        {
            return x.Scope_StartInclusiveIndex <= positionIndex &&
            	   // Global Scope awkwardly has '-1' ending index exclusive (2023-10-15)
                   (x.Scope_EndExclusiveIndex >= positionIndex || x.Scope_EndExclusiveIndex == -1);
        });

        // TODO: Does MinBy return default when previous Where result is empty?
		var tuple = possibleScopes.MinBy(x => positionIndex - x.Scope_StartInclusiveIndex);
		if (tuple is null)
		    return null;
	    else
	        return tuple;
    }

	public ICodeBlockOwner? GetScopeByScopeIndexKey(CSharpCompilationUnit compilationUnit, int scopeIndexKey)
    {
        if (scopeIndexKey < 0)
            return null;

        if (scopeIndexKey < compilationUnit.CodeBlockOwnerList.Count)
        {
            var isValid = true;

            if (isValid)
                return compilationUnit.CodeBlockOwnerList[scopeIndexKey];
        }
        
        return null;
    }
    
    public TypeDefinitionNode[] GetTypeDefinitionNodesByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey)
    {
    	var query = compilationUnit.CodeBlockOwnerList
    		.Where(kvp => kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.TypeDefinitionNode)
    		.Select(kvp => (TypeDefinitionNode)kvp);
    		
		if (scopeIndexKey == 0)
		    query = query.Concat(compilationUnit.ExternalTypeDefinitionList);
		
		return query.ToArray();
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
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

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
            	localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
			}
		}

        typeDefinitionNode = null;
        return false;
    }
    
    public bool TryGetTypeDefinitionNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string typeIdentifierText,
    	out TypeDefinitionNode typeDefinitionNode)
    {
    	var matchNode = compilationUnit.CodeBlockOwnerList.FirstOrDefault(x =>
    	{
    	    if (x.Unsafe_ParentIndexKey != scopeIndexKey ||
    	        x.SyntaxKind != SyntaxKind.TypeDefinitionNode)
    	    {
    	        return false;
    	    }
    	    
    	    return GetIdentifierText(x, compilationUnit) == typeIdentifierText;
	    });
    	
    	if (matchNode is null)
    	{
    	    if (scopeIndexKey == 0)
    	    {
    	         var externalMatchNode = compilationUnit.ExternalTypeDefinitionList.FirstOrDefault(x => GetIdentifierText(x, compilationUnit) == typeIdentifierText);
    	         if (externalMatchNode is not null)
    	         {
    	             typeDefinitionNode = (TypeDefinitionNode)externalMatchNode;
    	             return true;
    	         }
    	    }
    	
    	    typeDefinitionNode = null;
    	    return false;
    	}
    	else
    	{
    	    typeDefinitionNode = (TypeDefinitionNode)matchNode;
    	    return true;
    	}
    }
    
    public bool TryAddTypeDefinitionNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string typeIdentifierText,
        TypeDefinitionNode typeDefinitionNode)
    {
		var matchNode = compilationUnit.CodeBlockOwnerList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == typeIdentifierText);
    	
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
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

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
            	localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
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
    	var matchNode = compilationUnit.CodeBlockOwnerList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.FunctionDefinitionNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == functionIdentifierText);
    	
    	if (matchNode is null)
    	{
    	    functionDefinitionNode = null;
    	    return false;
    	}
    	else
    	{
    	    functionDefinitionNode = (FunctionDefinitionNode)matchNode;
    	    return true;
    	}
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
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

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
            	localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
        }

        variableDeclarationStatementNode = null;
        return false;
    }
    
    public bool TryGetVariableDeclarationNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string variableIdentifierText,
    	out VariableDeclarationNode variableDeclarationNode)
    {
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == variableIdentifierText);
    	
    	if (matchNode is null)
    	{
    	    variableDeclarationNode = null;
    	    return false;
    	}
    	else
    	{
    	    variableDeclarationNode = (VariableDeclarationNode)matchNode;
    	    return true;
    	}
    }
    
    public bool TryAddVariableDeclarationNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string variableIdentifierText,
        VariableDeclarationNode variableDeclarationNode)
    {
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == variableIdentifierText);
    	
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
    	var index = compilationUnit.NodeList.FindIndex(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
        	                                                x.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
        	                                                GetIdentifierText(x, compilationUnit) == variableIdentifierText);

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
        var localScope = GetScopeByScopeIndexKey(compilationUnit, initialScopeIndexKey);

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
            	localScope = GetScopeByScopeIndexKey(compilationUnit, localScope.Unsafe_ParentIndexKey);
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
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == labelIdentifierText);
    	
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
    
    public bool TryAddLabelDeclarationNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string labelIdentifierText,
        LabelDeclarationNode labelDeclarationNode)
    {
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == labelIdentifierText);
    	
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
    	var index = compilationUnit.NodeList.FindIndex(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
        	                                                x.SyntaxKind == SyntaxKind.LabelDeclarationNode &&
        	                                                GetIdentifierText(x, compilationUnit) == labelIdentifierText);

		if (index != -1)
		{
		    labelDeclarationNode.Unsafe_ParentIndexKey = scopeIndexKey;
		    compilationUnit.NodeList[index] = labelDeclarationNode;
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
    public ISyntaxNode? GetDefinitionNode(CSharpCompilationUnit compilationUnit, TextEditorTextSpan textSpan, SyntaxKind syntaxKind, Symbol? symbol = null, string? getTextResult = null)
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
        				compilationUnit,
        				scope.Unsafe_SelfIndexKey,
		                getTextResult ?? textSpan.GetText(compilationUnit.SourceText, TextEditorService),
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
	        				 compilationUnit,
        					 scope.Unsafe_SelfIndexKey,
		                     getTextResult ?? textSpan.GetText(compilationUnit.SourceText, TextEditorService),
		                     out var functionDefinitionNode)
		                 && functionDefinitionNode is not null)
		        {
		            if (functionDefinitionNode.IndexMethodOverloadDefinition != -1 &&
		                compilationUnit.FunctionInvocationParameterMetadataList is not null &&
		                symbol is not null)
		            {
		                var functionParameterList = compilationUnit.FunctionInvocationParameterMetadataList
		                    .Where(x => x.IdentifierStartInclusiveIndex == symbol.Value.TextSpan.StartInclusiveIndex)
		                    .ToList();
		            
		                for (int i = functionDefinitionNode.IndexMethodOverloadDefinition; i < MethodOverloadDefinitionList.Count; i++)
		                {
		                    var entry = MethodOverloadDefinitionList[i];
		                    
		                    if (__CompilationUnitMap.TryGetValue(entry.ResourceUri, out var innerCompilationUnit))
		                    {
		                        var innerFunctionDefinitionNode = (FunctionDefinitionNode)innerCompilationUnit.CodeBlockOwnerList[entry.ScopeIndexKey];
		                        
		                        if (innerFunctionDefinitionNode.FunctionArgumentListing.FunctionArgumentEntryList.Count == functionParameterList.Count)
		                        {
		                            for (int parameterIndex = 0; parameterIndex < innerFunctionDefinitionNode.FunctionArgumentListing.FunctionArgumentEntryList.Count; parameterIndex++)
		                            {
		                                var argument = innerFunctionDefinitionNode.FunctionArgumentListing.FunctionArgumentEntryList[parameterIndex];
		                                var parameter = functionParameterList[parameterIndex];
                                        
                                        string parameterTypeText;
                                        
                                        if (parameter.TypeReference.ExplicitDefinitionTextSpan != default &&
                                            __CompilationUnitMap.TryGetValue(parameter.TypeReference.ExplicitDefinitionResourceUri, out var parameterCompilationUnit))
                                        {
                                            parameterTypeText = parameter.TypeReference.TypeIdentifierToken.TextSpan.GetText(parameterCompilationUnit.SourceText, TextEditorService);
                                        }
                                        else
                                        {
                                            parameterTypeText = parameter.TypeReference.TypeIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
                                        }
		                                
		                                if (ArgumentModifierEqualsParameterModifier(argument.ArgumentModifierKind, parameter.ParameterModifierKind) &&
		                                    argument.VariableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan.GetText(innerCompilationUnit.SourceText, TextEditorService) ==
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
            				 compilationUnit,
        					 scope.Unsafe_SelfIndexKey,
                             getTextResult ?? textSpan.GetText(compilationUnit.SourceText, TextEditorService),
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
            		    textSpan.GetText(compilationUnit.SourceText, TextEditorService),
            		    out var namespacePrefixNode))
        		{
        		    return new NamespaceClauseNode(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
        		}
                
        		if (symbol is not null)
        		{
        		    var fullNamespaceName = symbol.Value.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
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
	        if (compilationUnit.SymbolIdToExternalTextSpanMap is not null)
	        {
	        	if (compilationUnit.SymbolIdToExternalTextSpanMap.TryGetValue(symbol.Value.SymbolId, out var definitionTuple))
	        	{
	        	    if (__CompilationUnitMap.TryGetValue(definitionTuple.ResourceUri, out var innerCompilationUnit))
	        	    {
	        	        return GetDefinitionNode(
    	        			innerCompilationUnit,
    	        			new TextEditorTextSpan(
    				            definitionTuple.StartInclusiveIndex,
    						    definitionTuple.StartInclusiveIndex + 1,
    						    default),
    	        			externalSyntaxKind,
    	        			symbol: symbol,
    	        			getTextResult: textSpan.GetText(compilationUnit.SourceText, TextEditorService));
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

    public ISyntaxNode? GetSyntaxNode(CSharpCompilationUnit compilationUnit, int positionIndex, CSharpResource? compilerServiceResource)
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
        			else if (variableDeclarationNode.TypeReference.GenericParameterListing.ConstructorWasInvoked)
        			{
        				foreach (var entry in variableDeclarationNode.TypeReference.GenericParameterListing.GenericParameterEntryList)
        				{
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
    /// 	  ...This should likely be changed, because function argument goto definition won't work if done from the argument listing, rather than the code block of the function.
    /// 	  This method will act as a temporary work around.
    /// </summary>
    public ISyntaxNode? GetFallbackNode(CSharpCompilationUnit compilationUnit, int positionIndex, CSharpResource compilerServiceResource, ICodeBlockOwner codeBlockOwner)
    {
        if (compilerServiceResource.CompilationUnit is null)
        	return null;
        
        // Try to find a symbol at that cursor position.
		IReadOnlyList<Symbol> symbolList = compilerServiceResource.CompilationUnit?.SymbolList ?? Array.Empty<Symbol>();
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
    }
    
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
    /// 	"Yes, C#/.Net methods require memory on per-AppDomain basis, there is no per-instance cost of the methods/properties.
	/// 	
	/// 	Cost comes from:
	/// 	
	/// 	methods metadata (part of type) and IL. I'm not sure how long IL stays loaded as it really only needed to JIT so my guess it is loaded as needed and discarded.
	/// 	after method is JITed machine code stays till AppDomain is unloaded (or if compiled as neutral till process terminates)
	/// 	So instantiating 1 or 50 objects with 50 methods will not require different amount of memory for methods."
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
	
	public string GetIdentifierText(ISyntaxNode node, CSharpCompilationUnit compilationUnit)
	{
	    string sourceText;
	
		switch (node.SyntaxKind)
		{
		    case SyntaxKind.TypeDefinitionNode:
		    {
		        var typeDefinitionNode = (TypeDefinitionNode)node;
        	    if (typeDefinitionNode.ResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
				return typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.TypeClauseNode:
			{
				var typeClauseNode = (TypeClauseNode)node;
        	    if (typeClauseNode.ExplicitDefinitionResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (__CompilationUnitMap.TryGetValue(typeClauseNode.ExplicitDefinitionResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
				return typeClauseNode.TypeIdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.FunctionDefinitionNode:
			{
				var functionDefinitionNode = (FunctionDefinitionNode)node;
				if (functionDefinitionNode.ResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
				return functionDefinitionNode.FunctionIdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.FunctionInvocationNode:
			{
				var functionInvocationNode = (FunctionInvocationNode)node;
				if (functionInvocationNode.ResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (__CompilationUnitMap.TryGetValue(functionInvocationNode.ResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
				return functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.VariableDeclarationNode:
			{
				var variableDeclarationNode = (VariableDeclarationNode)node;
				if (variableDeclarationNode.ResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
				return variableDeclarationNode.IdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.VariableReferenceNode:
			{
				var variableReferenceNode = (VariableReferenceNode)node;
				return variableReferenceNode.VariableIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
			}
			case SyntaxKind.LabelDeclarationNode:
			{
				var labelDeclarationNode = (LabelDeclarationNode)node;
				return labelDeclarationNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
			}
			case SyntaxKind.LabelReferenceNode:
			{
				var labelReferenceNode = (LabelReferenceNode)node;
				return labelReferenceNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
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
        
		var query = compilationUnit.CodeBlockOwnerList
		    .Where(x => x.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
    		                (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
    		                 x.SyntaxKind == SyntaxKind.FunctionDefinitionNode))
		    .Select(x => (ISyntaxNode)x)
		    .Concat(compilationUnit.NodeList.Where(x => x.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
		                x.SyntaxKind == SyntaxKind.VariableDeclarationNode));
		
        if (typeDefinitionNode.PrimaryConstructorFunctionArgumentListing.FunctionArgumentEntryList is not null)
        {
            query = query.Concat(typeDefinitionNode.PrimaryConstructorFunctionArgumentListing.FunctionArgumentEntryList.Select(
                x => x.VariableDeclarationNode));
        }
        
        if (typeDefinitionNode.IndexPartialTypeDefinition != -1)
        {
            int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < PartialTypeDefinitionList.Count)
            {
                if (PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    CSharpCompilationUnit? innerCompilationUnit;
                    
                    if (PartialTypeDefinitionList[positionExclusive].ScopeIndexKey != -1)
        		    {
        		        if (PartialTypeDefinitionList[positionExclusive].ResourceUri != compilationUnit.ResourceUri)
        		        {
            		        if (__CompilationUnitMap.TryGetValue(PartialTypeDefinitionList[positionExclusive].ResourceUri, out var temporaryCompilationUnit))
            		            innerCompilationUnit = temporaryCompilationUnit;
            		        else
            		            innerCompilationUnit = null;
        		        }
        		        else
        		        {
        		            innerCompilationUnit = compilationUnit;
    		            }
    		            
    		            if (innerCompilationUnit != null)
    		            {
    		                var innerScopeIndexKey = PartialTypeDefinitionList[positionExclusive].ScopeIndexKey;
                            query = query.Concat(innerCompilationUnit.CodeBlockOwnerList
                    		    .Where(x =>
                		        {
                		            return x.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                		                (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
                		                 x.SyntaxKind == SyntaxKind.FunctionDefinitionNode);
        		                })
                    		    .Select(x => (ISyntaxNode)x))
                    		    .Concat(innerCompilationUnit.NodeList.Where(x => x.Unsafe_ParentIndexKey == innerScopeIndexKey &&
                    		                x.SyntaxKind == SyntaxKind.VariableDeclarationNode));
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
        
        return query;
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

		return compilationUnit.CodeBlockOwnerList
		    .Where(x => x.Unsafe_ParentIndexKey == namespaceStatementNode.Unsafe_SelfIndexKey && x.SyntaxKind == SyntaxKind.TypeDefinitionNode)
		    .Select(x => (TypeDefinitionNode)x);
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
}
