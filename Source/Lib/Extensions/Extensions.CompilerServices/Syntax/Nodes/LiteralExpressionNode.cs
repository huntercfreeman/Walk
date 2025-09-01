using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LiteralExpressionNode : IExpressionNode
{
    public LiteralExpressionNode(SyntaxToken literalSyntaxToken, TypeReference typeReference)
    {
        LiteralSyntaxToken = literalSyntaxToken;
        ResultTypeReference = typeReference;
    }

    public SyntaxToken LiteralSyntaxToken { get; }
    public TypeReference ResultTypeReference { get; }

    public int ParentScopeOffset { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LiteralExpressionNode;
}
