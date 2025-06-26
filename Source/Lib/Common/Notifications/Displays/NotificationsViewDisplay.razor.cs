using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class NotificationsViewDisplay : ComponentBase, IDisposable
{
    [Inject]
    private INotificationService NotificationService { get; set; } = null!;
    
    private readonly Action _defaultClearAction = new Action(() => { });

    private NotificationsViewKind _chosenNotificationsViewKind = NotificationsViewKind.Notifications;

	protected override void OnInitialized()
    {
    	NotificationService.NotificationStateChanged += OnNotificationStateChanged;
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
        NotificationService.ReduceClearDefaultAction();
    }

    private void ClearRead()
    {
        NotificationService.ReduceClearReadAction();
    }

    private void ClearDeleted()
    {
        NotificationService.ReduceClearDeletedAction();
    }

    private void ClearArchived()
    {
        NotificationService.ReduceClearArchivedAction();
    }
    
    public async void OnNotificationStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
	
	public void Dispose()
	{
		NotificationService.NotificationStateChanged -= OnNotificationStateChanged;
	}
}
