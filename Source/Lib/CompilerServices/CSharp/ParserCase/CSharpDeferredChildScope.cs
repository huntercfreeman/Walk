using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
	public CSharpDeferredChildScope(
		int openTokenIndex,
		int closeTokenIndex,
		CSharpCodeBlockBuilder codeBlockBuilder)
	{
		OpenTokenIndex = openTokenIndex;
		CloseTokenIndex = closeTokenIndex;
		CodeBlockBuilder = codeBlockBuilder;
	}
	
	public int OpenTokenIndex { get; }
	public int CloseTokenIndex { get; }
	public CSharpCodeBlockBuilder CodeBlockBuilder { get; }
	
	public void PrepareMainParserLoop(int tokenIndexToRestore, CSharpCompilationUnit compilationUnit, ref CSharpParserModel parserModel)
	{
		parserModel.Binder.SetCurrentScopeAndBuilder(
			CodeBlockBuilder,
			compilationUnit,
			ref parserModel);
		
		parserModel.CurrentCodeBlockBuilder.PermitCodeBlockParsing = true;
		
		parserModel.TokenWalker.DeferredParsing(
			OpenTokenIndex,
			CloseTokenIndex,
			tokenIndexToRestore);
	}
}
