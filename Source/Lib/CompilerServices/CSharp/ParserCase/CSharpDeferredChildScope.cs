using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

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
    
    public readonly void PrepareMainParserLoop(int tokenIndexToRestore, ref CSharpParserModel parserModel)
    {
        parserModel.CurrentCodeBlockOwner = CodeBlockOwner;
        
        parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing = true;
        
        parserModel.TokenWalker.DeferredParsing(
            OpenTokenIndex,
            CloseTokenIndex,
            tokenIndexToRestore);
    }
}
