using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Notifications.Models;

public class NotificationBadge : IBadgeModel
{
    public static readonly Key<IBadgeModel> NotificationBadgeKey = Key<IBadgeModel>.NewKey();
    public static readonly Key<IDynamicViewModel> DialogRecordKey = Key<IDynamicViewModel>.NewKey();

    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    public NotificationBadge(
        INotificationService notificationService,
        IDialogService dialogService)
    {
        _notificationService = notificationService;
        _dialogService = dialogService;
    }
    
    private Func<Task>? _updateUiFunc;

    public Key<IBadgeModel> Key => NotificationBadgeKey;
	public BadgeKind BadgeKind => BadgeKind.Notification;
	public int Count => _notificationService.GetNotificationState().DefaultList.Count;
	
	public void OnClick()
	{
	    _dialogService.ReduceRegisterAction(new DialogViewModel(
            DialogRecordKey,
            "Notifications",
            typeof(Walk.Common.RazorLib.Notifications.Displays.NotificationsViewDisplay),
            null,
            null,
    		true,
    		setFocusOnCloseElementId: null));
	}
	
	public void AddSubscription(Func<Task> updateUiFunc)
	{
	    _updateUiFunc = updateUiFunc;
	    _notificationService.NotificationStateChanged += DoSubscription;
	}
	
	public async void DoSubscription()
	{
	    var localUpdateUiFunc = _updateUiFunc;
	    if (_updateUiFunc is not null)
	        await _updateUiFunc.Invoke();
	}
	
	public void DisposeSubscription()
	{
	    _notificationService.NotificationStateChanged -= DoSubscription;
	    _updateUiFunc = null;
	}
}
