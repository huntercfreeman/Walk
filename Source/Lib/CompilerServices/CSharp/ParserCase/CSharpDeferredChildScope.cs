using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

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
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserModel parserModel)
    {
        parserModel.ScopeCurrentSubIndex = ScopeSubIndex;

        Scope scope = default;

        try
        {
            scope = parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + parserModel.ScopeCurrentSubIndex];
        }
        catch (Exception e)
        {

            throw;
        }
        


        scope.PermitCodeBlockParsing = true;
        parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + parserModel.ScopeCurrentSubIndex] = scope;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
