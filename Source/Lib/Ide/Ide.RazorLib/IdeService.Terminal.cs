using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private readonly object _stateModificationLock = new();

    private TerminalState _terminalState = new();

    public TerminalState GetTerminalState() => _terminalState;
    
    private void AddTerminals()
    {
        ITerminal executionTerminal;
        ITerminal generalTerminal;
        
        if (CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
        {
            executionTerminal = new TerminalWebsite(
                "Execution",
                CommonService,
                TextEditorService)
            {
                Key = IdeFacts.EXECUTION_KEY
            };
            
            generalTerminal = new TerminalWebsite(
                "General",
                CommonService,
                TextEditorService)
            {
                Key = IdeFacts.GENERAL_KEY
            };
        }
        else
        {
            executionTerminal = new Terminal(
                "Execution",
                this)
            {
                Key = IdeFacts.EXECUTION_KEY
            };
            
            generalTerminal = new Terminal(
                "General",
                this)
            {
                Key = IdeFacts.GENERAL_KEY
            };
        }
    
        _terminalState = _terminalState with
        {
            ExecutionTerminal = executionTerminal,
            GeneralTerminal = generalTerminal
        };
    }

    public void Terminal_HasExecutingProcess_StateHasChanged()
    {
        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalHasExecutingProcessStateChanged);
    }
}
