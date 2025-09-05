using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class VariableReferenceNode : IExpressionNode
{
    public VariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        VariableDeclarationNode variableDeclarationNode)
    {
        VariableIdentifierToken = variableIdentifierToken;
        //ResultTypeReference = variableDeclarationNode.TypeReference;
    }
    
    public VariableReferenceNode(VariableReference variableReference)
    {
        VariableIdentifierToken = variableReference.VariableIdentifierToken;
        IsFabricated = variableReference.IsFabricated;
    }

    public int ParentScopeSubIndex { get; set; }
    public bool _isFabricated;

    public SyntaxToken VariableIdentifierToken { get; set; }
    public TypeReference TypeReference { get; set; }
    public TypeReference ResultTypeReference { get; set; } = TypeFacts.Empty.ToTypeReference();

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
