using Walk.Ide.RazorLib.CommandBars.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private CommandBarState _commandBarState = new();

    public CommandBarState GetCommandBarState() => _commandBarState;
}
