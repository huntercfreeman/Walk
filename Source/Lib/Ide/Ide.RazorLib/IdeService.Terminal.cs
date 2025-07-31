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
                terminal => new TerminalInteractive(terminal),
                terminal => new TerminalInputStringBuilder(terminal),
                terminal => new TerminalOutput(
                    terminal,
                    new TerminalOutputFormatterExpand(
                        terminal,
                        TextEditorService)),
                CommonService)
            {
                Key = IdeFacts.EXECUTION_KEY
            };
            
            generalTerminal = new TerminalWebsite(
                "General",
                terminal => new TerminalInteractive(terminal),
                terminal => new TerminalInputStringBuilder(terminal),
                terminal => new TerminalOutput(
                    terminal,
                    new TerminalOutputFormatterExpand(
                        terminal,
                        TextEditorService)),
                CommonService)
            {
                Key = IdeFacts.GENERAL_KEY
            };
        }
        else
        {
            executionTerminal = new Terminal(
                "Execution",
                terminal => new TerminalInteractive(terminal),
                terminal => new TerminalInputStringBuilder(terminal),
                terminal => new TerminalOutput(
                    terminal,
                    new TerminalOutputFormatterExpand(
                        terminal,
                        TextEditorService)),
                this)
            {
                Key = IdeFacts.EXECUTION_KEY
            };
            
            generalTerminal = new Terminal(
                "General",
                terminal => new TerminalInteractive(terminal),
                terminal => new TerminalInputStringBuilder(terminal),
                terminal => new TerminalOutput(
                    terminal,
                    new TerminalOutputFormatterExpand(
                        terminal,
                        TextEditorService)),
                this)
            {
                Key = IdeFacts.GENERAL_KEY
            };
        }
    
        _terminalState = _terminalState with
        {
            TerminalMap = new Dictionary<Key<ITerminal>, ITerminal>
            {
                {
                    executionTerminal.Key,
                    executionTerminal
                },
                {
                    generalTerminal.Key,
                    generalTerminal
                }
            }
        };
    }

    public void Terminal_StateHasChanged()
    {
        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalStateChanged);
    }
}
