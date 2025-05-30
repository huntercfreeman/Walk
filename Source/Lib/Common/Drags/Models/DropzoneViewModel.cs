using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Drags.Models;

public record DropzoneViewModel(
		Key<IDropzone> Key,
		MeasuredHtmlElementDimensions MeasuredHtmlElementDimensions,
		ElementDimensions DropzoneElementDimensions,
        Key<IDropzone> DropzoneKey,
        ElementDimensions ElementDimensions,
        string CssClass,
        string CssStyle)
	: IDropzone;
