using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib.Options.Models;

public partial class CommonService : IBackgroundTaskGroup
{
    private readonly object _stateModificationLock = new();
	
	public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi { get; }
	public ICommonComponentRenderers CommonComponentRenderers { get; }
	
	public WalkHostingInformation WalkHostingInformation { get; }

    public CommonService(
        WalkHostingInformation hostingInformation,
        ICommonComponentRenderers commonComponentRenderers,
        WalkCommonConfig commonConfig,
        IJSRuntime jsRuntime)
    {
        WalkHostingInformation = hostingInformation;
    
        CommonComponentRenderers = commonComponentRenderers;
        
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
    
	public event Action<CommonUiEventKind>? CommonUiStateChanged;
}
