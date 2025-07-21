using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalCommandParsed
{
    public TerminalCommandParsed(
        string targetFileName,
        string arguments,
        TerminalCommandRequest sourceTerminalCommandRequest)
    {
        TargetFileName = targetFileName;
        Arguments = arguments;
        SourceTerminalCommandRequest = sourceTerminalCommandRequest;
    }

    public string TargetFileName { get; }
    public string Arguments { get; }
    public StringBuilderCache OutputCache { get; } = new();
    public TerminalCommandRequest SourceTerminalCommandRequest { get; }
    public List<TextEditorTextSpan> TextSpanList { get; set; }
}
