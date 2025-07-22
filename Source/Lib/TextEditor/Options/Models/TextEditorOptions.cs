using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

namespace Walk.TextEditor.RazorLib.Options.Models;

/// <param name="TabKeyBehavior">Tab key inserts:(true:tab, false:spaces):</param>
public record TextEditorOptions(
    CommonOptions CommonOptions,
    bool ShowWhitespace,
    bool ShowNewlines,
    bool TabKeyBehavior,
    int TabWidth,
    double CursorWidthInPixels,
    CharAndLineMeasurements CharAndLineMeasurements)
{
    public Key<RenderState> RenderStateKey { get; init; } = Key<RenderState>.NewKey();
    
    /// <summary>
    /// Hacky setter on this property in particular because it can be overridden.
    /// And when overridden it causes an object allocation, and this happens frequently enough to be cause for concern.
    /// </summary>
    public ITextEditorKeymap Keymap { get; set; }
}
