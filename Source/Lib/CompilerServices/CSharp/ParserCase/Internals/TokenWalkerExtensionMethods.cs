using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

internal static class TokenWalkerExtensionMethods
{
    public static TypeClauseNode MatchTypeClauseNode(this TokenWalker tokenWalker, CSharpCompilationUnit compilationUnit, ref CSharpParserModel parserModel)
    {
        return ParseTypes.MatchTypeClause(compilationUnit, ref parserModel);
    }
    
    /*#if DEBUG
    private static readonly HashSet<int> _seenOpenTokenIndices = new();
    #endif*/

	public static void DeferParsingOfChildScope(
		this TokenWalker tokenWalker,
		CSharpCompilationUnit compilationUnit,
		ref CSharpParserModel parserModel)
    {    
		// Pop off the 'TypeDefinitionNode', then push it back on when later dequeued.
		var deferredCodeBlockBuilder = parserModel.CurrentCodeBlockOwner;
		
		parserModel.Binder.SetCurrentScopeAndBuilder(
			parserModel.GetParent(deferredCodeBlockBuilder, compilationUnit),
			compilationUnit,
			ref parserModel);

		var openTokenIndex = tokenWalker.Index - 1;
		
		/*#if DEBUG
		if (!_seenOpenTokenIndices.Add(openTokenIndex))
		{
			throw new NotImplementedException("aaa Infinite loop?");
		}
		#endif*/

		var openBraceCounter = 1;
		
		int closeTokenIndex;
		
		#if DEBUG
		parserModel.TokenWalker.SuppressProtectedSyntaxKindConsumption = true;
		#endif
		
		if (deferredCodeBlockBuilder.IsImplicitOpenCodeBlockTextSpan)
		{
			while (true)
			{
				if (tokenWalker.IsEof)
					break;
	
				if (tokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
					break;
	
				_ = tokenWalker.Consume();
			}
	
			closeTokenIndex = tokenWalker.Index;
			var statementDelimiterToken = tokenWalker.Match(SyntaxKind.StatementDelimiterToken);
		}
		else
		{
			while (true)
			{
				if (tokenWalker.IsEof)
					break;
	
				if (tokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
				{
					++openBraceCounter;
				}
				else if (tokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
				{
					if (--openBraceCounter <= 0)
						break;
				}
	
				_ = tokenWalker.Consume();
			}
	
			closeTokenIndex = tokenWalker.Index;
			var closeBraceToken = tokenWalker.Match(SyntaxKind.CloseBraceToken);
		}
		
		#if DEBUG
		parserModel.TokenWalker.SuppressProtectedSyntaxKindConsumption = false;
		#endif

		if (compilationUnit.CompilationUnitKind == CompilationUnitKind.SolutionWide_DefinitionsOnly &&
			deferredCodeBlockBuilder.SyntaxKind == SyntaxKind.FunctionDefinitionNode ||
    		deferredCodeBlockBuilder.SyntaxKind == SyntaxKind.ArbitraryCodeBlockNode)
		{
			return;
		}
		
		parserModel.ParseChildScopeStack.Push(
			(
				parserModel.CurrentCodeBlockOwner,
				new CSharpDeferredChildScope(
					openTokenIndex,
					closeTokenIndex,
					deferredCodeBlockBuilder)
			));
    }
}
