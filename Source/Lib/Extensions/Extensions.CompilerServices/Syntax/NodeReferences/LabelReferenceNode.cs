using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LabelReferenceNode : IExpressionNode
{
    public LabelReferenceNode(SyntaxToken identifierToken)
    {
        IdentifierToken = identifierToken;
    }

    public SyntaxToken IdentifierToken { get; }

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LabelReferenceNode;
}


