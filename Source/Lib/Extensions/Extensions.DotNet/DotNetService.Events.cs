namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
    public event Action<DotNetStateChangedKind>? DotNetStateChanged;
}

public enum DotNetStateChangedKind
{
    CliOutputParserStateChanged,
    SolutionStateChanged,
    NuGetPackageManagerStateChanged,
    OutputStateChanged,
    TestExplorerStateChanged,
}
