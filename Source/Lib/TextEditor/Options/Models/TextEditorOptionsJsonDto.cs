using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.RenderStates.Models;

namespace Walk.TextEditor.RazorLib.Options.Models;

/// <summary>
/// This type needs to exist so the <see cref="TextEditorOptions"/> properties can be nullable, as in they were not
/// already in local storage. Whereas throughout the app they should never be null.
/// </summary>
public record TextEditorOptionsJsonDto(
    CommonOptionsJsonDto? CommonOptionsJsonDto,
    bool ShowWhitespace,
    bool ShowNewlines,
    bool TabKeyBehavior,
    int TabWidth,
    double CursorWidthInPixels)
{
    public TextEditorOptionsJsonDto()
        : this(
            CommonOptionsJsonDto: null,
            ShowWhitespace: false,
            ShowNewlines: false,
            TabKeyBehavior: true,
            TabWidth: 4,
            CursorWidthInPixels: 2.5)
    {
    }
    
    public TextEditorOptionsJsonDto(TextEditorOptions options)
        : this(
              new CommonOptionsJsonDto(options.CommonOptions),
              options.ShowWhitespace,
              options.ShowNewlines,
              options.TabKeyBehavior,
              options.TabWidth,
              options.CursorWidthInPixels)
    {
        
    }

    public Key<RenderState> RenderStateKey { get; init; } = Key<RenderState>.NewKey();
}
