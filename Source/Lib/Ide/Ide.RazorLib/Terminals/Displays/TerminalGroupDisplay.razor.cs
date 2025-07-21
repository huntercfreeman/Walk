using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib.Terminals.Displays;

public partial class TerminalGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private Key<IDynamicViewModel> _addIntegratedTerminalDialogKey = Key<IDynamicViewModel>.NewKey();

    protected override void OnInitialized()
    {
        IdeService.TerminalGroupStateChanged += OnTerminalGroupStateChanged;
        IdeService.TerminalStateChanged += OnTerminalStateChanged;
    }

    private void DispatchSetActiveTerminalAction(Key<ITerminal> terminalKey)
    {
        IdeService.TerminalGroup_SetActiveTerminal(terminalKey);
    }
    
    private void ClearTerminalOnClick(Key<ITerminal> terminalKey)
    {
        IdeService.GetTerminalState().TerminalMap[terminalKey]?.ClearFireAndForget();
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
        IdeService.TerminalGroupStateChanged -= OnTerminalGroupStateChanged;
        IdeService.TerminalStateChanged -= OnTerminalStateChanged;
    }
}
