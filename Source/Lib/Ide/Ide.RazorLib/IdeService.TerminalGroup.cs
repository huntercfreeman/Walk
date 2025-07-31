using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private TerminalGroupState _terminalGroupState = new();

    public TerminalGroupState GetTerminalGroupState() => _terminalGroupState;

    public void TerminalGroup_SetActiveTerminal(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            _terminalGroupState = _terminalGroupState with
            {
                ActiveTerminalKey = terminalKey
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalGroupStateChanged);
    }
}
