using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
	public CSharpDeferredChildScope(
		int openTokenIndex,
		int closeTokenIndex,
		ICodeBlockOwner codeBlockOwner)
	{
		OpenTokenIndex = openTokenIndex;
		CloseTokenIndex = closeTokenIndex;
		CodeBlockOwner = codeBlockOwner;
	}
	
	public int OpenTokenIndex { get; }
	public int CloseTokenIndex { get; }
	public ICodeBlockOwner CodeBlockOwner { get; }
	
	public void PrepareMainParserLoop(int tokenIndexToRestore, CSharpCompilationUnit compilationUnit, ref CSharpParserModel parserModel)
	{
		parserModel.Binder.SetCurrentScopeAndBuilder(
			CodeBlockOwner,
			compilationUnit,
			ref parserModel);
		
		parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing = true;
		
		parserModel.TokenWalker.DeferredParsing(
			OpenTokenIndex,
			CloseTokenIndex,
			tokenIndexToRestore);
	}
}
