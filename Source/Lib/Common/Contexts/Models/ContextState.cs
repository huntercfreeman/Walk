using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct ContextState(
    IReadOnlyList<ContextRecord> AllContextsList,
    Key<ContextRecord> FocusedContextKey)
{
    public ContextState() : this(
        Array.Empty<ContextRecord>(),
        Key<ContextRecord>.Empty)
    {
        AllContextsList = ContextFacts.AllContextsList;
        FocusedContextKey = ContextFacts.GlobalContext.ContextKey;
    }
}