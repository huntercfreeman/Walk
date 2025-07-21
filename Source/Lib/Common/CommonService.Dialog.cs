using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    /// <summary>
    /// TODO: Some methods just invoke a single method, so remove the redundant middle man.
    /// TODO: Thread safety.
    /// </summary>
    private DialogState _dialogState = new();
    
    public DialogState GetDialogState() => _dialogState;
    
    public void Dialog_ReduceRegisterAction(IDialog dialog)
    {
        var inState = GetDialogState();
        
        if (inState.DialogList.Any(x => x.DynamicViewModelKey == dialog.DynamicViewModelKey))
        {
            _ = Task.Run(async () =>
                await JsRuntimeCommonApi
                    .FocusHtmlElementById(dialog.DialogFocusPointHtmlElementId)
                    .ConfigureAwait(false));
            
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
            return;
        }

        var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.Add(dialog);

        _dialogState = inState with 
        {
            DialogList = outDialogList,
            ActiveDialogKey = dialog.DynamicViewModelKey,
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }

    public void Dialog_ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized)
    {
        var inState = GetDialogState();
        
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
            return;
        }
            
        var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        
        outDialogList[indexDialog] = inDialog.SetDialogIsMaximized(isMaximized);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
    
    public void Dialog_ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
        var inState = GetDialogState();
        
        _dialogState = inState with { ActiveDialogKey = dynamicViewModelKey };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.ActiveDialogKeyChanged);
        return;
    }

    public void Dialog_ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
        var inState = GetDialogState();
    
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
            return;
        }

        var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.RemoveAt(indexDialog);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
}
