using Microsoft.JSInterop;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.JsRuntimes.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: the 'IJSRuntime' datatype is far more common in code,
/// 	than some specific type (example: DialogDisplay.razor).
///     So, adding 'Walk' in the class name for redundancy seems meaningful here.
/// </remarks>
public class WalkCommonJavaScriptInteropApi
{
    private readonly IJSRuntime _jsRuntime;

    public WalkCommonJavaScriptInteropApi(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask SubscribeWindowSizeChanged(DotNetObjectReference<BrowserResizeInterop> browserResizeInteropDotNetObjectReference)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkCommon.subscribeWindowSizeChanged",
            browserResizeInteropDotNetObjectReference);
    }

    public ValueTask DisposeWindowSizeChanged()
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkCommon.disposeWindowSizeChanged");
    }
    
    public ValueTask FocusHtmlElementById(string elementId, bool preventScroll = false)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkCommon.focusHtmlElementById",
            elementId,
            preventScroll);
    }

    public ValueTask<bool> TryFocusHtmlElementById(string elementId)
    {
        return _jsRuntime.InvokeAsync<bool>(
            "walkCommon.tryFocusHtmlElementById",
            elementId);
    }
    
    public ValueTask<MeasuredHtmlElementDimensions> MeasureElementById(string elementId)
    {
        return _jsRuntime.InvokeAsync<MeasuredHtmlElementDimensions>(
            "walkCommon.measureElementById",
            elementId);
    }

    public ValueTask LocalStorageSetItem(string key, object? value)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkCommon.localStorageSetItem",
            key,
            value);
    }

    public ValueTask<string?> LocalStorageGetItem(string key)
    {
        return _jsRuntime.InvokeAsync<string?>(
            "walkCommon.localStorageGetItem",
            key);
    }

    public ValueTask<string> ReadClipboard()
    {
        return _jsRuntime.InvokeAsync<string>(
            "walkCommon.readClipboard");
    }

    public ValueTask SetClipboard(string value)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkCommon.setClipboard",
            value);
    }

    public ValueTask<ContextMenuFixedPosition> GetTreeViewContextMenuFixedPosition(
        string nodeElementId)
    {
        return _jsRuntime.InvokeAsync<ContextMenuFixedPosition>(
            "walkCommon.getTreeViewContextMenuFixedPosition",
            nodeElementId);
    }
}
