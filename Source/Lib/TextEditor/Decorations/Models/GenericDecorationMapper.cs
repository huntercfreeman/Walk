namespace Walk.TextEditor.RazorLib.Decorations.Models;

public class GenericDecorationMapper : IDecorationMapper
{
	public string Map(byte decorationByte)
	{
		var decoration = (GenericDecorationKind)decorationByte;

		return decoration switch
		{
			GenericDecorationKind.None => string.Empty,
			GenericDecorationKind.Keyword => "di_te_keyword",
			GenericDecorationKind.KeywordControl => "di_te_keyword-control",
			GenericDecorationKind.EscapeCharacterPrimary => "di_te_string-literal-escape-character",
			GenericDecorationKind.EscapeCharacterSecondary => "di_te_string-literal-escape-character di_te_string-literal-escape-character-secondary",
			GenericDecorationKind.StringLiteral => "di_te_string-literal",
			GenericDecorationKind.Variable => "di_te_variable",
			GenericDecorationKind.CommentSingleLine => "di_te_comment",
			GenericDecorationKind.CommentMultiLine => "di_te_comment",
			GenericDecorationKind.Function => "di_te_method",
			GenericDecorationKind.PreprocessorDirective => "di_te_preprocessor-directive",
			GenericDecorationKind.DeliminationExtended => "di_te_string-literal",
			GenericDecorationKind.Type => "di_te_type",
			_ => string.Empty,
		};
	}
}
