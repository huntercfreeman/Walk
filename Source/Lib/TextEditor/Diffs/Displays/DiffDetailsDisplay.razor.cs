using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Diffs.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.TextEditor.RazorLib.Diffs.Displays;

public partial class DiffDetailsDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public Key<TextEditorDiffModel> DiffModelKey { get; set; } = Key<TextEditorDiffModel>.Empty;
    [Parameter, EditorRequired]
    public TextEditorDiffResult? DiffResult { get; set; } = null!;
}