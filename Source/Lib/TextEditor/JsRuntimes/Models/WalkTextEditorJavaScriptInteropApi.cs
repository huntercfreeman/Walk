using Microsoft.JSInterop;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Displays;

namespace Walk.TextEditor.RazorLib.JsRuntimes.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: the 'IJSRuntime' datatype is far more common in code,
///     than some specific type (example: DialogDisplay.razor).
///     So, adding 'Walk' in the class name for redundancy seems meaningful here.
/// </remarks>
public class WalkTextEditorJavaScriptInteropApi
{
    private readonly IJSRuntime _jsRuntime;

    public WalkTextEditorJavaScriptInteropApi(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask ScrollElementIntoView(string elementId)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkTextEditor.scrollElementIntoView",
            elementId);
    }
    
    public ValueTask SetPreventDefaultsAndStopPropagations(
        DotNetObjectReference<TextEditorViewModelSlimDisplay> dotNetHelper,
        string contentElementId,
        string rowSectionElementId,
        string HORIZONTAL_ScrollbarElementId,
        string VERTICAL_ScrollbarElementId,
        string CONNECTOR_ScrollbarElementId)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkTextEditor.setPreventDefaultsAndStopPropagations",
            dotNetHelper,
            contentElementId,
            rowSectionElementId,
            HORIZONTAL_ScrollbarElementId,
            VERTICAL_ScrollbarElementId,
            CONNECTOR_ScrollbarElementId);
    }

    public ValueTask<CharAndLineMeasurements> GetCharAndLineMeasurementsInPixelsById(
        string measureCharacterWidthAndLineHeightElementId)
    {
        return _jsRuntime.InvokeAsync<CharAndLineMeasurements>(
            "walkTextEditor.getCharAndLineMeasurementsInPixelsById",
            measureCharacterWidthAndLineHeightElementId);
    }

    /// <summary>
    /// TODO: This javascript function is only used from other javascript functions.
    /// </summary>
    public ValueTask<string> EscapeHtml(string input)
    {
        return _jsRuntime.InvokeAsync<string>(
            "walkTextEditor.escapeHtml",
            input);
    }

    public ValueTask<RelativeCoordinates> GetRelativePosition(
        string elementId,
        double clientX,
        double clientY)
    {
        return _jsRuntime.InvokeAsync<RelativeCoordinates>(
            "walkTextEditor.getRelativePosition",
            elementId,
            clientX,
            clientY);
    }

    public ValueTask SetScrollPositionBoth(
        string bodyElementId,
        double scrollLeftInPixels,
        double scrollTopInPixels)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkTextEditor.setScrollPositionBoth",
            bodyElementId,
            scrollLeftInPixels,
            scrollTopInPixels);
    }
    
    public ValueTask SetScrollPositionLeft(
        string bodyElementId,
        double scrollLeftInPixels)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkTextEditor.setScrollPositionLeft",
            bodyElementId,
            scrollLeftInPixels);
    }
    
    public ValueTask SetScrollPositionTop(
        string bodyElementId,
        double scrollTopInPixels)
    {
        return _jsRuntime.InvokeVoidAsync(
            "walkTextEditor.setScrollPositionTop",
            bodyElementId,
            scrollTopInPixels);
    }

    public ValueTask<TextEditorDimensions> GetTextEditorMeasurementsInPixelsById(
        string elementId)
    {
        return _jsRuntime.InvokeAsync<TextEditorDimensions>(
            "walkTextEditor.getTextEditorMeasurementsInPixelsById",
            elementId);
    }

    public ValueTask<ElementPositionInPixels> GetBoundingClientRect(string primaryCursorContentId)
    {
        return _jsRuntime.InvokeAsync<ElementPositionInPixels>(
            "walkTextEditor.getBoundingClientRect",
            primaryCursorContentId);
    }
}
