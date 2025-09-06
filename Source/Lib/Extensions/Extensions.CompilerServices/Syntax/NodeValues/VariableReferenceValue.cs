using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct VariableReferenceValue
{
    public static VariableReferenceValue Empty { get; } = default;

    public VariableReferenceValue(
        SyntaxToken variableIdentifierToken,
        TypeReferenceValue resultTypeReference,
        bool isFabricated)
    {
        VariableIdentifierToken = variableIdentifierToken;
        ResultTypeReference = resultTypeReference;
        IsFabricated = isFabricated;
    }
    
    public VariableReferenceValue(VariableReferenceNode variableReferenceNode)
    {
        VariableIdentifierToken = variableReferenceNode.VariableIdentifierToken;
        ResultTypeReference = variableReferenceNode.ResultTypeReference;
        IsFabricated = variableReferenceNode.IsFabricated;
    }

    public SyntaxToken VariableIdentifierToken { get; }
    public TypeReferenceValue ResultTypeReference { get; }
    public bool IsFabricated { get; }
}
