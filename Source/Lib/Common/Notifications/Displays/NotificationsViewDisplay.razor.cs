using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class NotificationsViewDisplay : ComponentBase, IDisposable
{
    [Inject]
    private ICommonUiService CommonUiService { get; set; } = null!;
    
    private readonly Action _defaultClearAction = new Action(() => { });

    private NotificationsViewKind _chosenNotificationsViewKind = NotificationsViewKind.Notifications;

	protected override void OnInitialized()
    {
    	CommonUiService.CommonUiStateChanged += OnCommonUiStateChanged;
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
        CommonUiService.Notification_ReduceClearDefaultAction();
    }

    private void ClearRead()
    {
        CommonUiService.Notification_ReduceClearReadAction();
    }

    private void ClearDeleted()
    {
        CommonUiService.Notification_ReduceClearDeletedAction();
    }

    private void ClearArchived()
    {
        CommonUiService.Notification_ReduceClearArchivedAction();
    }
    
    public async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.NotificationStateChanged)
    	    await InvokeAsync(StateHasChanged);
    }
	
	public void Dispose()
	{
		CommonUiService.CommonUiStateChanged -= OnCommonUiStateChanged;
	}
}
