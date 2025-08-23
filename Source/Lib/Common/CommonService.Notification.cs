using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private NotificationState _notificationState = new();
    
    public NotificationState GetNotificationState() => _notificationState;

    public void Notification_ReduceRegisterAction(INotification notification)
    {
        lock (_stateModificationLock)
        {
            var outDefaultList = new List<INotification>(_notificationState.DefaultList);
            outDefaultList.Add(notification);
            _notificationState = _notificationState with { DefaultList = outDefaultList };
        }
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceDisposeAction(Key<IDynamicViewModel> key)
    {
        lock (_stateModificationLock)
        {
            var indexNotification = _notificationState.DefaultList.FindIndex(
                x => x.DynamicViewModelKey == key);
    
            if (indexNotification != -1)
            {
                var inNotification = _notificationState.DefaultList[indexNotification];
                var outDefaultList = new List<INotification>(_notificationState.DefaultList);
                outDefaultList.RemoveAt(indexNotification);
                _notificationState = _notificationState with { DefaultList = outDefaultList };
            }
        }
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearDefaultAction()
    {
        lock (_stateModificationLock)
        {
            _notificationState = _notificationState with
            {
                DefaultList = new List<INotification>()
            };
        }
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
}
