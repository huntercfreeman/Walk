using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Reactives.Displays;

public partial class ProgressBarDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

	[CascadingParameter]
	public INotification? Notification { get; set; } = null!;

	[Parameter, EditorRequired]
	public ProgressBarModel ProgressBarModel { get; set; } = null!;

	private bool _hasSeenProgressModelIsDisposed = false;

	protected override void OnInitialized()
	{
		if (!ProgressBarModel.IsDisposed)
			ProgressBarModel.ProgressChanged += OnProgressChanged;
	}

	protected override Task OnAfterRenderAsync(bool firstRender)
	{
		if (Notification is not null && !_hasSeenProgressModelIsDisposed && ProgressBarModel.IsDisposed)
		{
			_hasSeenProgressModelIsDisposed = true;

			_ = Task.Run((Func<Task?>)(async () =>
			{
				await Task.Delay(4_000);
		        
				if (Notification.DeleteNotificationAfterOverlayIsDismissed)
					CommonService.Notification_ReduceMakeDeletedAction(Notification.DynamicViewModelKey);
		        else
					CommonService.Notification_ReduceMakeReadAction(Notification.DynamicViewModelKey);
			}));
		}
        
        return Task.CompletedTask;
	}

	public async void OnProgressChanged(bool isDisposing)
	{
		if (isDisposing)
			ProgressBarModel.ProgressChanged -= OnProgressChanged;

		await InvokeAsync(StateHasChanged);
	}

	public void Dispose()
	{
		ProgressBarModel.ProgressChanged -= OnProgressChanged;
	}
}
