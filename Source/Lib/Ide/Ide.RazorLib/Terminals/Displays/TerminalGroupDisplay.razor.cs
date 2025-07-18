using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.Terminals.Displays;

public partial class TerminalGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

	private Key<IDynamicViewModel> _addIntegratedTerminalDialogKey = Key<IDynamicViewModel>.NewKey();

	protected override void OnInitialized()
	{
		TerminalGroupService.TerminalGroupStateChanged += OnTerminalGroupStateChanged;
    	TerminalService.TerminalStateChanged += OnTerminalStateChanged;
	}

    private void DispatchSetActiveTerminalAction(Key<ITerminal> terminalKey)
    {
        TerminalGroupService.SetActiveTerminal(terminalKey);
    }
    
    private void ClearTerminalOnClick(Key<ITerminal> terminalKey)
    {
    	TerminalService.GetTerminalState().TerminalMap[terminalKey]?.ClearFireAndForget();
    }
    
    private async void OnTerminalGroupStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    private async void OnTerminalStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TerminalGroupService.TerminalGroupStateChanged -= OnTerminalGroupStateChanged;
    	TerminalService.TerminalStateChanged -= OnTerminalStateChanged;
    }
}