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
}
