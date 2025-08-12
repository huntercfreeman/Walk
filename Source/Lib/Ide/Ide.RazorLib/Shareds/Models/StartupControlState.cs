using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct StartupControlState(
    string ActiveStartupProjectAbsolutePathValue,
    IReadOnlyList<IStartupControlModel> StartupControlList)
{
    public StartupControlState() : this(
        string.Empty,
        Array.Empty<IStartupControlModel>())
    {
    }
}
