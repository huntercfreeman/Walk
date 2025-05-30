using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public static class TerminalPresentationFacts
{
    public const string CssClassString = "di_te_terminal-presentation";

    public static readonly Key<TextEditorPresentationModel> PresentationKey = Key<TextEditorPresentationModel>.NewKey();

    public static readonly TextEditorPresentationModel EmptyPresentationModel = new(
        PresentationKey,
        0,
        CssClassString,
        new TerminalDecorationMapper());
}
