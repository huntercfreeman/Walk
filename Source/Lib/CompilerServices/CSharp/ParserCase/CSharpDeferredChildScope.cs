using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
    public CSharpDeferredChildScope(
        int openTokenIndex,
        int closeTokenIndex,
        int codeBlockValueSelfIndexKey)
    {
        OpenTokenIndex = openTokenIndex;
        CloseTokenIndex = closeTokenIndex;
        CodeBlockValueSelfIndexKey = codeBlockValueSelfIndexKey;
    }
    
    public int OpenTokenIndex { get; }
    public int CloseTokenIndex { get; }
    public int CodeBlockValueSelfIndexKey { get; }
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserModel parserModel)
    {
        var currentScope = parserModel.Binder.ScopeList[parserModel.Compilation.IndexCodeBlockOwnerList + CodeBlockValueSelfIndexKey];
        currentScope.PermitCodeBlockParsing = true;
        parserModel.Binder.ScopeList[IndexCodeBlockValue] = currentCodeBlockValue;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
