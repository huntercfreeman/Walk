using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class NotificationInitializer : ComponentBase, IDisposable
{
    [Inject]
    private INotificationService NotificationService { get; set; } = null!;

    private ContextBoundary? _notificationContextBoundary;
    
    protected override void OnInitialized()
    {
    	NotificationService.NotificationStateChanged += OnNotificationStateChanged;
    	base.OnInitialized();
    }
    
    private Task HandleOnFocusIn(INotification notification)
    {
    	var localNotificationContextBoundary = _notificationContextBoundary;
    	
    	if (localNotificationContextBoundary is not null)
	    	localNotificationContextBoundary.HandleOnFocusIn();
	    	
	    return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(INotification notification)
    {
    	return Task.CompletedTask;
    }
    
    public async void OnNotificationStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
	
	public void Dispose()
	{
		NotificationService.NotificationStateChanged -= OnNotificationStateChanged;
	
		var notificationState = NotificationService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            NotificationService.ReduceDisposeAction(notification.DynamicViewModelKey);
        }
	}
}
