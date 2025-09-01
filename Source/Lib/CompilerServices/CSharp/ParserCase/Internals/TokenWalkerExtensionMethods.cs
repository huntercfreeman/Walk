using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Utility;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

internal static class TokenWalkerExtensionMethods
{
    public static void DeferParsingOfChildScope(
        this TokenWalker tokenWalker,
        ref CSharpParserModel parserModel)
    {
        // Pop off the 'TypeDefinitionNode', then push it back on when later dequeued.
        var deferredScope = parserModel.Binder.ScopeList[parserModel.CurrentScopeOffset];
        
        parserModel.CurrentScopeOffset = deferredScope.ParentScopeSubIndex;

        var openTokenIndex = tokenWalker.Index - 1;

        var openBraceCounter = 1;
        
        int closeTokenIndex;
        
        if (deferredScope.IsImplicitOpenCodeBlockTextSpan)
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
            deferredScope.OwnerSyntaxKind == SyntaxKind.FunctionDefinitionNode ||
            deferredScope.OwnerSyntaxKind == SyntaxKind.ArbitraryCodeBlockNode)
        {
            return;
        }
        
        parserModel.ParseChildScopeStack.Push(
            (
                parserModel.CurrentScopeOffset,
                new CSharpDeferredChildScope(
                    openTokenIndex,
                    closeTokenIndex,
                    deferredScope.SelfScopeSubIndex)
            ));
    }
}
