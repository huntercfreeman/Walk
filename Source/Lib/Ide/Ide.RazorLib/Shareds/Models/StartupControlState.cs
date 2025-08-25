using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct StartupControlState(
    string ActiveStartupProjectAbsolutePathValue,
    TerminalCommandRequest? ExecutingTerminalCommandRequest,
    IReadOnlyList<StartupControlModel> StartupControlList)
{
    public StartupControlState() : this(
        string.Empty,
        ExecutingTerminalCommandRequest: null,
        Array.Empty<StartupControlModel>())
    {
    }
}
