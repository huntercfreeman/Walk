using Walk.Extensions.CompilerServices.Syntax.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public class EmptyCodeBlockOwner : ICodeBlockOwner
{
    public static readonly EmptyCodeBlockOwner Instance = new();
    private int _selfScopeSubIndex = 0;
    private int _parentScopeSubIndex = 0;

    public int SelfScopeSubIndex
    {
        get => 0;
        set => _ = value;
    }
    public int ParentScopeSubIndex
    {
        get => -1;
        set => _ = value;
    }
    public SyntaxKind SyntaxKind => SyntaxKind.EmptyCodeBlockOwner;

    public bool IsFabricated { get; init; }
}
