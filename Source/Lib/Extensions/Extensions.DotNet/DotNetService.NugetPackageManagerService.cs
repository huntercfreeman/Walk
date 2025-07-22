using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.Nugets.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
    private NuGetPackageManagerState _nuGetPackageManagerState = new();

    public NuGetPackageManagerState GetNuGetPackageManagerState() => _nuGetPackageManagerState;

    public void ReduceSetSelectedProjectToModifyAction(IDotNetProject? selectedProjectToModify)
    {
        var inState = GetNuGetPackageManagerState();

        _nuGetPackageManagerState = inState with
        {
            SelectedProjectToModify = selectedProjectToModify
        };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.NuGetPackageManagerStateChanged);
        return;
    }

    public void ReduceSetNugetQueryAction(string nugetQuery)
    {
        var inState = GetNuGetPackageManagerState();

        _nuGetPackageManagerState = inState with { NugetQuery = nugetQuery };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.NuGetPackageManagerStateChanged);
        return;
    }

    public void ReduceSetIncludePrereleaseAction(bool includePrerelease)
    {
        var inState = GetNuGetPackageManagerState();

        _nuGetPackageManagerState = inState with { IncludePrerelease = includePrerelease };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.NuGetPackageManagerStateChanged);
        return;
    }

    public void ReduceSetMostRecentQueryResultAction(List<NugetPackageRecord> queryResultList)
    {
        var inState = GetNuGetPackageManagerState();

        _nuGetPackageManagerState = inState with { QueryResultList = queryResultList };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.NuGetPackageManagerStateChanged);
        return;
    }
}
