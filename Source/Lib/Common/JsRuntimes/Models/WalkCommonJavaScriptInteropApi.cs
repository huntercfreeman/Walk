using Microsoft.JSInterop;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.JsRuntimes.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: the 'IJSRuntime' datatype is far more common in code,
///     than some specific type (example: DialogDisplay.razor).
///     So, adding 'Walk' in the class name for redundancy seems meaningful here.
/// </remarks>
public class WalkCommonJavaScriptInteropApi
{
    public IJSRuntime JsRuntime { get; }

    public WalkCommonJavaScriptInteropApi(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public ValueTask SubscribeWindowSizeChanged(DotNetObjectReference<BrowserResizeInterop> browserResizeInteropDotNetObjectReference)
    {
        return JsRuntime.InvokeVoidAsync(
            "walkCommon.subscribeWindowSizeChanged",
            browserResizeInteropDotNetObjectReference);
    }

    public ValueTask DisposeWindowSizeChanged()
    {
        return JsRuntime.InvokeVoidAsync(
            "walkCommon.disposeWindowSizeChanged");
    }
    
    public ValueTask FocusHtmlElementById(string elementId, bool preventScroll = false)
    {
        return JsRuntime.InvokeVoidAsync(
            "walkCommon.focusHtmlElementById",
            elementId,
            preventScroll);
    }

    public ValueTask<bool> TryFocusHtmlElementById(string elementId)
    {
        return JsRuntime.InvokeAsync<bool>(
            "walkCommon.tryFocusHtmlElementById",
            elementId);
    }
    
    public ValueTask<MeasuredHtmlElementDimensions> MeasureElementById(string elementId)
    {
        return JsRuntime.InvokeAsync<MeasuredHtmlElementDimensions>(
            "walkCommon.measureElementById",
            elementId);
    }

    public ValueTask LocalStorageSetItem(string key, object? value)
    {
        return JsRuntime.InvokeVoidAsync(
            "walkCommon.localStorageSetItem",
            key,
            value);
    }

    public ValueTask<string?> LocalStorageGetItem(string key)
    {
        return JsRuntime.InvokeAsync<string?>(
            "walkCommon.localStorageGetItem",
            key);
    }

    public ValueTask<string> ReadClipboard()
    {
        return JsRuntime.InvokeAsync<string>(
            "walkCommon.readClipboard");
    }

    public ValueTask SetClipboard(string value)
    {
        return JsRuntime.InvokeVoidAsync(
            "walkCommon.setClipboard",
            value);
    }

    public ValueTask<ContextMenuFixedPosition> GetTreeViewContextMenuFixedPosition(
        string nodeElementId)
    {
        return JsRuntime.InvokeAsync<ContextMenuFixedPosition>(
            "walkCommon.getTreeViewContextMenuFixedPosition",
            nodeElementId);
    }
}
