using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (TerminalDecorationKind)decorationByte;

        return decoration switch
        {
            TerminalDecorationKind.None => string.Empty,
            TerminalDecorationKind.Comment => "di_comment",
            TerminalDecorationKind.Keyword => "di_keyword",
            TerminalDecorationKind.StringLiteral => "di_string",
            TerminalDecorationKind.TargetFilePath => "di_type",
            TerminalDecorationKind.Error => "di_tree-view-exception",
            TerminalDecorationKind.Warning => "di_tree-view-warning",
            _ => string.Empty,
        };
    }
}
