using Microsoft.JSInterop;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Common.RazorLib.Dialogs.Models;

/// <summary>
/// TODO: Some methods just invoke a single method, so remove the redundant middle man.
/// TODO: Thread safety.
/// </summary>
public class DialogService : IDialogService
{
	private readonly IJSRuntime _jsRuntime;

    public DialogService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    private WalkCommonJavaScriptInteropApi _jsRuntimeCommonApi;
    private DialogState _dialogState = new();
    
    private WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => _jsRuntimeCommonApi
		??= _jsRuntime.GetWalkCommonApi();
		
	public event Action? DialogStateChanged;
	
	public event Action? ActiveDialogKeyChanged;
	
	public DialogState GetDialogState() => _dialogState;
    
    public void ReduceRegisterAction(IDialog dialog)
    {
    	var inState = GetDialogState();
    	
        if (inState.DialogList.Any(x => x.DynamicViewModelKey == dialog.DynamicViewModelKey))
        {
        	_ = Task.Run(async () =>
        		await JsRuntimeCommonApi
	                .FocusHtmlElementById(dialog.DialogFocusPointHtmlElementId)
	                .ConfigureAwait(false));
        	
        	DialogStateChanged?.Invoke();
        	return;
        }

		var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.Add(dialog);

        _dialogState = inState with 
        {
            DialogList = outDialogList,
            ActiveDialogKey = dialog.DynamicViewModelKey,
        };
        
        DialogStateChanged?.Invoke();
        return;
    }

    public void ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized)
    {
    	var inState = GetDialogState();
    	
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
            DialogStateChanged?.Invoke();
        	return;
        }
            
        var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        
        outDialogList[indexDialog] = inDialog.SetDialogIsMaximized(isMaximized);

        _dialogState = inState with { DialogList = outDialogList };
        
        DialogStateChanged?.Invoke();
        return;
    }
    
    public void ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    	
        _dialogState = inState with { ActiveDialogKey = dynamicViewModelKey };
        
        ActiveDialogKeyChanged?.Invoke();
        return;
    }

    public void ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
        	DialogStateChanged?.Invoke();
        	return;
        }

		var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.RemoveAt(indexDialog);

        _dialogState = inState with { DialogList = outDialogList };
        
        DialogStateChanged?.Invoke();
        return;
    }
}