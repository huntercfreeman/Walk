using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LabelDeclarationNode : IExpressionNode
{
    public LabelDeclarationNode(SyntaxToken identifierToken)
    {
        IdentifierToken = identifierToken;
    }

    public SyntaxToken IdentifierToken { get; }

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LabelDeclarationNode;
}

