using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public record PanelGroupDropzone(
        MeasuredHtmlElementDimensions MeasuredHtmlElementDimensions,
        Key<PanelGroup> PanelGroupKey,
        ElementDimensions ElementDimensions,
        Key<IDropzone> DropzoneKey,
        string? CssClass,
        string? CssStyle)
    : IDropzone;
