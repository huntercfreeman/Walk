namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct StartupControlState(
    string ActiveStartupProjectAbsolutePathValue,
    IReadOnlyList<StartupControlModel> StartupControlList)
{
    public StartupControlState() : this(
        string.Empty,
        Array.Empty<StartupControlModel>())
    {
    }
}
