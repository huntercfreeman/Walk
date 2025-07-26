using Microsoft.JSInterop;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Keys.Models;
using System.Collections.Concurrent;

namespace Walk.Common.RazorLib;

public partial class CommonService : IBackgroundTaskGroup
{
    private readonly object _stateModificationLock = new();
    
    public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi { get; }
    
    public WalkHostingInformation WalkHostingInformation { get; }

    public CommonService(
        WalkHostingInformation hostingInformation,
        WalkCommonConfig commonConfig,
        IJSRuntime jsRuntime)
    {
        WalkHostingInformation = hostingInformation;
    
        CommonConfig = commonConfig;
    
        switch (hostingInformation.WalkHostingKind)
        {
            case WalkHostingKind.Photino:
                EnvironmentProvider = new LocalEnvironmentProvider();
                FileSystemProvider = new LocalFileSystemProvider(this);
                break;
            default:
                EnvironmentProvider = new InMemoryEnvironmentProvider();
                FileSystemProvider = new InMemoryFileSystemProvider(this);
                break;
        }
        
        JsRuntimeCommonApi = jsRuntime.GetWalkCommonApi();
    
        _debounceExtraEvent = new(
            TimeSpan.FromMilliseconds(250),
            CancellationToken.None,
            (_, _) =>
            {
                AppDimension_NotifyIntraAppResize(useExtraEvent: false);
                return Task.CompletedTask;
            });
    }
    
    public HashSet<string> AltKeybindFocusPointHashSet { get; } = new();
    
    public IEnvironmentProvider EnvironmentProvider { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    
    public Task? Continuous_StartAsyncTask { get; internal set; }
    public event Action? Continuous_ExecutingBackgroundTaskChanged;

    /// <summary>
    /// Generally speaking: Presume that the ContinuousTaskWorker is "always ready" to run the next task that gets enqueued.
    /// </summary>
    public async Task Continuous_ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Continuous__DequeueSemaphoreSlim.WaitAsync().ConfigureAwait(false);
                    await Continuous__DequeueOrDefault().HandleEvent().ConfigureAwait(false);
                    await Task.Yield();
                }
            }
            catch (Exception ex)
            {
                var message = ex is OperationCanceledException
                    ? "Task was cancelled {0}." // {0} => WorkItemName
                    : "Error occurred executing {0}."; // {0} => WorkItemName

                Console.WriteLine($"ERROR on (backgroundTask.Name was here): {ex.ToString()}");
            }
        }
    }
    
    public Task? Indefinite_StartAsyncTask { get; internal set; }
    public event Action? Indefinite_ExecutingBackgroundTaskChanged;

    /// <summary>
    /// Generally speaking: Presume that the IndefiniteTaskWorker is NOT ready to run the next task that gets enqueued.
    /// </summary>
    public async Task Indefinite_ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Indefinite__DequeueSemaphoreSlim.WaitAsync().ConfigureAwait(false);
            var backgroundTask = Indefinite__DequeueOrDefault();

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

                Console.WriteLine($"ERROR on (backgroundTask.Name was here): {ex.ToString()}");
            }
            finally
            {
                if (backgroundTask.__TaskCompletionSourceWasCreated)
                    CompleteTaskCompletionSource(backgroundTask.BackgroundTaskKey);
            }
        }
    }
    
    private readonly ConcurrentQueue<IBackgroundTaskGroup> Continuous_queue = new();

    /// <summary>
    /// Returns the amount of <see cref="IBackgroundTask"/>(s) in the queue.
    /// </summary>
    public int Continuous_Count => Continuous_queue.Count;

    public SemaphoreSlim Continuous__DequeueSemaphoreSlim { get; } = new(0);

    public List<IBackgroundTaskGroup> Continuous_GetBackgroundTaskList() => Continuous_queue.ToList();

    public void Continuous_Enqueue(IBackgroundTaskGroup downstreamEvent)
    {
        Continuous_queue.Enqueue(downstreamEvent);
        Continuous__DequeueSemaphoreSlim.Release();
    }
    
    public IBackgroundTaskGroup Continuous__DequeueOrDefault()
    {
        Continuous_queue.TryDequeue(out var backgroundTask);
        return backgroundTask;
    }
    
    private readonly ConcurrentQueue<IBackgroundTaskGroup> Indefinite_queue = new();

    /// <summary>
    /// Returns the amount of <see cref="IBackgroundTask"/>(s) in the queue.
    /// </summary>
    public int Indefinite_Count => Indefinite_queue.Count;

    public SemaphoreSlim Indefinite__DequeueSemaphoreSlim { get; } = new(0);

    public List<IBackgroundTaskGroup> Indefinite_GetBackgroundTaskList() => Indefinite_queue.ToList();

    public void Indefinite_Enqueue(IBackgroundTaskGroup downstreamEvent)
    {
        Indefinite_queue.Enqueue(downstreamEvent);
        Indefinite__DequeueSemaphoreSlim.Release();
    }
    
    public IBackgroundTaskGroup Indefinite__DequeueOrDefault()
    {
        Indefinite_queue.TryDequeue(out var backgroundTask);
        return backgroundTask;
    }
}
