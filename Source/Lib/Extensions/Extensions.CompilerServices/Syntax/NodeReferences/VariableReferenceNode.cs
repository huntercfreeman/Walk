using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class VariableReferenceNode : IExpressionNode
{
    public VariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        VariableDeclarationNode variableDeclarationNode)
    {
        VariableIdentifierToken = variableIdentifierToken;
        //ResultTypeReference = variableDeclarationNode.TypeReference;
    }
    
    public VariableReferenceNode(VariableReferenceValue variableReference)
    {
        VariableIdentifierToken = variableReference.VariableIdentifierToken;
        IsFabricated = variableReference.IsFabricated;
    }

    public int ParentScopeSubIndex { get; set; }
    public bool _isFabricated;

    public SyntaxToken VariableIdentifierToken { get; set; }
    public TypeReferenceValue TypeReference { get; set; }
    public TypeReferenceValue ResultTypeReference { get; set; } = TypeFacts.Empty.ToTypeReference();

    public bool IsFabricated
    {
        get
        {
            return _isFabricated;
        }
        init
        {
            _isFabricated = value;
        }
    }
    public SyntaxKind SyntaxKind => SyntaxKind.VariableReferenceNode;
}
