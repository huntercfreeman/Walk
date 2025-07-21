namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
	public event Action? TextEditorStateChanged;
	
	public event Action? Diff_TextEditorDiffStateChanged;
	public event Action? DirtyResourceUriStateChanged;
	public event Action? FindAllStateChanged;
	public event Action? Group_TextEditorGroupStateChanged;
	/// <summary>
	/// Step 1: Notifies the TextEditorViewModelDisplay to recalculate `_componentData.SetWrapperCssAndStyle();`
	///         and invoke `StateHasChanged()`.
	/// </summary>
	public event Action? Options_StaticStateChanged;
	/// <summary>
	/// Step 1: Notifies the WalkTextEditorInitializer to measure a tiny UI element that has the options applied to it.
	/// Step 2: WalkTextEditorInitializer then invokes `MeasuredStateChanged`.
	/// Step 3: TextEditorViewModelDisplay sees that second event fire, it enqueues a re-calculation of the virtualization result.
	/// Step 4: Eventually that virtualization result is finished and the editor re-renders.
	/// </summary>
	public event Action? Options_NeedsMeasured;
	/// <summary>
	/// Step 1: Notifies TextEditorViewModelDisplay to enqueue a re-calculation of the virtualization result.
	/// Step 2: Eventually that virtualization result is finished and the editor re-renders.
	/// </summary>
	public event Action? Options_MeasuredStateChanged;
	/// <summary>
	/// This event communicates from the text editor UI to the header and footer.
	/// </summary>
	public event Action? Options_TextEditorWrapperCssStateChanged;
}
