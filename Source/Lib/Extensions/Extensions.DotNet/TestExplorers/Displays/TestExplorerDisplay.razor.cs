using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.TestExplorers.Displays;

public partial class TestExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	[Inject]
	private IDecorationMapperRegistry DecorationMapperRegistry { get; set; } = null!;
	[Inject]
	private ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private ITerminalService TerminalService { get; set; } = null!;

	protected override void OnInitialized()
	{
		var model = TextEditorService.ModelApi.GetOrDefault(
			ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);

		if (model is null)
		{
			TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
			{
				var terminalDecorationMapper = DecorationMapperRegistry.GetDecorationMapper(ExtensionNoPeriodFacts.TERMINAL);
				var terminalCompilerService = CompilerServiceRegistry.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL);

				model = new TextEditorModel(
					ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
					DateTime.UtcNow,
					ExtensionNoPeriodFacts.TERMINAL,
					"initialContent:TestExplorerDetailsTextEditorResourceUri",
                    terminalDecorationMapper,
                    terminalCompilerService,
                    TextEditorService);

				TextEditorService.ModelApi.RegisterCustom(editContext, model);

				TextEditorService.ViewModelApi.Register(
					editContext,
					TestExplorerDetailsDisplay.DetailsTextEditorViewModelKey,
					ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
					new Category("terminal"));

				var modelModifier = editContext.GetModelModifier(model.PersistentState.ResourceUri);
		
				TextEditorService.ModelApi.AddPresentationModel(
					editContext,
					modelModifier,
					TerminalPresentationFacts.EmptyPresentationModel);

				TextEditorService.ModelApi.AddPresentationModel(
					editContext,
					modelModifier,
					CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);

				TextEditorService.ModelApi.AddPresentationModel(
					editContext,
					modelModifier,
					FindOverlayPresentationFacts.EmptyPresentationModel);

				model.PersistentState.CompilerService.RegisterResource(
					model.PersistentState.ResourceUri,
					shouldTriggerResourceWasModified: true);

				var viewModelModifier = editContext.GetViewModelModifier(TestExplorerDetailsDisplay.DetailsTextEditorViewModelKey);

				var firstPresentationLayerKeys = new List<Key<TextEditorPresentationModel>>
				{
					TerminalPresentationFacts.PresentationKey,
					CompilerServiceDiagnosticPresentationFacts.PresentationKey,
					FindOverlayPresentationFacts.PresentationKey,
				};

				viewModelModifier.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;

				// await InvokeAsync(StateHasChanged);
			});
		}
	
		DotNetBackgroundTaskApi.TestExplorerService.TestExplorerStateChanged += OnTestExplorerStateChanged;
		CommonUtilityService.TreeViewStateChanged += OnTreeViewStateChanged;
		TerminalService.TerminalStateChanged += OnTerminalStateChanged;

		_ = Task.Run(async () =>
		{
			await DotNetBackgroundTaskApi.TestExplorerService
				.HandleUserInterfaceWasInitializedEffect()
				.ConfigureAwait(false);
		});
	}

	private void DispatchShouldDiscoverTestsEffect()
	{
		_ = Task.Run(async () =>
		{
			await DotNetBackgroundTaskApi.TestExplorerService
				.HandleShouldDiscoverTestsEffect()
				.ConfigureAwait(false);
		});
	}
	
	private void KillExecutionProcessOnClick()
	{
		var terminalState = TerminalService.GetTerminalState();
		var executionTerminal = terminalState.TerminalMap[TerminalFacts.EXECUTION_KEY];
		executionTerminal.KillProcess();
	}
	
	private bool GetIsKillProcessDisabled()
	{
		var terminalState = TerminalService.GetTerminalState();
		var executionTerminal = terminalState.TerminalMap[TerminalFacts.EXECUTION_KEY];
		return !executionTerminal.HasExecutingProcess;
	}
	
	private async void OnTestExplorerStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	private async void OnTreeViewStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	private async void OnTerminalStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetBackgroundTaskApi.TestExplorerService.TestExplorerStateChanged -= OnTestExplorerStateChanged;
		CommonUtilityService.TreeViewStateChanged -= OnTreeViewStateChanged;
		TerminalService.TerminalStateChanged -= OnTerminalStateChanged;
	}
}