using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.CommandLines.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CommandLines.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class DotNetSolutionFormDisplay : ComponentBase, IDisposable
{
	[Inject]
	private ITerminalService TerminalService { get; set; } = null!;
	[Inject]
	private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

	[CascadingParameter]
	public IDialog DialogRecord { get; set; } = null!;

	private string _solutionName = string.Empty;
	private string _parentDirectoryName = string.Empty;

	public Key<TerminalCommandRequest> NewDotNetSolutionTerminalCommandRequestKey { get; } = Key<TerminalCommandRequest>.NewKey();
	public CancellationTokenSource NewDotNetSolutionCancellationTokenSource { get; set; } = new();

	private string DisplaySolutionName => string.IsNullOrWhiteSpace(_solutionName)
		? "{enter solution name}"
		: _solutionName;

	private string DisplayParentDirectoryName => string.IsNullOrWhiteSpace(_parentDirectoryName)
		? "{enter parent directory name}"
		: _parentDirectoryName;

	private FormattedCommand FormattedCommand => DotNetCliCommandFormatter.FormatDotnetNewSln(_solutionName);
	
	protected override void OnInitialized()
	{
		TerminalService.TerminalStateChanged += OnTerminalStateChanged;
	}

	private void RequestInputFileForParentDirectory()
	{
		IdeBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
		{
			WorkKind = IdeBackgroundTaskApiWorkKind.RequestInputFileStateForm,
			Message = "Directory for new .NET Solution",
			OnAfterSubmitFunc = async absolutePath =>
			{
				if (absolutePath.ExactInput is null)
					return;

				_parentDirectoryName = absolutePath.Value;
				await InvokeAsync(StateHasChanged);
			},
			SelectionIsValidFunc = absolutePath =>
			{
				if (absolutePath.ExactInput is null || !absolutePath.IsDirectory)
					return Task.FromResult(false);

				return Task.FromResult(true);
			},
			InputFilePatterns = new()
			{
				new InputFilePattern("Directory", absolutePath => absolutePath.IsDirectory)
			}
		});
	}

	private async Task StartNewDotNetSolutionCommandOnClick()
	{
		var localFormattedCommand = FormattedCommand;
		var localSolutionName = _solutionName;
		var localParentDirectoryName = _parentDirectoryName;

		if (string.IsNullOrWhiteSpace(localSolutionName) ||
			string.IsNullOrWhiteSpace(localParentDirectoryName))
		{
			return;
		}

		if (CommonUtilityService.WalkHostingInformation.WalkHostingKind != WalkHostingKind.Photino)
		{
			await HackForWebsite_StartNewDotNetSolutionCommandOnClick(
					localSolutionName,
					localParentDirectoryName)
				.ConfigureAwait(false);
		}
		else
		{
			var terminalCommandRequest = new TerminalCommandRequest(
	        	localFormattedCommand.Value,
	        	_parentDirectoryName,
	        	new Key<TerminalCommandRequest>(NewDotNetSolutionTerminalCommandRequestKey.Guid))
	        {
	        	ContinueWithFunc = parsedCommand =>
	        	{
	        		// Close Dialog
					CommonUtilityService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

					// Open the created .NET Solution
					var parentDirectoryAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
						localParentDirectoryName,
						true);

					var solutionAbsolutePathString =
						parentDirectoryAbsolutePath.Value +
						localSolutionName +
						CommonUtilityService.EnvironmentProvider.DirectorySeparatorChar +
						localSolutionName +
						'.' +
						ExtensionNoPeriodFacts.DOT_NET_SOLUTION;

					var solutionAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
						solutionAbsolutePathString,
						false);

					DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
					{
						WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
						DotNetSolutionAbsolutePath = solutionAbsolutePath,
					});
					return Task.CompletedTask;
	        	}
	        };
	        	
	        TerminalService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
		}
	}

	private async Task HackForWebsite_StartNewDotNetSolutionCommandOnClick(
		string localSolutionName,
		string localParentDirectoryName)
	{
		var directoryContainingSolution =
			CommonUtilityService.EnvironmentProvider.JoinPaths(localParentDirectoryName, localSolutionName) +
			CommonUtilityService.EnvironmentProvider.DirectorySeparatorChar;

		await CommonUtilityService.FileSystemProvider.Directory
			.CreateDirectoryAsync(directoryContainingSolution)
			.ConfigureAwait(false);

		var localSolutionFilenameWithExtension =
			localSolutionName +
			'.' +
			ExtensionNoPeriodFacts.DOT_NET_SOLUTION;

		var solutionAbsolutePathString = CommonUtilityService.EnvironmentProvider.JoinPaths(
			directoryContainingSolution,
			localSolutionFilenameWithExtension);

		await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
				solutionAbsolutePathString,
				HackForWebsite_NEW_SOLUTION_TEMPLATE)
			.ConfigureAwait(false);

		// Close Dialog
		CommonUtilityService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

		NotificationHelper.DispatchInformative("Website .sln template was used", "No terminal available", CommonUtilityService, TimeSpan.FromSeconds(7));

		var solutionAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
			solutionAbsolutePathString,
			false);

		DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
		{
			WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
			DotNetSolutionAbsolutePath = solutionAbsolutePath,
		});
	}

	public const string HackForWebsite_NEW_SOLUTION_TEMPLATE = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.7.34018.315
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{EC571C96-8996-402C-B44A-264F84598795}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{EC571C96-8996-402C-B44A-264F84598795}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{EC571C96-8996-402C-B44A-264F84598795}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{EC571C96-8996-402C-B44A-264F84598795}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {CC0E8FC7-3D42-4480-BAF6-86D1E2F2289E}
	EndGlobalSection
EndGlobal
";

	public async void OnTerminalStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}

	public void Dispose()
	{
		TerminalService.TerminalStateChanged -= OnTerminalStateChanged;
	}
}