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

public partial class CSharpBinder
{
	private readonly Dictionary<ResourceUri, CSharpCompilationUnit> _compilationUnitMap = new();
	//private readonly object _compilationUnitMapLock = new();

	/// <summary>
	/// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
	/// </summary>
    private readonly Dictionary<string, NamespaceGroup> _namespaceGroupMap = CSharpFacts.Namespaces.GetInitialBoundNamespaceStatementNodes();
    private readonly Dictionary<string, TypeDefinitionNode> _allTypeDefinitions = new();
    private readonly NamespaceStatementNode _topLevelNamespaceStatementNode = CSharpFacts.Namespaces.GetTopLevelNamespaceStatementNode();
    
    /// <summary>
	/// This is not thread safe to access because 'BindNamespaceStatementNode(...)' will directly modify the NamespaceGroup's List.
	/// </summary>
    public IReadOnlyDictionary<string, NamespaceGroup> NamespaceGroupMap => _namespaceGroupMap;
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions => _allTypeDefinitions;
    
    public NamespacePrefixTree NamespacePrefixTree { get; } = new();
    
    public NamespaceStatementNode TopLevelNamespaceStatementNode => _topLevelNamespaceStatementNode;
    
    public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> CSharpParserModel_ParseChildScopeStack { get; } = new();
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode ExpressionNode)> CSharpParserModel_ExpressionList { get; set; } = new();
    public List<SyntaxKind> CSharpParserModel_TryParseExpressionSyntaxKindList { get; } = new();
    public HashSet<int> SolutionWide_MinimumLocalsData_ScopeIndexKey_HashSet { get; } = new();
    
    public TokenWalker CSharpParserModel_TokenWalker { get; } = new(Array.Empty<SyntaxToken>(), useDeferredParsing: true);
    
    public AmbiguousIdentifierExpressionNode CSharpParserModel_AmbiguousIdentifierExpressionNode { get; } = new AmbiguousIdentifierExpressionNode(
		default,
        genericParameterListing: default,
        CSharpFacts.Types.Void.ToTypeReference());
        
    public TypeClauseNode CSharpParserModel_TypeClauseNode { get; } = new TypeClauseNode(
		typeIdentifier: default,
		valueType: null,
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
    public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> CSharpStatementBuilder_ParseChildScopeStack { get; } = new();
    
    public TextEditorService TextEditorService { get; set; }
    
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
        		var x = namespaceGroupNodeKvp.Value.NamespaceStatementNodeList[i];
        		
        		if (x.ResourceUri == resourceUri)
        			namespaceGroupNodeKvp.Value.NamespaceStatementNodeList.RemoveAt(i);
        	}
        }
    }

	/// <summary><see cref="StartCompilationUnit"/></summary>
	public void FinalizeCompilationUnit(CSharpCompilationUnit compilationUnit)
	{
		UpsertCompilationUnit(compilationUnit);
	}

    public void BindDiscard(
        SyntaxToken identifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	compilationUnit.__SymbolList.Add(
    		new Symbol(
        		SyntaxKind.DiscardSymbol,
	        	parserModel.GetNextSymbolId(),
	        	identifierToken.TextSpan with
		        {
		            DecorationByte = (byte)GenericDecorationKind.None,
		        }));
    }

    public void BindFunctionDefinitionNode(
        FunctionDefinitionNode functionDefinitionNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var functionIdentifierText = functionDefinitionNode.FunctionIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, parserModel.Binder.TextEditorService);

        var functionSymbol = new Symbol(
        	SyntaxKind.FunctionSymbol,
        	parserModel.GetNextSymbolId(),
        	functionDefinitionNode.FunctionIdentifierToken.TextSpan with
	        {
	            DecorationByte = (byte)GenericDecorationKind.Function
	        });

        compilationUnit.__SymbolList.Add(functionSymbol);

        //if (!TryAddFunctionDefinitionNodeByScope(
        //		compilationUnit,
        //		compilationUnit.ResourceUri,
        //		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
        //		functionIdentifierText,
        //        functionDefinitionNode))
        //{
        //    /*DiagnosticHelper.ReportAlreadyDefinedFunction(
        //    	compilationUnit.__DiagnosticList,
        //        functionDefinitionNode.FunctionIdentifierToken.TextSpan,
        //        functionIdentifierText);*/
        //}
    }

    public void SetCurrentNamespaceStatementNode(
        NamespaceStatementNode namespaceStatementNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        parserModel.CurrentNamespaceStatementNode = namespaceStatementNode;
    }

    public void BindNamespaceStatementNode(
        NamespaceStatementNode namespaceStatementNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var namespaceString = namespaceStatementNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);

        if (_namespaceGroupMap.TryGetValue(namespaceString, out var inNamespaceGroupNode))
        {
        	inNamespaceGroupNode.NamespaceStatementNodeList.Add(namespaceStatementNode);
        }
        else
        {
            _namespaceGroupMap.Add(namespaceString, new NamespaceGroup(
                namespaceString,
                new List<NamespaceStatementNode> { namespaceStatementNode }));
                
            var fullNamespaceName = namespaceStatementNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
            
            var splitResult = fullNamespaceName.Split('.');
            
            NamespacePrefixNode? namespacePrefixNode = null;
            
            foreach (var namespacePrefix in splitResult)
            {
                namespacePrefixNode = parserModel.Binder.NamespacePrefixTree.AddNamespacePrefix(namespacePrefix, namespacePrefixNode);
            }
        }
    }

    public void BindVariableDeclarationNode(
        VariableDeclarationNode variableDeclarationNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel,
        bool shouldCreateVariableSymbol = true)
    {
    	if (shouldCreateVariableSymbol)
        	CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind, compilationUnit, ref parserModel);
        	
        var text = variableDeclarationNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
        
        if (TryGetVariableDeclarationNodeByScope(
        		compilationUnit,
        		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
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
        			compilationUnit,
        			parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                	text,
                	variableDeclarationNode);
            }

            DiagnosticHelper.ReportAlreadyDefinedVariable(
            	compilationUnit.__DiagnosticList,
                variableDeclarationNode.IdentifierToken.TextSpan,
                text);
        }
        else
        {
        	_ = TryAddVariableDeclarationNodeByScope(
        		compilationUnit,
    			parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
            	text,
            	variableDeclarationNode);
        }
    }
    
    public void BindLabelDeclarationNode(
        LabelDeclarationNode labelDeclarationNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	compilationUnit.__SymbolList.Add(
        	new Symbol(
        		SyntaxKind.LabelSymbol,
            	parserModel.GetNextSymbolId(),
            	labelDeclarationNode.IdentifierToken.TextSpan with
            	{
            	    DecorationByte = (byte)GenericDecorationKind.None
            	}));
        	
        var text = labelDeclarationNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
        
        if (TryGetLabelDeclarationNodeByScope(
        		compilationUnit,
        		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
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
        			compilationUnit,
        			parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                	text,
                	labelDeclarationNode);
            }

            DiagnosticHelper.ReportAlreadyDefinedLabel(
            	compilationUnit.__DiagnosticList,
                labelDeclarationNode.IdentifierToken.TextSpan,
                text);
        }
        else
        {
        	_ = TryAddLabelDeclarationNodeByScope(
        		compilationUnit,
    			parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
            	text,
            	labelDeclarationNode);
        }
    }

    public VariableReferenceNode ConstructAndBindVariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var text = variableIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
        VariableReferenceNode? variableReferenceNode;

        if (TryGetVariableDeclarationHierarchically(
        		compilationUnit,
                parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                text,
                out var variableDeclarationNode)
            && variableDeclarationNode is not null)
        {
            variableReferenceNode = parserModel.ConstructOrRecycleVariableReferenceNode(
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
                compilationUnit.ResourceUri)
            {
                IsFabricated = true,
            };

            variableReferenceNode = parserModel.ConstructOrRecycleVariableReferenceNode(
                variableIdentifierToken,
                variableDeclarationNode);

            /*compilationUnit.BinderSession.DiagnosticBag.ReportUndefinedVariable(
                variableIdentifierToken.TextSpan,
                text);*/
        }

        CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, variableDeclarationNode.VariableKind, compilationUnit, ref parserModel);
        return variableReferenceNode;
    }
    
    public void BindLabelReferenceNode(
        LabelReferenceNode labelReferenceNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        compilationUnit.__SymbolList.Add(
        	new Symbol(
        		SyntaxKind.LabelSymbol,
            	parserModel.GetNextSymbolId(),
            	labelReferenceNode.IdentifierToken.TextSpan with
            	{
            	    DecorationByte = (byte)GenericDecorationKind.None
            	}));
    }

    public void BindConstructorDefinitionIdentifierToken(
        SyntaxToken identifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var constructorSymbol = new Symbol(
        	SyntaxKind.ConstructorSymbol,
	        parserModel.GetNextSymbolId(),
	        identifierToken.TextSpan with
	        {
	            DecorationByte = (byte)GenericDecorationKind.Type
	        });

        compilationUnit.__SymbolList.Add(constructorSymbol);
    }

    public void BindFunctionInvocationNode(
        FunctionInvocationNode functionInvocationNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var functionInvocationIdentifierText = functionInvocationNode
            .FunctionInvocationIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);

        var functionSymbol = new Symbol(
        	SyntaxKind.FunctionSymbol,
        	parserModel.GetNextSymbolId(),
        	functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
	        {
	            DecorationByte = (byte)GenericDecorationKind.Function
	        });

        compilationUnit.__SymbolList.Add(functionSymbol);

        if (TryGetFunctionHierarchically(
        		compilationUnit,
                parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                functionInvocationIdentifierText,
                out var functionDefinitionNode) &&
            functionDefinitionNode is not null)
        {
            functionInvocationNode.ResultTypeReference = functionDefinitionNode.ReturnTypeReference;
        }
        else
        {
            /*compilationUnit.BinderSession.DiagnosticBag.ReportUndefinedFunction(
                functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan,
                functionInvocationIdentifierText);*/
        }
    }

    public void BindNamespaceReference(
        SyntaxToken namespaceIdentifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        var namespaceSymbol = new Symbol(
        	SyntaxKind.NamespaceSymbol,
        	parserModel.GetNextSymbolId(),
        	namespaceIdentifierToken.TextSpan with
	        {
	            DecorationByte = (byte)GenericDecorationKind.None
	        });

        compilationUnit.__SymbolList.Add(namespaceSymbol);
    }

    public void BindTypeClauseNode(
        TypeClauseNode typeClauseNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        if (!typeClauseNode.IsKeywordType)
        {
            var typeSymbol = new Symbol(
            	SyntaxKind.TypeSymbol,
            	parserModel.GetNextSymbolId(),
            	typeClauseNode.TypeIdentifierToken.TextSpan with
	            {
	                DecorationByte = (byte)GenericDecorationKind.Type
	            });

            compilationUnit.__SymbolList.Add(typeSymbol);
        }

        var matchingTypeDefintionNode = CSharpFacts.Types.TypeDefinitionNodes.SingleOrDefault(
            x => x.TypeIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService) == typeClauseNode.TypeIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService));

        if (matchingTypeDefintionNode is not null)
        {
        	typeClauseNode.SetValueType(matchingTypeDefintionNode.ValueType);
        }
        /*else
        {
        	if (TryGetTypeDefinitionHierarchically(
	        		compilationUnit,
	                compilationUnit.BinderSession.ResourceUri,
	                compilationUnit.BinderSession.CurrentScopeIndexKey,
	                typeClauseNode.TypeIdentifierToken.TextSpan.GetText(),
	                out var typeDefinitionNode) &&
	            typeDefinitionNode is not null)
	        {
	            return;
	        }
	        else
	        {
	            // TODO: Diagnostics need to take the syntax token...
	        	// ...so they can lazily invoke TextSpan.GetText().
	        	DiagnosticHelper.ReportUndefinedTypeOrNamespace(
	            	compilationUnit.__DiagnosticList,
	                typeClauseNode.TypeIdentifierToken.TextSpan,
	                typeClauseNode.TypeIdentifierToken.TextSpan.GetText());
	        }
        }*/
    }
    
    /*
    // FindAllReferences
    public void BindTypeClauseNodeSuccessfully(
        TypeClauseNode typeClauseNode,
        TypeDefinitionNode typeDefinitionNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	typeDefinitionNode.ReferenceHashSet.Add(compilationUnit.ResourceUri);
    }
    */

    public void BindTypeIdentifier(
        SyntaxToken identifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        if (identifierToken.SyntaxKind == SyntaxKind.IdentifierToken)
        {
            var typeSymbol = new Symbol(
            	SyntaxKind.TypeSymbol,
            	parserModel.GetNextSymbolId(),
            	identifierToken.TextSpan with
	            {
	                DecorationByte = (byte)GenericDecorationKind.Type
	            });

            compilationUnit.__SymbolList.Add(typeSymbol);
        }
    }

    public void BindUsingStatementTuple(
        SyntaxToken usingKeywordToken,
        SyntaxToken namespaceIdentifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        AddNamespaceToCurrentScope(namespaceIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService), compilationUnit, ref parserModel);
    }
    
    public void BindTypeDefinitionNode(
        TypeDefinitionNode typeDefinitionNode,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel,
        bool shouldOverwrite = false)
    {
        var typeIdentifierText = typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
        var currentNamespaceStatementText = parserModel.CurrentNamespaceStatementNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
        var namespaceAndTypeIdentifiers = new NamespaceAndTypeIdentifiers(currentNamespaceStatementText, typeIdentifierText);

        typeDefinitionNode.EncompassingNamespaceIdentifierString = currentNamespaceStatementText;
        
       // if (TryGetTypeDefinitionNodeByScope(
       // 		compilationUnit,
       // 		compilationUnit.ResourceUri,
       // 		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
       // 		typeIdentifierText,
       // 		out var existingTypeDefinitionNode))
       // {
       // 	if (shouldOverwrite || existingTypeDefinitionNode.IsFabricated)
       // 	{
       // 		SetTypeDefinitionNodeByScope(
       // 			compilationUnit,
       // 			compilationUnit.ResourceUri,
	      //  		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
	      //  		typeIdentifierText,
	      //  		typeDefinitionNode);
       // 	}
       // 	else
       // 	{
       // 		/*DiagnosticHelper.ReportAlreadyDefinedType(
	      //      	compilationUnit.__DiagnosticList,
	      //          typeDefinitionNode.TypeIdentifierToken.TextSpan,
	      //          typeIdentifierText);*/
       // 	}
       // }
       // else
       // {
       // 	_ = TryAddTypeDefinitionNodeByScope(
       // 		compilationUnit,
    			//compilationUnit.ResourceUri,
       // 		parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
       // 		typeIdentifierText,
       // 		typeDefinitionNode);
       // }

        var success = _allTypeDefinitions.TryAdd(typeIdentifierText, typeDefinitionNode);
        if (!success)
        {
        	var entryFromAllTypeDefinitions = _allTypeDefinitions[typeIdentifierText];
        	
        	if (shouldOverwrite || entryFromAllTypeDefinitions.IsFabricated)
        		_allTypeDefinitions[typeIdentifierText] = typeDefinitionNode;
        }
    }

	/// <summary>
	/// If the 'codeBlockBuilder.ScopeIndexKey' is null then a scope will be instantiated
	/// added to the list of scopes. The 'codeBlockBuilder.ScopeIndexKey' will then be set
	/// to the instantiated scope's 'IndexKey'. As well, the current scope index key will be set to the
	/// instantiated scope's 'IndexKey'.
	/// 
	/// Also will update the 'parserModel.CurrentCodeBlockBuilder'.
	/// </summary>
    public void NewScopeAndBuilderFromOwner(
    	ICodeBlockOwner codeBlockOwner,
        TextEditorTextSpan textSpan,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        /*#if DEBUG
    	Console.WriteLine($"-------NewSBin: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------NewSBin: has console.write... that needs commented out");
    	#endif*/
    
    	if (codeBlockOwner.Unsafe_SelfIndexKey != -1)
    	{
			parserModel.CurrentCodeBlockOwner = codeBlockOwner;

			// TODO: This does not catch nearly as many infinite loop cases as I initially thought it would...
			//       ...When the token walker sets the token index for deferred parsing,
			//       a new instance of the node ends up being parsed.
			//
			// throw new WalkTextEditorException($"{nameof(NewScopeAndBuilderFromOwner)} codeBlockOwner.ScopeIndexKey is NOT '-1'; an infinite loop? _{codeBlockOwner.SyntaxKind}");
		}
    	
    	codeBlockOwner.Unsafe_ParentIndexKey = parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey;
    	codeBlockOwner.Scope_StartInclusiveIndex = textSpan.StartInclusiveIndex;

		codeBlockOwner.Unsafe_SelfIndexKey = compilationUnit.NodeList.Count;
		compilationUnit.NodeList.Add(codeBlockOwner);

		var parent = parserModel.GetParent(codeBlockOwner, compilationUnit);
    	
    	var parentScopeDirection = parent?.ScopeDirectionKind ?? ScopeDirectionKind.Both;
        if (parentScopeDirection == ScopeDirectionKind.Both)
        	codeBlockOwner.PermitCodeBlockParsing = false;
    
        parserModel.CurrentCodeBlockOwner = codeBlockOwner;
        
        parserModel.Binder.OnBoundScopeCreatedAndSetAsCurrent(codeBlockOwner, compilationUnit, ref parserModel);
        
        /*#if DEBUG
    	Console.WriteLine($"-------NewSBout: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------NewSBout: has console.write... that needs commented out");
    	#endif*/
    }
    
    /// <summary>
    /// 'NewScopeAndBuilderFromOwner' takes a 'ref CSharpParserModel parserModel',
    /// but the 'CSharpParserModel' takes the global scope in its constructor.
    ///
    /// TODO: Determine a better solution.
    /// </summary>
    public ICodeBlockOwner NewScopeAndBuilderFromOwner_GlobalScope_Hack(
    	ICodeBlockOwner codeBlockOwner,
        TextEditorTextSpan textSpan,
        CSharpCompilationUnit compilationUnit)
    {
    	if (codeBlockOwner.Unsafe_SelfIndexKey != -1)
    	{
    		// TODO: This does not catch nearly as many infinite loop cases as I initially thought it would...
    		//       ...When the token walker sets the token index for deferred parsing,
    		//       a new instance of the node ends up being parsed.
    		throw new WalkTextEditorException($"{nameof(NewScopeAndBuilderFromOwner)} codeBlockBuilder.ScopeIndexKey is NOT '-1'; an infinite loop?");
		}
    	
    	codeBlockOwner.Unsafe_ParentIndexKey = -1;
    	codeBlockOwner.Unsafe_SelfIndexKey = 0;
    	codeBlockOwner.Scope_StartInclusiveIndex = textSpan.StartInclusiveIndex;
    	codeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;

        compilationUnit.NodeList.Add(codeBlockOwner);
    	
    	return codeBlockOwner;
    }
    
    public void SetCurrentScopeAndBuilder(
    	ICodeBlockOwner codeBlockOwner, CSharpCompilationUnit compilationUnit, ref CSharpParserModel parserModel)
    {
    	/*#if DEBUG
    	Console.WriteLine($"-------SetSBin: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------SetSBin: has console.write... that needs commented out");
    	#endif*/
    
    	if (codeBlockOwner.Unsafe_SelfIndexKey == -1)
    		throw new WalkTextEditorException($"{nameof(SetCurrentScopeAndBuilder)} codeBlockBuilder.CodeBlockBuilder.ScopeIndexKey is '-1'. Invoke {NewScopeAndBuilderFromOwner}?");
    
		parserModel.CurrentCodeBlockOwner = codeBlockOwner;
		
		/*#if DEBUG
    	Console.WriteLine($"-------SetSBout: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------SetSBout: has console.write... that needs commented out");
    	#endif*/
    }

    public void AddNamespaceToCurrentScope(
        string namespaceString,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
        if (_namespaceGroupMap.TryGetValue(namespaceString, out var namespaceGroup) &&
            namespaceGroup.ConstructorWasInvoked)
        {
            var typeDefinitionNodes = GetTopLevelTypeDefinitionNodes_NamespaceGroup(namespaceGroup);
            
            foreach (var typeDefinitionNode in typeDefinitionNodes)
            {
        		var matchNode = compilationUnit.ExternalTypeDefinitionList.FirstOrDefault(x => GetIdentifierText(x, compilationUnit) == GetIdentifierText(typeDefinitionNode, compilationUnit));
            	
            	if (matchNode is null)
            	    compilationUnit.ExternalTypeDefinitionList.Add(typeDefinitionNode);
            }
        }
    }

    public void CloseScope(
        TextEditorTextSpan textSpan,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	/*#if DEBUG
    	Console.WriteLine($"-------{nameof(CloseScope)}in: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------{nameof(CloseScope)}in: has console.write... that needs commented out");
    	#endif*/
    
    	// Check if it is the global scope, if so return early.
    	if (parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey == 0)
    		return;
    	
    	parserModel.CurrentCodeBlockOwner.Scope_EndExclusiveIndex = textSpan.EndExclusiveIndex;
		parserModel.CurrentCodeBlockOwner = parserModel.GetParent(parserModel.CurrentCodeBlockOwner, compilationUnit);
		
		/*#if DEBUG
    	Console.WriteLine($"-------{nameof(CloseScope)}out: {parserModel.CurrentCodeBlockBuilder.CodeBlockOwner.SyntaxKind}");
    	#else
    	Console.WriteLine($"-------{nameof(CloseScope)}out: has console.write... that needs commented out");
    	#endif*/
    }

	/// <summary>
	/// Returns the 'symbolId: compilationUnit.BinderSession.GetNextSymbolId();'
	/// that was used to construct the ITextEditorSymbol.
	/// </summary>
    public int CreateVariableSymbol(
        SyntaxToken identifierToken,
        VariableKind variableKind,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	var symbolId = parserModel.GetNextSymbolId();
    	
        switch (variableKind)
        {
            case VariableKind.Field:
                compilationUnit.__SymbolList.Add(
                	new Symbol(
                		SyntaxKind.FieldSymbol,
	                	symbolId,
	                	identifierToken.TextSpan with
		                {
		                    DecorationByte = (byte)GenericDecorationKind.Field
		                }));
                break;
            case VariableKind.Property:
                compilationUnit.__SymbolList.Add(
                	new Symbol(
                		SyntaxKind.PropertySymbol,
                		symbolId,
                		identifierToken.TextSpan with
		                {
		                    DecorationByte = (byte)GenericDecorationKind.Property
		                }));
                break;
            case VariableKind.EnumMember:
            	compilationUnit.__SymbolList.Add(
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
                compilationUnit.__SymbolList.Add(
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
            var keepStatements = namespaceGroupNodeKvp.Value.NamespaceStatementNodeList
                .Where(x => x.ResourceUri != resourceUri)
                .ToList();

            _namespaceGroupMap[namespaceGroupNodeKvp.Key] =
                new NamespaceGroup(
                    namespaceGroupNodeKvp.Value.NamespaceString,
                    keepStatements);
        }

		_compilationUnitMap.Remove(resourceUri);
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

    public ICodeBlockOwner? GetScope(CSharpCompilationUnit compilationUnit, TextEditorTextSpan textSpan)
    {
    	return GetScopeByPositionIndex(compilationUnit, textSpan.StartInclusiveIndex);
    }
    
    public ICodeBlockOwner? GetScopeByPositionIndex(CSharpCompilationUnit compilationUnit, int positionIndex)
    {
        var possibleScopes = compilationUnit.NodeList.Where(x =>
        {
            if (!ICodeBlockOwner.ImplementsICodeBlockOwner(x.SyntaxKind))
                return false;
        
            var codeBlockOwner = (ICodeBlockOwner)x;
        
            return codeBlockOwner.Scope_StartInclusiveIndex <= positionIndex &&
            	   // Global Scope awkwardly has '-1' ending index exclusive (2023-10-15)
                   (codeBlockOwner.Scope_EndExclusiveIndex >= positionIndex || codeBlockOwner.Scope_EndExclusiveIndex == -1);
        });

        // TODO: Does MinBy return default when previous Where result is empty?
		var tuple = possibleScopes.MinBy(x => positionIndex - ((ICodeBlockOwner)x).Scope_StartInclusiveIndex);
		if (tuple is null)
		    return null;
	    else
	        return (ICodeBlockOwner)tuple;
    }

	/// <summary>
	/// 'validationNode' avoids an infinite loop when invoking this within a while loop or etc...
	/// 
	/// The infinite loop happens due to using statements / namespace statements bringing in types that come from a different
	/// CSharpCompilationUnit instance.
	/// 
	/// So checking that the compilationUnit contains the provided validationNode at 'validationNode.Unsafe_SelfIndexKey'
    /// ensures that the compilationUnit is the same instance.
	/// </summary>
	public ICodeBlockOwner? GetScopeByScopeIndexKey(CSharpCompilationUnit compilationUnit, int scopeIndexKey)
    {
        if (scopeIndexKey < 0)
            return null;

        if (scopeIndexKey < compilationUnit.NodeList.Count)
        {
            var isValid = true;

            if (isValid)
            {
                var node = compilationUnit.NodeList[scopeIndexKey];
                if (ICodeBlockOwner.ImplementsICodeBlockOwner(node.SyntaxKind))
				    return (ICodeBlockOwner)node;
		    }
        }
        
        return null;
    }
    
    public bool TryGetCompilationUnit(ResourceUri resourceUri, out CSharpCompilationUnit compilationUnit)
    {
    	var success = _compilationUnitMap.TryGetValue(resourceUri, out var x);
    	compilationUnit = x;
    	return success;
    }
    
    public void UpsertCompilationUnit(CSharpCompilationUnit compilationUnit)
    {
    	try
    	{
    		if (_compilationUnitMap.ContainsKey(compilationUnit.ResourceUri))
	    		_compilationUnitMap[compilationUnit.ResourceUri] = compilationUnit;
	    	else
	    		_compilationUnitMap.Add(compilationUnit.ResourceUri, compilationUnit);
    	}
    	catch (Exception e)
    	{
    		Console.WriteLine(e);
    	}
    }
    
    public bool RemoveCompilationUnit(ResourceUri resourceUri)
    {
    	return _compilationUnitMap.Remove(resourceUri);
    }
    
    public TypeDefinitionNode[] GetTypeDefinitionNodesByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey)
    {
    	var query = compilationUnit.NodeList
    		.Where(kvp => kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.TypeDefinitionNode)
    		.Select(kvp => (TypeDefinitionNode)kvp);
    		
		if (scopeIndexKey == 0)
		    query = query.Concat(compilationUnit.ExternalTypeDefinitionList);
		
		return query.ToArray();
    }
    
    public bool TryGetTypeDefinitionNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string typeIdentifierText,
    	out TypeDefinitionNode typeDefinitionNode)
    {
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x =>
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
		var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
                	                                                 x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                	                                                 GetIdentifierText(x, compilationUnit) == typeIdentifierText);
    	
    	if (matchNode is null)
    	{
    	    compilationUnit.NodeList.Add(typeDefinitionNode);
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
    	return compilationUnit.NodeList
    		.Where(kvp => kvp.Unsafe_ParentIndexKey == scopeIndexKey && kvp.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
    		.Select(kvp => (FunctionDefinitionNode)kvp)
    		.ToArray();
    }
    
    public bool TryGetFunctionDefinitionNodeByScope(
    	CSharpCompilationUnit compilationUnit,
    	int scopeIndexKey,
    	string functionIdentifierText,
    	out FunctionDefinitionNode functionDefinitionNode)
    {
    	var matchNode = compilationUnit.NodeList.FirstOrDefault(x => x.Unsafe_ParentIndexKey == scopeIndexKey &&
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
	        	    if (TryGetCompilationUnit(definitionTuple.ResourceUri, out var innerCompilationUnit))
	        	    {
	        	        return GetDefinitionNode(
    	        			innerCompilationUnit,
    	        			new TextEditorTextSpan(
    				            definitionTuple.StartInclusiveIndex,
    						    definitionTuple.StartInclusiveIndex + 1,
    						    default),
    	        			externalSyntaxKind,
    	        			getTextResult: textSpan.GetText(compilationUnit.SourceText, TextEditorService));
	        	    }
	        	}
	        }
        }

        return null;
    }

    public ISyntaxNode? GetSyntaxNode(CSharpCompilationUnit compilationUnit, int positionIndex, CSharpResource? compilerServiceResource)
    {
        return null;
        
        /*var scope = GetScopeByPositionIndex(compilationUnit, resourceUri, positionIndex);
        if (!scope.ConstructorWasInvoked)
        	return null;
        
        IReadOnlyList<ISyntax> childList;
        	
        var codeBlockOwner = scope.CodeBlockOwner;
        
        if (codeBlockOwner is not null)
        	childList = codeBlockOwner.CodeBlock.ChildList;
        else
        	childList = null;
        
        if (childList is null)
        	return null;
        
        var possibleNodeList = new List<ISyntaxNode>();
        
        ISyntaxNode? fallbackDefinitionNode = null;
        
        foreach (var child in childList)
        {
        	if (child is not ISyntaxNode node)
    			continue;
    		
    		if (node.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
    			node.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
    		{
    			fallbackDefinitionNode = node;
    		}
        	
        	var nodePositionIndices = GetNodePositionIndices(node, resourceUri);
        	if (nodePositionIndices == (-1, -1))
        		continue;
        		
        	if (nodePositionIndices.StartInclusiveIndex <= positionIndex &&
        		nodePositionIndices.EndExclusiveIndex >= positionIndex)
        	{
        		possibleNodeList.Add(node);
        	}
        }
        
        if (possibleNodeList.Count <= 0)
        {
        	if (fallbackDefinitionNode is not null)
        	{
        		if (fallbackDefinitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
        			fallbackDefinitionNode.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
        		{
        			var fallbackCodeBlockOwner = ((ICodeBlockOwner)fallbackDefinitionNode);
        			TextEditorTextSpan? fallbackTextSpan = null;
        			
        			if (fallbackCodeBlockOwner.OpenCodeBlockTextSpan.ConstructorWasInvoked)
        				fallbackTextSpan = fallbackCodeBlockOwner.OpenCodeBlockTextSpan;
        			else if (fallbackCodeBlockOwner.CloseCodeBlockTextSpan.ConstructorWasInvoked)
        				fallbackTextSpan = fallbackCodeBlockOwner.CloseCodeBlockTextSpan;
        				
        			if (fallbackTextSpan is not null && compilerServiceResource is not null)
        			{
        				var fallbackScope = GetScopeByPositionIndex(compilationUnit, resourceUri, fallbackTextSpan.Value.StartInclusiveIndex);
        				// RETROSPECTIVE: Shouldn't it be checking the 'fallbackScope.ConstructorWasInvoked'?
        				if (scope.ConstructorWasInvoked)
        					return GetFallbackNode(compilationUnit, positionIndex, resourceUri, compilerServiceResource, fallbackScope);
        			}
        		}
        	}

        	return null;
        }
        	
        var closestNode = possibleNodeList.MinBy(node =>
        {
        	// TODO: Wasteful re-invocation of this method, can probably do this in one invocation.
        	var nodePositionIndices = GetNodePositionIndices(node, resourceUri);
        	if (nodePositionIndices == (-1, -1))
        		return int.MaxValue;
        	
        	return positionIndex - nodePositionIndices.StartInclusiveIndex;
        });
        
        if (closestNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
        	return GetChildNodeOrSelfByPositionIndex(closestNode, resourceUri, positionIndex);
        
        return closestNode;
        */
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
    			/*#if DEBUG
    			Console.WriteLine($"method: '{nameof(GetNodePositionIndices)}' The {nameof(SyntaxKind)}: '{syntaxNode}' defaulted in switch statement.");
    			#endif*/
    			
    			return (-1, -1);
    		}
    	}
    }
    
    public void OnBoundScopeCreatedAndSetAsCurrent(ICodeBlockOwner codeBlockOwner, CSharpCompilationUnit compilationUnit, ref CSharpParserModel parserModel)
    {
    	switch (codeBlockOwner.SyntaxKind)
    	{
    		case SyntaxKind.NamespaceStatementNode:
    			var namespaceStatementNode = (NamespaceStatementNode)codeBlockOwner;
	    		var namespaceString = namespaceStatementNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
	        	parserModel.Binder.AddNamespaceToCurrentScope(namespaceString, compilationUnit, ref parserModel);
	        	
			    parserModel.Binder.BindNamespaceStatementNode((NamespaceStatementNode)codeBlockOwner, compilationUnit, ref parserModel);
	        	return;
			case SyntaxKind.LambdaExpressionNode:
				var lambdaExpressionNode = (LambdaExpressionNode)codeBlockOwner;
	    		foreach (var variableDeclarationNode in lambdaExpressionNode.VariableDeclarationNodeList)
		    	{
		    		parserModel.Binder.BindVariableDeclarationNode(variableDeclarationNode, compilationUnit, ref parserModel);
		    	}
		    	return;
		    case SyntaxKind.TryStatementCatchNode:
		    	// (2025-03-13) Bug: this is showing as a tooltip for things above it...
		    	// ...because it takes the TypeClauseNode position indices from 'codeBlockOwner'
		    	var tryStatementCatchNode = (TryStatementCatchNode)codeBlockOwner;
    		
	    		if (tryStatementCatchNode.VariableDeclarationNode is not null)
		    		parserModel.Binder.BindVariableDeclarationNode(tryStatementCatchNode.VariableDeclarationNode, compilationUnit, ref parserModel);
		    		
		    	return;
		    case SyntaxKind.TypeDefinitionNode:
		    
				parserModel.Binder.BindTypeDefinitionNode((TypeDefinitionNode)codeBlockOwner, compilationUnit, ref parserModel, true);
		    
		    	var typeDefinitionNode = (TypeDefinitionNode)codeBlockOwner;
		    	
		    	if (typeDefinitionNode.GenericParameterListing.ConstructorWasInvoked)
		    	{
		    		foreach (var entry in typeDefinitionNode.GenericParameterListing.GenericParameterEntryList)
		    		{
		    			parserModel.Binder.BindTypeDefinitionNode(
					        new TypeDefinitionNode(
								AccessModifierKind.Public,
								hasPartialModifier: false,
								StorageModifierKind.Class,
								entry.TypeReference.TypeIdentifierToken,
								entry.TypeReference.ValueType,
								entry.TypeReference.GenericParameterListing,
								primaryConstructorFunctionArgumentListing: default,
								inheritedTypeReference: TypeFacts.NotApplicable.ToTypeReference(),
								string.Empty,
								compilationUnit.ResourceUri
								// FindAllReferences
								// ,referenceHashSet: new()
								),
					        compilationUnit,
					        ref parserModel);
		    		}
		    	}
		    	
		    	return;
    	}
    }
    
    public ICodeBlockOwner SetOpenCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_StartInclusiveIndex, List<TextEditorDiagnostic> diagnosticList, TokenWalker tokenWalker)
    {
		if (codeBlockOwner.CodeBlock_StartInclusiveIndex == -1)
			ICodeBlockOwner.ThrowMultipleScopeDelimiterException(diagnosticList, tokenWalker);

		codeBlockOwner.CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
		return codeBlockOwner;
    }
    
    public ICodeBlockOwner SetCloseCodeBlockTextSpan(ICodeBlockOwner codeBlockOwner, int codeBlock_EndExclusiveIndex, List<TextEditorDiagnostic> diagnosticList, TokenWalker tokenWalker)
    {
		if (codeBlockOwner.CodeBlock_EndExclusiveIndex == -1)
			ICodeBlockOwner.ThrowMultipleScopeDelimiterException(diagnosticList, tokenWalker);

		codeBlockOwner.CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
		return codeBlockOwner;
    }
	
	public string GetIdentifierText(ISyntaxNode node, CSharpCompilationUnit compilationUnit)
	{
		switch (node.SyntaxKind)
		{
		    case SyntaxKind.TypeDefinitionNode:
		    {
		        var typeDefinitionNode = (TypeDefinitionNode)node;
    	
        	    string sourceText;
        	    
        	    if (typeDefinitionNode.ResourceUri == compilationUnit.ResourceUri)
        	    {
        	        sourceText = compilationUnit.SourceText;
        	    }
        	    else
        	    {
        	        if (TryGetCompilationUnit(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
        	            sourceText = innerCompilationUnit.SourceText;
        	        else
    	                return string.Empty;
        	    }
        	    
				return typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(sourceText, TextEditorService);
			}
			case SyntaxKind.TypeClauseNode:
			{
				var typeClauseNode = (TypeClauseNode)node;
				return typeClauseNode.TypeIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
			}
			case SyntaxKind.FunctionDefinitionNode:
			{
				var functionDefinitionNode = (FunctionDefinitionNode)node;
				return functionDefinitionNode.FunctionIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
			}
			case SyntaxKind.FunctionInvocationNode:
			{
				var functionInvocationNode = (FunctionInvocationNode)node;
				return functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
			}
			case SyntaxKind.VariableDeclarationNode:
			{
				var variableDeclarationNode = (VariableDeclarationNode)node;
				return variableDeclarationNode.IdentifierToken.TextSpan.GetText(compilationUnit.SourceText, TextEditorService);
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
		    !_compilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var compilationUnit))
	    {
			return Array.Empty<ISyntaxNode>();
        }
        
		var query = compilationUnit.NodeList
		    .Where(x => x.Unsafe_ParentIndexKey == typeDefinitionNode.Unsafe_SelfIndexKey &&
    		                (x.SyntaxKind == SyntaxKind.TypeDefinitionNode ||
    		                 x.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
    		                 x.SyntaxKind == SyntaxKind.VariableDeclarationNode))
		    .Select(x => (ISyntaxNode)x);
		
        if (typeDefinitionNode.PrimaryConstructorFunctionArgumentListing.FunctionArgumentEntryList is not null)
        {
            query = query.Concat(typeDefinitionNode.PrimaryConstructorFunctionArgumentListing.FunctionArgumentEntryList.Select(
                x => x.VariableDeclarationNode));
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
		    !_compilationUnitMap.TryGetValue(namespaceStatementNode.ResourceUri, out var compilationUnit))
	    {
			return Array.Empty<TypeDefinitionNode>();
        }

		return compilationUnit.NodeList
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
