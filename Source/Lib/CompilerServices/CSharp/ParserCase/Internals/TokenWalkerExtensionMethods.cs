using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

internal static class TokenWalkerExtensionMethods
{
    public static void DeferParsingOfChildScope(
        this TokenWalker tokenWalker,
        ref CSharpParserModel parserModel)
    {
        // Pop off the 'TypeDefinitionNode', then push it back on when later dequeued.
        var deferredCodeBlockBuilder = parserModel.CurrentCodeBlockOwner;
        
        parserModel.CurrentCodeBlockOwner = parserModel.GetParent(deferredCodeBlockBuilder, parserModel.Compilation);

        var openTokenIndex = tokenWalker.Index - 1;

        var openBraceCounter = 1;
        
        int closeTokenIndex;
        
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

        if (parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_DefinitionsOnly &&
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
