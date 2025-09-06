using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class TryStatementNode : ISyntaxNode
{
    public TryStatementNode(
        TryStatementTryNode? tryNode,
        TryStatementCatchNode? catchNode,
        TryStatementFinallyNode? finallyNode)
    {
        TryNode = tryNode;
        CatchNode = catchNode;
        FinallyNode = finallyNode;
    }

    public TryStatementTryNode? TryNode { get; set; }
    public TryStatementCatchNode? CatchNode { get; set; }
    public TryStatementFinallyNode? FinallyNode { get; set; }

    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.TryStatementNode;
}
