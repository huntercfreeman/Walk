using Microsoft.JSInterop;

namespace Walk.TextEditor.RazorLib.JsRuntimes.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: the 'IJSRuntime' datatype is far more common in code,
/// 	than some specific type (example: DialogDisplay.razor).
///     So, adding 'Walk' in the class name for redundancy seems meaningful here.
/// </remarks>
public static class WalkTextEditorJsRuntimeExtensionMethods
{
    public static WalkTextEditorJavaScriptInteropApi GetWalkTextEditorApi(this IJSRuntime jsRuntime)
    {
        return new WalkTextEditorJavaScriptInteropApi(jsRuntime);
    }
}