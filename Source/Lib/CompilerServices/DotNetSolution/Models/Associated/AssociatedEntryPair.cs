using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.DotNetSolution.Models.Associated;

public record AssociatedEntryPair : IAssociatedEntry
{
    public AssociatedEntryPair(
        SyntaxToken associatedNameToken,
        SyntaxToken associatedValueToken)
    {
        AssociatedNameToken = associatedNameToken;
        AssociatedValueToken = associatedValueToken;
    }

    public SyntaxToken AssociatedNameToken { get; init; }
    public SyntaxToken AssociatedValueToken { get; init; }

    public AssociatedEntryKind AssociatedEntryKind => AssociatedEntryKind.Pair;
}