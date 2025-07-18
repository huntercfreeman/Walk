using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalOutputFormatterAll : ITerminalOutputFormatter
{
	public static Guid Id { get; } = Guid.NewGuid();

	public static ResourceUri TextEditorModelResourceUri { get; } = new(
        ResourceUriFacts.Terminal_ReservedResourceUri_Prefix + Id.ToString());

    public static Key<TextEditorViewModel> TextEditorViewModelKey { get; } = new Key<TextEditorViewModel>(Id);

	private readonly ITerminal _terminal;
	private readonly TextEditorService _textEditorService;

	public TerminalOutputFormatterAll(
		ITerminal terminal,
		TextEditorService textEditorService)
	{
		_terminal = terminal;
		_textEditorService = textEditorService;
	}

	public string Name { get; } = nameof(TerminalOutputFormatterAll);
	
	public ITerminalOutputFormatted Format()
	{
		return new TerminalOutputFormattedTextEditor(
			string.Empty,
			new List<TerminalCommandParsed>(),
			new List<TextEditorTextSpan>(),
			new List<Symbol>());
	}
	
	public void Dispose()
	{
	}
}
