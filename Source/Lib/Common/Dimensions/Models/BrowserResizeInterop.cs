using Microsoft.JSInterop;
using Walk.Common.RazorLib.JsRuntimes.Models;

namespace Walk.Common.RazorLib.Dimensions.Models;

/// <summary>
/// TODO: The way this class will interact with Blazor ServerSide is a current concern (as this class is being written). So, make sure to test out ServerSide, refresh the page, etc...
///
/// Subscribes to JavaScript 'resize' event.
///
/// https://github.com/chrissainty/BlazorBrowserResize/blob/master/BrowserResize/BrowserResize/BrowserResizeService.cs
/// </summary>
public class BrowserResizeInterop
{
    private readonly object _dotNetObjectReferenceLock = new();
    private readonly CommonService _commonService;

    public BrowserResizeInterop(CommonService commonService)
    {
        _commonService = commonService;
    }
    
    private DotNetObjectReference<BrowserResizeInterop>? _browserResizeInteropDotNetObjectReference;
    
    /// <summary>
    /// A static method isn't used here because it needs to be scoped to the individual user session
    /// in order to access the <see cref="IDispatcher"/>.
    /// </summary>
    [JSInvokable]
    public void OnBrowserResize(AppDimensionState appDimensionState)
    {
        // AppDimensionState(int Width, int Height, int Left, int Top)
        _commonService.AppDimension_NotifyUserAgentResize(appDimensionState);
    }
    
    public async Task SubscribeWindowSizeChanged(WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi)
    {
        if (_browserResizeInteropDotNetObjectReference is null)
        {
            lock (_dotNetObjectReferenceLock)
            {
                if (_browserResizeInteropDotNetObjectReference is null)
                    _browserResizeInteropDotNetObjectReference = DotNetObjectReference.Create(this);
            }
        }
        
        _commonService.SetAppDimensions_Silent_NoEventRaised(await walkCommonJavaScriptInteropApi.SubscribeWindowSizeChanged(
            _browserResizeInteropDotNetObjectReference,
            CommonFacts.RootHtmlElementId));
    }
    
    public void DisposeWindowSizeChanged(WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi)
    {
        walkCommonJavaScriptInteropApi.DisposeWindowSizeChanged();
    }
}
