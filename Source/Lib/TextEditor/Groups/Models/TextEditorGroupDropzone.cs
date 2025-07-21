using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.TextEditor.RazorLib.Groups.Models;

public class TextEditorGroupDropzone : IDropzone
{
    public TextEditorGroupDropzone(
        MeasuredHtmlElementDimensions measuredHtmlElementDimensions,
        Key<TextEditorGroup> textEditorGroupKey,
        ElementDimensions elementDimensions)
    {
        MeasuredHtmlElementDimensions = measuredHtmlElementDimensions;
        TextEditorGroupKey = textEditorGroupKey;
        ElementDimensions = elementDimensions;
    }

    public MeasuredHtmlElementDimensions MeasuredHtmlElementDimensions { get; }
    public Key<TextEditorGroup> TextEditorGroupKey { get; }
    public Key<IDropzone> DropzoneKey { get; }
    public ElementDimensions ElementDimensions { get; }
    public string CssClass { get; init; }
    public string CssStyle { get; }
}

