using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Common.RazorLib.Tooltips.Models;

public sealed record TooltipModel(
    Type RendererType,
    Dictionary<string, object?>? ParameterMap,
    double X,
    double Y,
    string? CssClassString,
    Func<Task> OnMouseOver);
