using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LiteralExpressionNode : IExpressionNode
{
    public LiteralExpressionNode(SyntaxToken literalSyntaxToken, TypeReferenceValue typeReference)
    {
        LiteralSyntaxToken = literalSyntaxToken;
        ResultTypeReference = typeReference;
    }

    public SyntaxToken LiteralSyntaxToken { get; }
    public TypeReferenceValue ResultTypeReference { get; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LiteralExpressionNode;
}
