using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

public sealed record TooltipViewModel(
    Type RendererType,
    Dictionary<string, object?>? ParameterMap,
    RelativeCoordinates RelativeCoordinates,
    string? CssClassString,
    Func<Task> OnMouseOver);