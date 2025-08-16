using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;

namespace Walk.CompilerServices.Xml.Html.SyntaxObjects;

public struct AttributeEntry
{
    public AttributeEntry(TextEditorTextSpan nameTextSpan, TextEditorTextSpan valueTextSpan)
    {
        NameTextSpan = nameTextSpan;
        ValueTextSpan = valueTextSpan;
    }

    public TextEditorTextSpan NameTextSpan { get; }
    public TextEditorTextSpan ValueTextSpan { get; }
}
