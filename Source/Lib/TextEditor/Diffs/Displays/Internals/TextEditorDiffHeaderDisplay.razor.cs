using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.Diffs.Displays.Internals;

public partial class TextEditorDiffHeaderDisplay : ComponentBase, ITextEditorDependentComponent
{
	[Parameter, EditorRequired]
	public Key<TextEditorComponentData> ComponentDataKey { get; set; }

	private static readonly Throttle _throttleRender = new(TimeSpan.FromMilliseconds(1_000));

	protected override void OnInitialized()
    {
        // TextEditorViewModelSlimDisplay.RenderBatchChanged += OnRenderBatchChanged;
        OnRenderBatchChanged();
    }

	private void OnRenderBatchChanged()
    {
    	_throttleRender.Run(async _ =>
    	{
    		await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    	});
    }

	public void Dispose()
    {
    	// TextEditorViewModelSlimDisplay.RenderBatchChanged -= OnRenderBatchChanged;
    }
}