using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dialogs.Models;

public interface IDialogService
{
	public event Action? DialogStateChanged;
	public event Action? ActiveDialogKeyChanged;
	
	/// <summary>
	/// Capture the reference and re-use it,
	/// because the state will change out from under you, if you continually invoke this.
	/// </summary>
	public DialogState GetDialogState();

    public void ReduceRegisterAction(IDialog dialog);

    public void ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized);
    
    public void ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey);
    public void ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey);
}