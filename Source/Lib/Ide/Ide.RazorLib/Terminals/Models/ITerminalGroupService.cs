using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public interface ITerminalGroupService
{
	public event Action? TerminalGroupStateChanged;
	
	public TerminalGroupState GetTerminalGroupState();

    public void SetActiveTerminal(Key<ITerminal> terminalKey);
    public void InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit);
}
