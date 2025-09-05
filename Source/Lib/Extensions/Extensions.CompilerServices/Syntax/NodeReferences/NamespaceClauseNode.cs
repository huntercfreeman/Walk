using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class NamespaceClauseNode : IExpressionNode
{
    public NamespaceClauseNode(SyntaxToken identifierToken)
    {
        IdentifierToken = identifierToken;
    }
    
    public NamespaceClauseNode(SyntaxToken identifierToken, /*NamespacePrefixNode namespacePrefixNode,*/ int startOfMemberAccessChainPositionIndex)
    {
        IdentifierToken = identifierToken;
        // NamespacePrefixNode = namespacePrefixNode;
        StartOfMemberAccessChainPositionIndex = startOfMemberAccessChainPositionIndex;
    }

    public SyntaxToken IdentifierToken { get; set; }
    // public NamespacePrefixNode? NamespacePrefixNode { get; set; }
    public NamespaceClauseNode? PreviousNamespaceClauseNode { get; set; }
    public int StartOfMemberAccessChainPositionIndex { get; set; }

    TypeReferenceValue IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int ParentScopeSubIndex { get; set; }
    
    public bool _isFabricated;
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
    
    public SyntaxKind SyntaxKind => SyntaxKind.NamespaceClauseNode;
}
