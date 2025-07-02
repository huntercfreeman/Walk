using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class NotificationsViewDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;
    
    private readonly Action _defaultClearAction = new Action(() => { });

    private NotificationsViewKind _chosenNotificationsViewKind = NotificationsViewKind.Notifications;

	protected override void OnInitialized()
    {
    	CommonUtilityService.CommonUiStateChanged += OnCommonUiStateChanged;
    }

    private string GetIsActiveCssClass(
        NotificationsViewKind chosenNotificationsViewKind,
        NotificationsViewKind iterationNotificationsViewKind)
    {
        return chosenNotificationsViewKind == iterationNotificationsViewKind
            ? "di_active"
            : string.Empty;
    }

    private void Clear()
    {
        CommonUtilityService.Notification_ReduceClearDefaultAction();
    }

    private void ClearRead()
    {
        CommonUtilityService.Notification_ReduceClearReadAction();
    }

    private void ClearDeleted()
    {
        CommonUtilityService.Notification_ReduceClearDeletedAction();
    }

    private void ClearArchived()
    {
        CommonUtilityService.Notification_ReduceClearArchivedAction();
    }
    
    public async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.NotificationStateChanged)
    	    await InvokeAsync(StateHasChanged);
    }
	
	public void Dispose()
	{
		CommonUtilityService.CommonUiStateChanged -= OnCommonUiStateChanged;
	}
}
