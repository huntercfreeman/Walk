using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.CSharp.ParserCase;

public class CSharpStatementBuilder
{
    public CSharpStatementBuilder()
    {
        MostRecentNode = Walk.Extensions.CompilerServices.Syntax.Nodes.EmptyExpressionNode.Empty;
        ChildList.Clear();
        ParseLambdaStatementScopeStack.Clear();
    }
    
    public bool StatementIsEmpty => ChildList.Count == 0 &&
                                    MostRecentNode == Walk.Extensions.CompilerServices.Syntax.Nodes.EmptyExpressionNode.Empty;

    public List<SyntaxToken> ChildList { get; } = new();
    public ISyntaxNode MostRecentNode { get; set; }    /// <summary>
    /// Prior to finishing a statement, you must check whether ParseLambdaStatementScopeStack has a child that needs to be parsed.
    /// All currently known cases of finishing a statement will do so by invoking FinishStatement(...),
    /// this method will perform this check internally.'
    ///
    /// The ScopeSubIndex is that of the parent which contains the scope that was deferred.
    /// </summary>
    public Stack<(int ScopeSubIndex, CSharpDeferredChildScope DeferredChildScope)> ParseLambdaStatementScopeStack { get; } = new();
    
    /// <summary>Invokes the other overload with index: ^1</summary>
    public bool TryPeek(out SyntaxToken syntax)
    {
        return TryPeek(^1, out syntax);
    }
    
    /// <summary>^1 gives the last entry</summary>
    public bool TryPeek(Index index, out SyntaxToken syntax)
    {
        if (ChildList.Count - index.Value > -1)
        {
            syntax = ChildList[index];
            return true;
        }
        
        syntax = default;
        return false;
    }
    
    public SyntaxToken Pop()
    {
        var syntax = ChildList[^1];
        ChildList.RemoveAt(ChildList.Count - 1);
        return syntax;
    }

    /// <summary>
    /// If 'StatementDelimiterToken', 'OpenBraceToken', or 'CloseBraceToken'
    /// are parsed by the main loop,
    ///
    /// Then check that the last item in the StatementBuilder.ChildList
    /// has been added to the parserModel.CurrentCodeBlockBuilder.ChildList.
    ///
    /// If it was not yet added, then add it.
    ///
    /// Lastly, clear the StatementBuilder.ChildList.
    ///
    /// Returns the result of 'ParseLambdaStatementScopeStack.TryPop(out var deferredChildScope)'.
    /// </summary>
    public bool FinishStatement(int finishTokenIndex, ref CSharpParserModel parserModel)
    {
        switch (MostRecentNode.SyntaxKind)
        {
            case SyntaxKind.VariableReferenceNode:
            {
                parserModel.Return_VariableReferenceNode(
                    (Walk.Extensions.CompilerServices.Syntax.Nodes.VariableReferenceNode)MostRecentNode);
                break;
            }
            case SyntaxKind.FunctionInvocationNode:
            {
                parserModel.Return_FunctionInvocationNode(
                    (Walk.Extensions.CompilerServices.Syntax.Nodes.FunctionInvocationNode)MostRecentNode);
                break;
            }
            case SyntaxKind.ConstructorInvocationExpressionNode:
            {
                parserModel.Return_ConstructorInvocationExpressionNode(
                    (Walk.Extensions.CompilerServices.Syntax.Nodes.ConstructorInvocationExpressionNode)MostRecentNode);
                break;
            }
            case SyntaxKind.BinaryExpressionNode:
            {
                parserModel.Return_BinaryExpressionNode(
                    (Walk.Extensions.CompilerServices.Syntax.Nodes.BinaryExpressionNode)MostRecentNode);
                break;
            }
        }
    
        MostRecentNode = Walk.Extensions.CompilerServices.Syntax.Nodes.EmptyExpressionNode.Empty;
        ChildList.Clear();
        
        /*if (ParseLambdaStatementScopeStack.Count > 0)
        {
            var tuple = ParseLambdaStatementScopeStack.Peek();
            
            if (tuple.ScopeSubIndex == parserModel.ScopeCurrentSubIndex)
            {
                tuple = ParseLambdaStatementScopeStack.Pop();
                tuple.DeferredChildScope.PrepareMainParserLoop(finishTokenIndex, ref parserModel);
                return true;
            }
        }*/
        
        return false;
    }
}

