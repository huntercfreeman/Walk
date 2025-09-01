using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
    public CSharpDeferredChildScope(
        int openTokenIndex,
        int closeTokenIndex,
        int scopeOffset)
    {
        OpenTokenIndex = openTokenIndex;
        CloseTokenIndex = closeTokenIndex;
        ScopeOffset = scopeOffset;
    }
    
    public int OpenTokenIndex { get; }
    public int CloseTokenIndex { get; }
    public int ScopeOffset { get; }
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserModel parserModel)
    {
        var scope = parserModel.Binder.ScopeList[parserModel.Compilation.ScopeIndex + ScopeOffset];
        scope.PermitCodeBlockParsing = true;
        parserModel.Binder.ScopeList[parserModel.Compilation.ScopeIndex + ScopeOffset] = scope;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
