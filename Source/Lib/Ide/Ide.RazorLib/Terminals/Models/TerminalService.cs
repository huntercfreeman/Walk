using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalService : ITerminalService
{
    private readonly object _stateModificationLock = new();

    private TerminalState _terminalState = new();

	public event Action? TerminalStateChanged;

	public TerminalState GetTerminalState() => _terminalState;

    public void Register(ITerminal terminal)
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

        TerminalStateChanged?.Invoke();
    }

    public void StateHasChanged()
    {
    	TerminalStateChanged?.Invoke();
    }

    public void Dispose(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            var nextMap = new Dictionary<Key<ITerminal>, ITerminal>(_terminalState.TerminalMap);
            nextMap.Remove(terminalKey);

            _terminalState = _terminalState with { TerminalMap = nextMap };
        }

        TerminalStateChanged?.Invoke();
    }
}
