using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dynamics.Models;

public interface IDropzone
{
    public Key<IDropzone> DropzoneKey { get; }
    public MeasuredHtmlElementDimensions MeasuredHtmlElementDimensions { get; }
    public ElementDimensions ElementDimensions { get; }
    public string? CssClass { get; }
    public string? CssStyle { get; }
}
