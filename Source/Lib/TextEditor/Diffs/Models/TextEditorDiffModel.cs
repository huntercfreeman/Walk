using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Diffs.Models;

public record TextEditorDiffModel(
    Key<TextEditorDiffModel> DiffKey,
    Key<TextEditorViewModel> InViewModelKey,
    Key<TextEditorViewModel> OutViewModelKey)
{
    public Key<RenderState> RenderStateKey { get; init; } = Key<RenderState>.NewKey();
}
