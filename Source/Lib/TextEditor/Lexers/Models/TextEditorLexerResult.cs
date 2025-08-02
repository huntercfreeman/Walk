namespace Walk.TextEditor.RazorLib.Lexers.Models;

public interface TextEditorLexerResult
{
    public IReadOnlyList<TextEditorTextSpan> TextSpanList { get; }
    public string ResourceUri { get; }
}
