using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;

namespace Walk.TextEditor.RazorLib.Lexers.Models;

public interface TextEditorLexerResult
{
    public IReadOnlyList<TextEditorTextSpan> TextSpanList { get; }
    public string ResourceUri { get; }
    public Key<RenderState> ModelRenderStateKey { get; }
}