using System.Text;
using CliWrap.EventStream;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalInteractive : ITerminalInteractive
{
	public const string RESERVED_TARGET_FILENAME_PREFIX = "Walk_";

	private readonly ITerminal _terminal;
	private readonly object _syncRoot = new();
	private readonly List<TerminalCommandRequest> _terminalCommandRequestHistory = new();

	public TerminalInteractive(ITerminal terminal)
	{
		_terminal = terminal;
	}

	private string? _previousWorkingDirectory;
	private string? _workingDirectory;
	
	public string? WorkingDirectory => _workingDirectory;

	public event Action? WorkingDirectoryChanged;
	
	public async Task<TerminalCommandParsed?> TryHandleCommand(TerminalCommandRequest terminalCommandRequest)
	{
		// Store in history
		lock (_syncRoot)
		{
			if (_terminalCommandRequestHistory.Count > 10)
				_terminalCommandRequestHistory.Clear();
				
			_terminalCommandRequestHistory.Insert(0, terminalCommandRequest);
		}
	
		var parsedCommand = Parse(terminalCommandRequest);
		
		// To set the working directory, is not mutually exclusive
		// to the "cd" command. Do not combine these.
		if (terminalCommandRequest.WorkingDirectory is not null &&
			terminalCommandRequest.WorkingDirectory != WorkingDirectory)
		{
			SetWorkingDirectory(terminalCommandRequest.WorkingDirectory);
		}
		
		if (parsedCommand.TargetFileName.StartsWith(RESERVED_TARGET_FILENAME_PREFIX))
		{
			_terminal.TerminalOutput.WriteOutput(
				parsedCommand,
				new StartedCommandEvent(-1));
		
			await parsedCommand.SourceTerminalCommandRequest.BeginWithFunc.Invoke(parsedCommand);
			return null;
		}
		
		switch (parsedCommand.TargetFileName)
		{
			case "cd":
				_terminal.TerminalOutput.WriteOutput(
					parsedCommand,
					new StartedCommandEvent(-1));
			
				SetWorkingDirectory(parsedCommand.Arguments);
				
				_terminal.TerminalOutput.WriteOutput(
					parsedCommand,
					new StandardOutputCommandEvent($"WorkingDirectory set to: '{parsedCommand.Arguments}'\n"));
				return null;
			case "clear":
				_terminal.TerminalOutput.ClearOutput();
				return null;
			default:
				return parsedCommand;
		}
	}
	
	public void SetWorkingDirectory(string workingDirectory)
	{
		_previousWorkingDirectory = _workingDirectory;
        _workingDirectory = workingDirectory;

        if (_previousWorkingDirectory != _workingDirectory)
            WorkingDirectoryChanged?.Invoke();
	}
	
	public List<TerminalCommandRequest> GetTerminalCommandRequestHistory()
	{
		lock (_syncRoot)
		{
			return _terminalCommandRequestHistory;
		}
	}
	
	public TerminalCommandParsed Parse(TerminalCommandRequest terminalCommandRequest)
	{
		try
		{
			var stringWalker = new StringWalker(ResourceUri.Empty, terminalCommandRequest.CommandText);
			
			// Get target file name
			string targetFileName;
			{
				var targetFileNameBuilder = new StringBuilder();
				var startPositionIndex = stringWalker.PositionIndex;
		
				while (!stringWalker.IsEof)
				{
					if (WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
						break;
					else
						targetFileNameBuilder.Append(stringWalker.CurrentCharacter);
				
					_ = stringWalker.ReadCharacter();
				}
				
				targetFileName = targetFileNameBuilder.ToString();
			}
			
			// Get arguments
			stringWalker.SkipWhitespace();
			var arguments = stringWalker.RemainingText;
		
			return new TerminalCommandParsed(
				targetFileName,
				arguments,
				terminalCommandRequest);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			throw;
		}
	}
	
	public void Dispose()
	{
		return;
	}
}
