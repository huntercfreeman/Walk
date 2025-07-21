using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Json.SyntaxEnums;

namespace Walk.CompilerServices.Json;

public interface IJsonSyntax
{
    public JsonSyntaxKind JsonSyntaxKind { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
    public IReadOnlyList<IJsonSyntax> ChildJsonSyntaxes { get; }
}
