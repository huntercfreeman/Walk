using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.Facts;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase;

/// <summary>
/// The computational state for the CSharpParser is contained within this type.
/// The output of the CSharpParser is the <see cref="CSharpCompilationUnit"/>.<see cref="CSharpCompilationUnit.RootCodeBlockNode"/>
/// </summary>
public struct CSharpParserModel
{
	/// <summary>
	/// 0 is the global scope
	/// </summary>
	private int _indexKey = 0;
	
	private int _symbolId = 0;

    public CSharpParserModel(
        CSharpBinder binder,
        CSharpCompilationUnit compilationUnit,
        List<SyntaxToken> tokenList,
        ICodeBlockOwner currentCodeBlockOwner,
	    NamespaceStatementNode topLevelNamespaceStatementNode)
    {
    	Binder = binder;
    	Compilation = compilationUnit;
	    CurrentCodeBlockOwner = currentCodeBlockOwner;
	    CurrentNamespaceStatementNode = topLevelNamespaceStatementNode;
    
    	TokenWalker = Binder.CSharpParserModel_TokenWalker;
    	TokenWalker.Reinitialize(tokenList);
    	
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
			valueType: null,
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
    }

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
				valueType,
				genericParameterListing,
				isKeywordType);
		}    
    	
    	TypeClauseNode.SetSharedInstance(
    		typeIdentifier,
			valueType,
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
        var functionIdentifierText = functionDefinitionNode.FunctionIdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);

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
        var namespaceString = namespaceStatementNode.IdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);

        if (Binder._namespaceGroupMap.TryGetValue(namespaceString, out var inNamespaceGroupNode))
        {
        	inNamespaceGroupNode.NamespaceStatementNodeList.Add(namespaceStatementNode);
        }
        else
        {
            Binder._namespaceGroupMap.Add(namespaceString, new NamespaceGroup(
                namespaceString,
                new List<NamespaceStatementNode> { namespaceStatementNode }));
                
            var fullNamespaceName = namespaceStatementNode.IdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
            
            var splitResult = fullNamespaceName.Split('.');
            
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
        	
        var text = variableDeclarationNode.IdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
        
        if (Binder.TryGetVariableDeclarationNodeByScope(
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
                Binder.SetVariableDeclarationNodeByScope(
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
        	_ = Binder.TryAddVariableDeclarationNodeByScope(
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
        	
        var text = labelDeclarationNode.IdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
        
        if (Binder.TryGetLabelDeclarationNodeByScope(
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
                Binder.SetLabelDeclarationNodeByScope(
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
        	_ = Binder.TryAddLabelDeclarationNodeByScope(
        		Compilation,
    			CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
            	text,
            	labelDeclarationNode);
        }
    }

    public VariableReferenceNode ConstructAndBindVariableReferenceNode(SyntaxToken variableIdentifierToken)
    {
        var text = variableIdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
        VariableReferenceNode? variableReferenceNode;

        if (Binder.TryGetVariableDeclarationHierarchically(
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
        var functionInvocationIdentifierText = functionInvocationNode
            .FunctionInvocationIdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);

        var functionSymbol = new Symbol(
        	SyntaxKind.FunctionSymbol,
        	GetNextSymbolId(),
        	functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
	        {
	            DecorationByte = (byte)GenericDecorationKind.Function
	        });

        Compilation.__SymbolList.Add(functionSymbol);

        if (Binder.TryGetFunctionHierarchically(
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
        
        // TODO: Cannot use ref, out, or in...
        var compilation = Compilation;
        var binder = Binder;

        var matchingTypeDefintionNode = CSharpFacts.Types.TypeDefinitionNodes.SingleOrDefault(
            x => x.TypeIdentifierToken.TextSpan.GetText(compilation.SourceText, binder.TextEditorService) == typeClauseNode.TypeIdentifierToken.TextSpan.GetText(compilation.SourceText, binder.TextEditorService));

        if (matchingTypeDefintionNode is not null)
        {
        	typeClauseNode.SetValueType(matchingTypeDefintionNode.ValueType);
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
        AddNamespaceToCurrentScope(namespaceIdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService));
    }
    
    public void BindTypeDefinitionNode(TypeDefinitionNode typeDefinitionNode, bool shouldOverwrite = false)
    {
        var typeIdentifierText = typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(Compilation.SourceText, Binder.TextEditorService);
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
        
        Binder.OnBoundScopeCreatedAndSetAsCurrent(codeBlockOwner, Compilation, ref this);
    }

    public void AddNamespaceToCurrentScope(string namespaceString)
    {
        if (Binder._namespaceGroupMap.TryGetValue(namespaceString, out var namespaceGroup) &&
            namespaceGroup.ConstructorWasInvoked)
        {
            var typeDefinitionNodes = Binder.GetTopLevelTypeDefinitionNodes_NamespaceGroup(namespaceGroup);
            
            // TODO: Cannot use ref, out, or in...
            var compilation = Compilation;
            var binder = Binder;
            
            foreach (var typeDefinitionNode in typeDefinitionNodes)
            {
        		var matchNode = Compilation.ExternalTypeDefinitionList.FirstOrDefault(x => binder.GetIdentifierText(x, compilation) == binder.GetIdentifierText(typeDefinitionNode, compilation));
            	
            	if (matchNode is null)
            	    Compilation.ExternalTypeDefinitionList.Add(typeDefinitionNode);
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
}
