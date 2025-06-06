using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dialogs.Models;

/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct DialogState
{
	public DialogState()
	{
	}

    public IReadOnlyList<IDialog> DialogList { get; init; } = Array.Empty<IDialog>();
    /// <summary>
    /// The active dialog is either:<br/><br/>
    /// -the one which has focus within it,<br/>
    /// -most recently focused dialog,<br/>
    /// -most recently registered dialog
    /// <br/><br/>
    /// The motivation for this property is when two dialogs are rendered
    /// at the same time, and one overlaps the other. One of those
    /// dialogs is hidden by the other. To be able to 'bring to front'
    /// the dialog one is interested in by setting focus to it, is useful.
    /// </summary>
    public Key<IDynamicViewModel> ActiveDialogKey { get; init; } = Key<IDynamicViewModel>.Empty;
}