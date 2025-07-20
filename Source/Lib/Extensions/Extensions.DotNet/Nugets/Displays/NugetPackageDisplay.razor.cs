using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.CommandLines.Models;

namespace Walk.Extensions.DotNet.Nugets.Displays;

public partial class NugetPackageDisplay : ComponentBase, IDisposable
{
	[Inject]
	private DotNetService DotNetService { get; set; } = null!;

	[Parameter, EditorRequired]
	public NugetPackageRecord NugetPackageRecord { get; set; } = null!;

	private static readonly Key<TerminalCommandRequest> AddNugetPackageTerminalCommandRequestKey = Key<TerminalCommandRequest>.NewKey();

	private string _nugetPackageVersionString = string.Empty;

	private List<NugetPackageVersionRecord> _nugetPackageVersionsOrdered = new();
	private string? _previousNugetPackageId;

	protected override void OnInitialized()
	{
		DotNetService.NuGetPackageManagerStateChanged += OnNuGetPackageManagerStateChanged;
		DotNetService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	}
	
	protected override void OnParametersSet()
	{
		if (_previousNugetPackageId is null || _previousNugetPackageId != NugetPackageRecord.Id)
		{
			_previousNugetPackageId = NugetPackageRecord.Id;

			_nugetPackageVersionsOrdered = NugetPackageRecord.Versions
				.OrderByDescending(x => x.Version)
				.ToList();

			_nugetPackageVersionString = _nugetPackageVersionsOrdered.FirstOrDefault()
				?.Version ?? string.Empty;
		}

		base.OnParametersSet();
	}

	private void SelectedNugetVersionChanged(ChangeEventArgs changeEventArgs)
	{
		_nugetPackageVersionString = changeEventArgs.Value?.ToString() ?? string.Empty;
	}

	private bool ValidateSolutionContainsSelectedProject(
		DotNetSolutionState dotNetSolutionState,
		NuGetPackageManagerState nuGetPackageManagerState)
	{
		if (dotNetSolutionState.DotNetSolutionModel is null || nuGetPackageManagerState.SelectedProjectToModify is null)
			return false;

		return dotNetSolutionState.DotNetSolutionModel.DotNetProjectList.Any(
			x => x.ProjectIdGuid == nuGetPackageManagerState.SelectedProjectToModify.ProjectIdGuid);
	}

	private void AddNugetPackageReferenceOnClick(
		DotNetSolutionState dotNetSolutionState,
		NuGetPackageManagerState nuGetPackageManagerState)
	{
		var targetProject = nuGetPackageManagerState.SelectedProjectToModify;
		var targetNugetPackage = NugetPackageRecord;
		var targetNugetVersion = _nugetPackageVersionString;

		var isValid = ValidateSolutionContainsSelectedProject(dotNetSolutionState, nuGetPackageManagerState);

		if (!isValid || targetProject is null)
		{
			return;
		}

		var parentDirectory = targetProject.AbsolutePath.ParentDirectory;

		if (parentDirectory is null)
			return;

		var formattedCommand = DotNetCliCommandFormatter.FormatAddNugetPackageReferenceToProject(
			targetProject.AbsolutePath.Value,
			targetNugetPackage.Id,
			targetNugetVersion);

		var terminalCommandRequest = new TerminalCommandRequest(
        	formattedCommand.Value,
        	parentDirectory,
        	AddNugetPackageTerminalCommandRequestKey)
        {
        	ContinueWithFunc = parsedCommand =>
        	{
        		NotificationHelper.DispatchInformative("Add Nuget Package Reference", $"{targetNugetPackage.Title}, {targetNugetVersion} was added to {targetProject.DisplayName}", DotNetService.IdeService.CommonService, TimeSpan.FromSeconds(7));
				return Task.CompletedTask;
        	}
        };
        	
        DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
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