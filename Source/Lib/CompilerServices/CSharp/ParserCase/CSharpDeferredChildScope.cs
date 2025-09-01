using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpDeferredChildScope
{
    public CSharpDeferredChildScope(
        int openTokenIndex,
        int closeTokenIndex,
        int indexCodeBlockValue)
    {
        OpenTokenIndex = openTokenIndex;
        CloseTokenIndex = closeTokenIndex;
        IndexCodeBlockValue = indexCodeBlockValue;
    }
    
    public int OpenTokenIndex { get; }
    public int CloseTokenIndex { get; }
    public int IndexCodeBlockValue { get; }
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserModel parserModel)
    {
        var currentCodeBlockValue = parserModel.Binder.CodeBlockValueList[IndexCodeBlockValue];
        currentCodeBlockValue.PermitCodeBlockParsing = true;
        parserModel.Binder.CodeBlockValueList[IndexCodeBlockValue] = currentCodeBlockValue;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
