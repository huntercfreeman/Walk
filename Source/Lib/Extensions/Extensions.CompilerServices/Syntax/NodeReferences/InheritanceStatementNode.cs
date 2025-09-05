using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class InheritanceStatementNode : ISyntaxNode
{
    public InheritanceStatementNode(TypeClauseNode parentTypeClauseNode)
    {
        ParentTypeClauseNode = parentTypeClauseNode;
    }

    public TypeClauseNode ParentTypeClauseNode { get; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.InheritanceStatementNode;
}
