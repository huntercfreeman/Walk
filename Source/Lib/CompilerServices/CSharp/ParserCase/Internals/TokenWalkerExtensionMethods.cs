using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Utility;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

internal static class TokenWalkerExtensionMethods
{
    public static void DeferParsingOfChildScope(
        this TokenWalker tokenWalker,
        ref CSharpParserState parserModel)
    {
        // Pop off the 'TypeDefinitionNode', then push it back on when later dequeued.
        var deferredScope = parserModel.ScopeCurrent;
        
        parserModel.ScopeCurrentSubIndex = deferredScope.ParentScopeSubIndex;

        var openTokenIndex = tokenWalker.Index - 1;

        var openBraceCounter = 1;        int closeTokenIndex;        if (deferredScope.IsImplicitOpenCodeBlockTextSpan)
        {
            while (true)
            {
                if (tokenWalker.IsEof)
                    break;
    
                if (tokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
                    break;
    
                _ = tokenWalker.Consume();
            }
    
            closeTokenIndex = tokenWalker.Index;            _ = tokenWalker.Match(SyntaxKind.StatementDelimiterToken);
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
    
            closeTokenIndex = tokenWalker.Index;            _ = tokenWalker.Match(SyntaxKind.CloseBraceToken);
        }

        if (parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_DefinitionsOnly &&
            (deferredScope.OwnerSyntaxKind == SyntaxKind.FunctionDefinitionNode ||
             deferredScope.OwnerSyntaxKind == SyntaxKind.ArbitraryCodeBlockNode))
        {
            return;
        }
        
        parserModel.ParseChildScopeStack.Push(
            (
                parserModel.ScopeCurrentSubIndex,
                new CSharpDeferredChildScope(
                    openTokenIndex,
                    closeTokenIndex,
                    deferredScope.SelfScopeSubIndex)
            ));
    }
}
