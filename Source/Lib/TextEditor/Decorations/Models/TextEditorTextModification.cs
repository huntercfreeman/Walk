using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.Decorations.Models;

public record struct TextEditorTextModification(bool WasInsertion, TextEditorTextSpan TextEditorTextSpan);
