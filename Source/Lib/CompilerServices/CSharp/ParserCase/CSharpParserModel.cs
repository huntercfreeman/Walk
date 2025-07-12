using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.Facts;

namespace Walk.CompilerServices.CSharp.ParserCase;

/// <summary>
/// The computational state for the CSharpParser is contained within this type.
/// The output of the CSharpParser is the <see cref="CSharpCompilationUnit"/>.<see cref="CSharpCompilationUnit.RootCodeBlockNode"/>
/// </summary>
public struct CSharpParserModel
{
	/// <summary>
	/// Should 0 be the global scope?
	/// </summary>
	private int _indexKey = 0;
	
	private int _symbolId = 0;

    public CSharpParserModel(
        CSharpBinder binder,
        List<SyntaxToken> tokenList,
        ICodeBlockOwner currentCodeBlockOwner,
	    NamespaceStatementNode topLevelNamespaceStatementNode)
    {
    	Binder = binder;
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
		
		ClearedPartialDefinitionHashSet = binder.CSharpParserModel_ClearedPartialDefinitionHashSet;
		ClearedPartialDefinitionHashSet.Clear();
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
            
        return (ICodeBlockOwner)cSharpCompilationUnit.NodeList[codeBlockOwner.Unsafe_ParentIndexKey];
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
}
