using Microsoft.JSInterop;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;

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
    
    public IEnvironmentProvider EnvironmentProvider { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    
    public Task? Continuous_StartAsyncTask { get; internal set; }
    public event Action? Continuous_ExecutingBackgroundTaskChanged;

    public async Task Continuous_ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ContinuousQueue.__DequeueSemaphoreSlim.WaitAsync().ConfigureAwait(false);
                    await ContinuousQueue.__DequeueOrDefault().HandleEvent().ConfigureAwait(false);
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
}
