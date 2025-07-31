using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib.Terminals.Displays;

public partial class TerminalGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private Key<IDynamicViewModel> _addIntegratedTerminalDialogKey = Key<IDynamicViewModel>.NewKey();
    
    private ResizableColumnParameter _resizableColumnParameter;

    protected override void OnInitialized()
    {
        var terminalGroupDisplayState = IdeService.GetTerminalGroupState();
    
        _resizableColumnParameter = new(
            terminalGroupDisplayState.BodyElementDimensions,
            terminalGroupDisplayState.TabsElementDimensions,
            () => InvokeAsync(StateHasChanged));
    
        IdeService.IdeStateChanged += OnTerminalGroupStateChanged;
    }

    private void DispatchSetActiveTerminalAction(Key<ITerminal> terminalKey)
    {
        IdeService.TerminalGroup_SetActiveTerminal(terminalKey);
    }
    
    private void ClearTerminalOnClick(Key<ITerminal> terminalKey)
    {
        IdeService.GetTerminalState().TerminalMap[terminalKey]?.ClearFireAndForget();
    }
    
    private async void OnTerminalGroupStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
    
        if (ideStateChangedKind == IdeStateChangedKind.TerminalGroupStateChanged ||
            ideStateChangedKind == IdeStateChangedKind.TerminalStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        IdeService.IdeStateChanged -= OnTerminalGroupStateChanged;
    }
}
