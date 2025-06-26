using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Edits.Models;

namespace Walk.TextEditor.RazorLib.Edits.Displays;

public partial class DirtyResourceUriInteractiveIconDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IDirtyResourceUriService DirtyResourceUriService { get; set; } = null!;
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public string CssClassString { get; set; } = string.Empty;
    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;

	private const string _buttonId = "di_web_dirty-text-editor-models-interactive-icon_id";

    private static Key<IDynamicViewModel> _dialogRecordKey = Key<IDynamicViewModel>.NewKey();

    private readonly IDialog _dialogRecord = new DialogViewModel(
        _dialogRecordKey,
        "Unsaved Files",
        typeof(DirtyResourceUriViewDisplay),
        null,
        null,
		true,
		_buttonId);
		
	protected override void OnInitialized()
	{
		DirtyResourceUriService.DirtyResourceUriStateChanged += OnDirtyResourceUriStateChanged;
	}

    private void ShowDialogOnClick()
    {
        DialogService.ReduceRegisterAction(_dialogRecord);
    }
    
    public async void OnDirtyResourceUriStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	DirtyResourceUriService.DirtyResourceUriStateChanged -= OnDirtyResourceUriStateChanged;
    }
}