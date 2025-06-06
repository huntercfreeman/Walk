using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Installations.Displays;

/// <summary>
/// NOT thread safe.
///
/// Ensure only 1 instance is rendered
/// to avoid race condition with:
/// 'BackgroundTaskService.ContinuousTaskWorker.StartAsyncTask = Task.Run...'
/// </summary>
public partial class WalkCommonInitializer : ComponentBase, IDisposable
{
    [Inject]
    private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    [Inject]
    private WalkHostingInformation WalkHostingInformation { get; set; } = null!;
    
    public static Key<ContextSwitchGroup> ContextSwitchGroupKey { get; } = Key<ContextSwitchGroup>.NewKey();
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
	protected override void OnInitialized()
	{
        CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
        {
        	WorkKind = CommonWorkKind.WalkCommonInitializerWork
    	});
        base.OnInitialized();
	}

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			var token = _workerCancellationTokenSource.Token;

			if (BackgroundTaskService.ContinuousWorker.StartAsyncTask is null)
			{
				BackgroundTaskService.ContinuousWorker.StartAsyncTask = Task.Run(
					() => BackgroundTaskService.ContinuousWorker.ExecuteAsync(token),
					token);
			}

			if (WalkHostingInformation.WalkPurposeKind == WalkPurposeKind.Ide)
			{
				if (BackgroundTaskService.IndefiniteWorker.StartAsyncTask is null)
				{
					BackgroundTaskService.IndefiniteWorker.StartAsyncTask = Task.Run(
						() => BackgroundTaskService.IndefiniteWorker.ExecuteAsync(token),
						token);
				}
			}

			BrowserResizeInterop.SubscribeWindowSizeChanged(CommonBackgroundTaskApi.JsRuntimeCommonApi);
		}

		base.OnAfterRender(firstRender);
	}
    
    /// <summary>
    /// Presumptions:
	/// - Dispose is invoked from UI thread
	/// - Dispose being ran stops the Blazor lifecycle from being ran in the future
    ///     - i.e.: OnInitialized(), Dispose(), OnAfterRender() <---- bad. Is it possible?
    /// </summary>
    public void Dispose()
    {
    	BrowserResizeInterop.DisposeWindowSizeChanged(CommonBackgroundTaskApi.JsRuntimeCommonApi);
    	
    	_workerCancellationTokenSource.Cancel();
    	_workerCancellationTokenSource.Dispose();
    	
    	BackgroundTaskService.ContinuousWorker.StartAsyncTask = null;
    	BackgroundTaskService.IndefiniteWorker.StartAsyncTask = null;
    }
}