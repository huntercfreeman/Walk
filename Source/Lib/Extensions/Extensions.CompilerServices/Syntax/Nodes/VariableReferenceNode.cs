using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class VariableReferenceNode : IExpressionNode
{
    public VariableReferenceNode(
        SyntaxToken variableIdentifierToken,
        VariableDeclarationNode variableDeclarationNode)
    {
        VariableIdentifierToken = variableIdentifierToken;
        VariableDeclarationNode = variableDeclarationNode;
    }
    
    public VariableReferenceNode(VariableReference variableReference)
    {
        VariableIdentifierToken = variableReference.VariableIdentifierToken;
        IsFabricated = variableReference.IsFabricated;
    }

    public int ParentScopeSubIndex { get; set; }
    public bool _isFabricated;

    public SyntaxToken VariableIdentifierToken { get; set; }
    /// <summary>
    /// The <see cref="VariableDeclarationNode"/> is null when the variable is undeclared
    ///
    /// TODO: Do not store the VariableDeclarationNode
    /// </summary>
    public VariableDeclarationNode VariableDeclarationNode { get; set; }
    public TypeReference ResultTypeReference
    {
        get
        {
            if (VariableDeclarationNode is null)
                return TypeFacts.Empty.ToTypeReference();
            
            return VariableDeclarationNode.TypeReference;
        }
    }

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
