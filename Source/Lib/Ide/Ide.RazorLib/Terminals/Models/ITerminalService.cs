using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public interface ITerminalService
{
	public event Action? TerminalStateChanged;

	public TerminalState GetTerminalState();

    public void Register(ITerminal terminal);
    public void StateHasChanged();
    public void Dispose(Key<ITerminal> terminalKey);
}
