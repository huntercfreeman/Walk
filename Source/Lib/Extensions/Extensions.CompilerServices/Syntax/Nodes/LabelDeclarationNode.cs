using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LabelDeclarationNode : IExpressionNode
{
    public LabelDeclarationNode(SyntaxToken identifierToken)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LabelDeclarationNode++;
        #endif
    
        IdentifierToken = identifierToken;
    }

    public SyntaxToken IdentifierToken { get; }

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int Unsafe_ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LabelDeclarationNode;

    #if DEBUG    
    ~LabelDeclarationNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LabelDeclarationNode--;
    }
    #endif
}

