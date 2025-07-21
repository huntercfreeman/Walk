using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.Nugets.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
	private NuGetPackageManagerState _nuGetPackageManagerState = new();

	public event Action? NuGetPackageManagerStateChanged;

	public NuGetPackageManagerState GetNuGetPackageManagerState() => _nuGetPackageManagerState;

	public void ReduceSetSelectedProjectToModifyAction(IDotNetProject? selectedProjectToModify)
	{
		var inState = GetNuGetPackageManagerState();

		_nuGetPackageManagerState = inState with
		{
			SelectedProjectToModify = selectedProjectToModify
		};

		NuGetPackageManagerStateChanged?.Invoke();
		return;
	}

	public void ReduceSetNugetQueryAction(string nugetQuery)
	{
		var inState = GetNuGetPackageManagerState();

		_nuGetPackageManagerState = inState with { NugetQuery = nugetQuery };

		NuGetPackageManagerStateChanged?.Invoke();
		return;
	}

	public void ReduceSetIncludePrereleaseAction(bool includePrerelease)
	{
		var inState = GetNuGetPackageManagerState();

		_nuGetPackageManagerState = inState with { IncludePrerelease = includePrerelease };

		NuGetPackageManagerStateChanged?.Invoke();
		return;
	}

	public void ReduceSetMostRecentQueryResultAction(List<NugetPackageRecord> queryResultList)
	{
		var inState = GetNuGetPackageManagerState();

		_nuGetPackageManagerState = inState with { QueryResultList = queryResultList };

		NuGetPackageManagerStateChanged?.Invoke();
		return;
	}
}
