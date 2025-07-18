using Microsoft.Extensions.Logging;
using Walk.Common.RazorLib.Installations.Models;

namespace Walk.Common.RazorLib.BackgroundTasks.Models;

public sealed class IndefiniteBackgroundTaskWorker
{
    private readonly ILogger _logger;

    public IndefiniteBackgroundTaskWorker(
        BackgroundTaskQueue queue,
        CommonService commonService,
        ILoggerFactory loggerFactory,
        WalkHostingKind walkHostingKind)
    {
        Queue = queue;
        CommonService = commonService;
        _logger = loggerFactory.CreateLogger<IndefiniteBackgroundTaskWorker>();
        WalkHostingKind = walkHostingKind;
    }
    
    public BackgroundTaskQueue Queue { get; }
    public CommonService CommonService { get; }
    public Task? StartAsyncTask { get; internal set; }
    public WalkHostingKind WalkHostingKind { get; }

    public event Action? ExecutingBackgroundTaskChanged;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(IndefiniteBackgroundTaskWorker)} is starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
        	await Queue.__DequeueSemaphoreSlim.WaitAsync().ConfigureAwait(false);
        	var backgroundTask = Queue.__DequeueOrDefault();

            try
            {
                await backgroundTask.HandleEvent().ConfigureAwait(false);
                await Task.Yield();
            }
            catch (Exception ex)
            {
                var message = ex is OperationCanceledException
                    ? "Task was cancelled {0}." // {0} => WorkItemName
                    : "Error occurred executing {0}."; // {0} => WorkItemName

                _logger.LogError(ex, message, "(backgroundTask.Name was here)");
				Console.WriteLine($"ERROR on (backgroundTask.Name was here): {ex.ToString()}");
            }
            finally
            {
            	if (backgroundTask.__TaskCompletionSourceWasCreated)
            		CommonService.CompleteTaskCompletionSource(backgroundTask.BackgroundTaskKey);
            }
    	}
	}   
}