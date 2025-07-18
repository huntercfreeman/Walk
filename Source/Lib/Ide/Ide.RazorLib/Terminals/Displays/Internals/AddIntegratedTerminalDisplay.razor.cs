using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib.Terminals.Displays.Internals;

public partial class AddIntegratedTerminalDisplay : ComponentBase
{
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	[Inject]
	private ITerminalService TerminalService { get; set; } = null!;
	
	[CascadingParameter]
	public IDialog Dialog { get; set; } = null!;

	public static Key<TerminalCommandRequest> TypeBashTerminalCommandRequestKey { get; } = Key<TerminalCommandRequest>.NewKey();

	private string _pathToShellExecutable = string.Empty;
	private string _integratedTerminalDisplayName = string.Empty;
	
	protected override void OnInitialized()
	{
		var terminalCommandRequest = new TerminalCommandRequest(
        	"bash -c \"type bash\"",
        	TextEditorService.CommonUtilityService.EnvironmentProvider.HomeDirectoryAbsolutePath.Value,
        	TypeBashTerminalCommandRequestKey)
        {
        	ContinueWithFunc = parsedCommand =>
        	{
        		if (string.IsNullOrWhiteSpace(_pathToShellExecutable))
        		{
        			var output = parsedCommand.OutputCache.ToString();
        			var identifierText = "bash is ";
        		
        			if (output.StartsWith(identifierText))
        			{
        				_pathToShellExecutable = output.Substring(identifierText.Length).Trim();
        				
        				if (string.IsNullOrWhiteSpace(_integratedTerminalDisplayName))
        					_integratedTerminalDisplayName = "Bash";
        				
        				return InvokeAsync(StateHasChanged);
        			}
        		}
        		
        		return Task.CompletedTask;
        	}
        };
        	
        TerminalService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
	}
	
	private void SubmitOnClick()
	{
		var pathToShellExecutableLocal = _pathToShellExecutable;
		var integratedTerminalDisplayNameLocal = _integratedTerminalDisplayName;
		
		var terminalIntegrated = new TerminalIntegrated(
			_integratedTerminalDisplayName,
			terminal => new TerminalInteractive(terminal),
			terminal => new TerminalInputStringBuilder(terminal),
			terminal => new TerminalOutput(
				terminal,
				new TerminalOutputFormatterExpand(
					terminal,
					TextEditorService)),
			TextEditorService.CommonUtilityService,
			_pathToShellExecutable)
		{
			Key = Key<ITerminal>.NewKey()
		};
		
		terminalIntegrated.Start();
		
		TerminalService.Register(terminalIntegrated);
			
		TextEditorService.CommonUtilityService.Dialog_ReduceDisposeAction(Dialog.DynamicViewModelKey);
	}
}