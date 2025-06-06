using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.DotNetSolution.Models.Associated;

public record AssociatedEntryGroup : IAssociatedEntry
{
    public AssociatedEntryGroup(
        SyntaxToken openAssociatedGroupToken,
        List<IAssociatedEntry> associatedEntryList,
        SyntaxToken closeAssociatedGroupToken)
    {
        OpenAssociatedGroupToken = openAssociatedGroupToken;
        AssociatedEntryList = associatedEntryList;
        CloseAssociatedGroupToken = closeAssociatedGroupToken;
    }

    public SyntaxToken OpenAssociatedGroupToken { get; }
    public List<IAssociatedEntry> AssociatedEntryList { get; init; }
    public SyntaxToken CloseAssociatedGroupToken { get; }

    public AssociatedEntryKind AssociatedEntryKind => AssociatedEntryKind.Group;
}
