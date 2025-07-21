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
    public void OnBrowserResize()
    {
        _commonService.AppDimension_NotifyUserAgentResize();
    }
    
    /// <summary>
    /// The idea here is that one can subscribe/dispose as much as they'd like if they want to stop listening to the
    /// resize event.
    ///
    /// But I'm probably never going to do use this extra logic I wrote with the re-using of the DotNetObjectReference
    /// when I re-subscribe and such (because I'll only dispose when the app is closed). #YAGNI
    /// </summary>
    public void SubscribeWindowSizeChanged(WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi)
    {
        if (_browserResizeInteropDotNetObjectReference is null)
        {
            lock (_dotNetObjectReferenceLock)
            {
                if (_browserResizeInteropDotNetObjectReference is null)
                    _browserResizeInteropDotNetObjectReference = DotNetObjectReference.Create(this);
            }
        }
        
        walkCommonJavaScriptInteropApi.SubscribeWindowSizeChanged(
            _browserResizeInteropDotNetObjectReference);
    }
    
    public void DisposeWindowSizeChanged(WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi)
    {
        walkCommonJavaScriptInteropApi.DisposeWindowSizeChanged();
    }
}
