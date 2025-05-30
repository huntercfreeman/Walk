using Walk.CompilerServices.DotNetSolution.Models.Associated;

namespace Walk.CompilerServices.DotNetSolution.Models;

public record DotNetSolutionHeader
{
    public DotNetSolutionHeader()
    {
    }

    public DotNetSolutionHeader(
            AssociatedEntryPair? formatVersionPair,
            AssociatedEntryPair? hashtagVisualStudioVersionPair,
            AssociatedEntryPair? exactVisualStudioVersionPair,
            AssociatedEntryPair? minimumVisualStudioVersionPair)
        : this()
    {
        FormatVersionPair = formatVersionPair;
        HashtagVisualStudioVersionPair = hashtagVisualStudioVersionPair;
        ExactVisualStudioVersionPair = exactVisualStudioVersionPair;
        MinimumVisualStudioVersionPair = minimumVisualStudioVersionPair;
    }

    public AssociatedEntryPair? FormatVersionPair { get; init; }
    public AssociatedEntryPair? HashtagVisualStudioVersionPair { get; init; }
    public AssociatedEntryPair? ExactVisualStudioVersionPair { get; init; }
    public AssociatedEntryPair? MinimumVisualStudioVersionPair { get; init; }
}
