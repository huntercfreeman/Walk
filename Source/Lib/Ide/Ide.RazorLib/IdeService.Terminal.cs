using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private readonly object _stateModificationLock = new();

    private TerminalState _terminalState = new();

    public TerminalState GetTerminalState() => _terminalState;

    public void Terminal_Register(ITerminal terminal)
    {
        lock (_stateModificationLock)
        {
            if (!_terminalState.TerminalMap.ContainsKey(terminal.Key))
            {
                var nextMap = new Dictionary<Key<ITerminal>, ITerminal>(_terminalState.TerminalMap);
                nextMap.Add(terminal.Key, terminal);

                _terminalState = _terminalState with { TerminalMap = nextMap };
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalStateChanged);
    }

    public void Terminal_StateHasChanged()
    {
        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalStateChanged);
    }

    public void Terminal_Dispose(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            var nextMap = new Dictionary<Key<ITerminal>, ITerminal>(_terminalState.TerminalMap);
            nextMap.Remove(terminalKey);

            _terminalState = _terminalState with { TerminalMap = nextMap };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalStateChanged);
    }
}
