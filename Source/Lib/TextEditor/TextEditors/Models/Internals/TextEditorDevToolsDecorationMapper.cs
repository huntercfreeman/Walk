using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

public sealed class TextEditorDevToolsDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (TextEditorDevToolsDecorationKind)decorationByte;

        return decoration switch
        {
            TextEditorDevToolsDecorationKind.None => string.Empty,
            TextEditorDevToolsDecorationKind.Scope => "background-color: var(--di_te_brace_matching-background-color);",
            _ => string.Empty,
        };
    }
}
