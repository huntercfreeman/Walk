using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.Websites.ProjectTemplates.Models;
using Walk.Extensions.DotNet.Websites;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Displays;

public partial class CSharpProjectFormDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private DotNetCliOutputParser DotNetCliOutputParser { get; set; } = null!;

	[CascadingParameter]
	public IDialog DialogRecord { get; set; } = null!;

	[Parameter]
	public Key<DotNetSolutionModel> DotNetSolutionModelKey { get; set; }

	private CSharpProjectFormViewModel _viewModel = null!;

	private DotNetSolutionModel? DotNetSolutionModel => DotNetBackgroundTaskApi.DotNetSolutionService.GetDotNetSolutionState().DotNetSolutionsList.FirstOrDefault(
		x => x.Key == DotNetSolutionModelKey);

	protected override void OnInitialized()
	{
		_viewModel = new(DotNetSolutionModel, IdeService.TextEditorService.CommonUtilityService.EnvironmentProvider);
		
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
		IdeService.TerminalStateChanged += OnTerminalStateChanged;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await ReadProjectTemplates().ConfigureAwait(false);
		}
	}

	private string GetIsActiveCssClassString(CSharpProjectFormPanelKind panelKind) =>
		_viewModel.ActivePanelKind == panelKind ? "di_active" : string.Empty;

	private void RequestInputFileForParentDirectory(string message)
	{
		IdeService.Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.RequestInputFileStateForm,
			StringValue = message,
			OnAfterSubmitFunc = async absolutePath =>
			{
				if (absolutePath.ExactInput is null)
					return;

				_viewModel.ParentDirectoryNameValue = absolutePath.Value;
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

	private async Task ReadProjectTemplates()
	{
		if (IdeService.TextEditorService.CommonUtilityService.WalkHostingInformation.WalkHostingKind != WalkHostingKind.Photino)
		{
			_viewModel.ProjectTemplateList = WebsiteProjectTemplateFacts.WebsiteProjectTemplatesContainer.ToList();
			await InvokeAsync(StateHasChanged);
		}
		else
		{
			await EnqueueDotNetNewListAsync().ConfigureAwait(false);
		}
	}

	private async Task EnqueueDotNetNewListAsync()
	{
		try
		{
			// Render UI loading icon
			_viewModel.IsReadingProjectTemplates = true;
			await InvokeAsync(StateHasChanged);

			var formattedCommand = DotNetCliCommandFormatter.FormatDotnetNewList();
			var generalTerminal = IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY];
				
			var terminalCommandRequest = new TerminalCommandRequest(
				formattedCommand.Value,
				IdeService.CommonUtilityService.EnvironmentProvider.HomeDirectoryAbsolutePath.Value,
				new Key<TerminalCommandRequest>(_viewModel.LoadProjectTemplatesTerminalCommandRequestKey.Guid))
			{
				ContinueWithFunc = parsedTerminalCommand =>
				{
					DotNetCliOutputParser.ParseOutputLineDotNetNewList(parsedTerminalCommand.OutputCache.ToString());
					_viewModel.ProjectTemplateList = DotNetCliOutputParser.ProjectTemplateList ?? new();
					return InvokeAsync(StateHasChanged);
				}
			};

			generalTerminal.EnqueueCommand(terminalCommandRequest);
		}
		finally
		{
			// UI loading message
			_viewModel.IsReadingProjectTemplates = false;
			await InvokeAsync(StateHasChanged);
		}
	}

	/// <summary>If the non-deprecated version of the command fails, then try the deprecated one.</summary>
	private async Task EnqueueDotnetNewListDeprecatedAsync()
	{
		try
		{
			// UI loading message
			_viewModel.IsReadingProjectTemplates = true;
			await InvokeAsync(StateHasChanged);

			var formattedCommand = DotNetCliCommandFormatter.FormatDotnetNewListDeprecated();
			var generalTerminal = IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY];

			var terminalCommandRequest = new TerminalCommandRequest(
	        	formattedCommand.Value,
	        	IdeService.TextEditorService.CommonUtilityService.EnvironmentProvider.HomeDirectoryAbsolutePath.Value,
	        	new Key<TerminalCommandRequest>(_viewModel.LoadProjectTemplatesTerminalCommandRequestKey.Guid))
	        {
	        	ContinueWithFunc = parsedCommand =>
	        	{
		        	DotNetCliOutputParser.ParseOutputLineDotNetNewList(parsedCommand.OutputCache.ToString());
					_viewModel.ProjectTemplateList = DotNetCliOutputParser.ProjectTemplateList ?? new();
					return InvokeAsync(StateHasChanged);
				}
	        };
	        	
	        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
		}
		finally
		{
			// UI loading message
			_viewModel.IsReadingProjectTemplates = false;
			await InvokeAsync(StateHasChanged);
		}
	}

	private string GetCssClassForActivePanelKind(CSharpProjectFormPanelKind localActivePanelKind)
	{
		return localActivePanelKind switch
		{
			CSharpProjectFormPanelKind.Graphical => "di_ide_c-sharp-project-form-graphical-panel",
			CSharpProjectFormPanelKind.Manual => "di_ide_c-sharp-project-form-manual-panel",
			_ => throw new NotImplementedException($"The {nameof(CSharpProjectFormPanelKind)}: '{localActivePanelKind}' was unrecognized."),
		};
	}

	private async Task StartNewCSharpProjectCommandOnClick()
	{
		if (!_viewModel.TryTakeSnapshot(out var immutableView) ||
			immutableView is null)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(immutableView.ProjectTemplateShortNameValue) ||
			string.IsNullOrWhiteSpace(immutableView.CSharpProjectNameValue) ||
			string.IsNullOrWhiteSpace(immutableView.ParentDirectoryNameValue))
		{
			return;
		}

		if (IdeService.TextEditorService.CommonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
		{
			var generalTerminal = IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY];

			var terminalCommandRequest = new TerminalCommandRequest(
	        	immutableView.FormattedNewCSharpProjectCommand.Value,
	        	immutableView.ParentDirectoryNameValue,
	        	new Key<TerminalCommandRequest>(immutableView.NewCSharpProjectTerminalCommandRequestKey.Guid))
	        {
	        	ContinueWithFunc = parsedCommand =>
	        	{
					var terminalCommandRequest = new TerminalCommandRequest(
			        	immutableView.FormattedAddExistingProjectToSolutionCommand.Value,
			        	immutableView.ParentDirectoryNameValue,
			        	new Key<TerminalCommandRequest>(immutableView.AddCSharpProjectToSolutionTerminalCommandRequestKey.Guid))
			        {
			        	ContinueWithFunc = parsedCommand =>
			        	{
				        	IdeService.TextEditorService.CommonUtilityService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);
	
							DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
							{
								WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
								DotNetSolutionAbsolutePath = immutableView.DotNetSolutionModel.NamespacePath.AbsolutePath,
							});
							return Task.CompletedTask;
						}
			        };
			        	
			        generalTerminal.EnqueueCommand(terminalCommandRequest);
					return Task.CompletedTask;
	        	}
	        };
	        	
	        generalTerminal.EnqueueCommand(terminalCommandRequest);
		}
		else
		{
			await WebsiteDotNetCliHelper.StartNewCSharpProjectCommand(
					immutableView,
					(IEnvironmentProvider)IdeService.TextEditorService.CommonUtilityService.EnvironmentProvider,
					(IFileSystemProvider)IdeService.TextEditorService.CommonUtilityService.FileSystemProvider,
					DotNetBackgroundTaskApi,
					(Common.RazorLib.Options.Models.CommonUtilityService)IdeService.TextEditorService.CommonUtilityService,
					DialogRecord,
					(ICommonComponentRenderers)IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers)
				.ConfigureAwait(false);
		}
	}
	
	public async void OnDotNetSolutionStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public async void OnTerminalStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
		IdeService.TerminalStateChanged -= OnTerminalStateChanged;
	}
}
