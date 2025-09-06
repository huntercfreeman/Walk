namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
    public CSharpDeferredChildScope(
        int openTokenIndex,
        int closeTokenIndex,
        int scopeSubIndex)
    {
        OpenTokenIndex = openTokenIndex;
        CloseTokenIndex = closeTokenIndex;
        ScopeSubIndex = scopeSubIndex;
    }
    
    public int OpenTokenIndex { get; }
    public int CloseTokenIndex { get; }
    public int ScopeSubIndex { get; }
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserState parserModel)
    {
        parserModel.ScopeCurrentSubIndex = ScopeSubIndex;
        parserModel.SetCurrentScope_PermitCodeBlockParsing(true);
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
