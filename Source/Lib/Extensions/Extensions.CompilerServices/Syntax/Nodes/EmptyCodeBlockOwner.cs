using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public class EmptyCodeBlockOwner : ICodeBlockOwner
{
    public static readonly EmptyCodeBlockOwner Instance = new();

    public int SelfScopeSubIndex { get; set; } = 0;
    public int ParentScopeSubIndex { get; set; } = 0;

    public SyntaxKind SyntaxKind => SyntaxKind.EmptyCodeBlockOwner;

    public bool IsFabricated { get; init; }
}
