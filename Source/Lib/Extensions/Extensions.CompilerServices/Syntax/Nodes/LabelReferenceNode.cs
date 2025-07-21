using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LabelReferenceNode : IExpressionNode
{
    public LabelReferenceNode(SyntaxToken identifierToken)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LabelReferenceNode++;
        #endif
    
        IdentifierToken = identifierToken;
    }

    public SyntaxToken IdentifierToken { get; }

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int Unsafe_ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LabelReferenceNode;

#if DEBUG
    ~LabelReferenceNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LabelReferenceNode--;
    }
    #endif
}


