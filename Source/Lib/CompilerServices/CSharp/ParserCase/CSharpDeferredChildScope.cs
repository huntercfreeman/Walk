using Walk.Extensions.CompilerServices.Syntax;
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
        parserModel.CurrentScopeOffset = ScopeOffset;

        Scope scope = default;

        try
        {
            scope = parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + parserModel.CurrentScopeOffset];
        }
        catch (Exception e)
        {

            throw;
        }
        


        scope.PermitCodeBlockParsing = true;
        parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + parserModel.CurrentScopeOffset] = scope;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
