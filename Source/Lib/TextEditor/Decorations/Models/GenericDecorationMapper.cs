namespace Walk.TextEditor.RazorLib.Decorations.Models;

public class GenericDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (GenericDecorationKind)decorationByte;

        return decoration switch
        {
            GenericDecorationKind.None => string.Empty,
            GenericDecorationKind.Keyword => "di_keyword",
            GenericDecorationKind.KeywordControl => "di_keyword-control",
            GenericDecorationKind.EscapeCharacterPrimary => "di_string-escape",
            GenericDecorationKind.EscapeCharacterSecondary => "di_string-escape di_string-escape-alt",
            GenericDecorationKind.StringLiteral => "di_string",
            GenericDecorationKind.Variable => "di_variable",
            GenericDecorationKind.CommentSingleLine => "di_comment",
            GenericDecorationKind.CommentMultiLine => "di_comment",
            GenericDecorationKind.Function => "di_method",
            GenericDecorationKind.PreprocessorDirective => "di_preprocessor",
            GenericDecorationKind.DeliminationExtended => "di_string",
            GenericDecorationKind.Type => "di_type",
            _ => string.Empty,
        };
    }
}
