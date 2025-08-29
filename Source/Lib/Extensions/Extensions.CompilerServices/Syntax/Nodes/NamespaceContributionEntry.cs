using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// The textspan points to the text that identifies the namespace group that was contributed to.
/// </summary>
public struct NamespaceContributionEntry
{
    public NamespaceContributionEntry(TextEditorTextSpan textSpan)
    {
        TextSpan = textSpan;
    }

    public TextEditorTextSpan TextSpan { get; }
}
