using Microsoft.AspNetCore.Components;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Nugets.Models;

namespace Walk.Extensions.DotNet.Nugets.Displays;

public partial class NuGetPackageManager : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private bool _performingNugetQuery;
    private Exception? _exceptionFromNugetQuery;

    public string NugetQuery
    {
        get => DotNetService.GetNuGetPackageManagerState().NugetQuery;
        set => DotNetService.ReduceSetNugetQueryAction(value);
    }

    public bool IncludePrerelease
    {
        get => DotNetService.GetNuGetPackageManagerState().IncludePrerelease;
        set => DotNetService.ReduceSetIncludePrereleaseAction(value);
    }

    protected override void OnInitialized()
    {
        DotNetService.NuGetPackageManagerStateChanged += OnNuGetPackageManagerStateChanged;
        DotNetService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
    }

    private void SelectedProjectToModifyChanged(ChangeEventArgs changeEventArgs, DotNetSolutionState dotNetSolutionState)
    {
        if (changeEventArgs.Value is null || dotNetSolutionState.DotNetSolutionModel is null)
            return;

        var projectIdGuid = Guid.Parse((string)changeEventArgs.Value);

        IDotNetProject? selectedProject = null;

        if (projectIdGuid != Guid.Empty)
        {
            selectedProject = dotNetSolutionState.DotNetSolutionModel.DotNetProjectList
                .SingleOrDefault(x => x.ProjectIdGuid == projectIdGuid);
        }

        DotNetService.ReduceSetSelectedProjectToModifyAction(selectedProject);
    }

    private bool CheckIfProjectIsSelected(IDotNetProject dotNetProject, NuGetPackageManagerState nuGetPackageManagerState)
    {
        if (nuGetPackageManagerState.SelectedProjectToModify is null)
            return false;

        return nuGetPackageManagerState.SelectedProjectToModify.ProjectIdGuid == dotNetProject.ProjectIdGuid;
    }

    private bool ValidateSolutionContainsSelectedProject(DotNetSolutionState dotNetSolutionState, NuGetPackageManagerState nuGetPackageManagerState)
    {
        if (dotNetSolutionState.DotNetSolutionModel is null || nuGetPackageManagerState.SelectedProjectToModify is null)
            return false;

        return dotNetSolutionState.DotNetSolutionModel.DotNetProjectList.Any(
            x => x.ProjectIdGuid == nuGetPackageManagerState.SelectedProjectToModify.ProjectIdGuid);
    }

    private async Task SubmitNuGetQueryOnClick()
    {
        var query = DotNetService.BuildQuery(NugetQuery, IncludePrerelease);

        try
        {
            // UI
            {
                _exceptionFromNugetQuery = null;
                _performingNugetQuery = true;
                await InvokeAsync(StateHasChanged);
            }

            DotNetService.Enqueue(new DotNetWorkArgs
            {
                WorkKind = DotNetWorkKind.SubmitNuGetQuery,
                NugetPackageManagerQuery = query,
            });
        }
        catch (Exception e)
        {
            _exceptionFromNugetQuery = e;
        }
        finally
        {
            _performingNugetQuery = false;
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async void OnNuGetPackageManagerStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    private async void OnDotNetSolutionStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        DotNetService.NuGetPackageManagerStateChanged -= OnNuGetPackageManagerStateChanged;
        DotNetService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
    }
}
