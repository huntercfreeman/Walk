namespace Walk.TextEditor.RazorLib.Decorations.Models;

public class GenericDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (GenericDecorationKind)decorationByte;

        return decoration switch
        {
            GenericDecorationKind.None => string.Empty,
            GenericDecorationKind.Keyword => "di0",
            GenericDecorationKind.KeywordControl => "di4",
            GenericDecorationKind.EscapeCharacterPrimary => "di7",
            GenericDecorationKind.EscapeCharacterSecondary => "di7 di8",
            GenericDecorationKind.StringLiteral => "di6",
            GenericDecorationKind.Variable => "di1",
            GenericDecorationKind.CommentSingleLine => "di5",
            GenericDecorationKind.CommentMultiLine => "di5",
            GenericDecorationKind.Function => "di3",
            GenericDecorationKind.PreprocessorDirective => "di9",
            GenericDecorationKind.DeliminationExtended => "di6",
            GenericDecorationKind.Type => "di2",
            _ => string.Empty,
        };
    }
}
