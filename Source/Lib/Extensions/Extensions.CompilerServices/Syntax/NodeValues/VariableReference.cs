using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

public record struct VariableReference
{
    public static VariableReference Empty { get; } = default;

    public VariableReference(
        SyntaxToken variableIdentifierToken,
        TypeReference resultTypeReference,
        bool isFabricated)
    {
        VariableIdentifierToken = variableIdentifierToken;
        ResultTypeReference = resultTypeReference;
        IsFabricated = isFabricated;
    }
    
    public VariableReference(VariableReferenceNode variableReferenceNode)
    {
        VariableIdentifierToken = variableReferenceNode.VariableIdentifierToken;
        ResultTypeReference = variableReferenceNode.ResultTypeReference;
        IsFabricated = variableReferenceNode.IsFabricated;
    }

    public SyntaxToken VariableIdentifierToken { get; }
    public TypeReference ResultTypeReference { get; }
    public bool IsFabricated { get; }
}
