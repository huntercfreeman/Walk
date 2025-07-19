using Walk.Ide.RazorLib.CommandBars.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
	private CommandBarState _commandBarState = new();

	public event Action? CommandBarStateChanged;

	public CommandBarState GetCommandBarState() => _commandBarState;
}
