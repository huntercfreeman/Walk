using Walk.TextEditor.RazorLib.Diffs.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.TextEditor.RazorLib.Diffs.Displays.Internals;

public partial class DiffDetailsOutTextDisplay : ComponentBase
{
    [CascadingParameter]
    public TextEditorDiffResult DiffResult { get; set; } = null!;
}