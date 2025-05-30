using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;

namespace Walk.TextEditor.RazorLib.Keymaps.Models;

public static class TextEditorKeymapFacts
{
    public static readonly ITextEditorKeymap DefaultKeymap = new TextEditorKeymapDefault();

    public static List<ITextEditorKeymap> AllKeymapsList { get; } =
        new()
        {
            DefaultKeymap,
		};
}