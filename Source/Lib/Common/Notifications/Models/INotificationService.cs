using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Notifications.Models;

public interface INotificationService
{
	public event Action? NotificationStateChanged;
	
	public NotificationState GetNotificationState();

    public void ReduceRegisterAction(INotification notification);
    public void ReduceDisposeAction(Key<IDynamicViewModel> key);
    public void ReduceMakeReadAction(Key<IDynamicViewModel> key);
    public void ReduceUndoMakeReadAction(Key<IDynamicViewModel> key);
    public void ReduceMakeDeletedAction(Key<IDynamicViewModel> key);
    public void ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key);
    public void ReduceMakeArchivedAction(Key<IDynamicViewModel> key);
    public void ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key);
    public void ReduceClearDefaultAction();
    public void ReduceClearReadAction();
    public void ReduceClearDeletedAction();
    public void ReduceClearArchivedAction();
}
