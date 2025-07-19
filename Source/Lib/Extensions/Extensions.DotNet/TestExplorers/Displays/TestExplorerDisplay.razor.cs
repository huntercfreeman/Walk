using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.TestExplorers.Displays;

public partial class TestExplorerDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	protected override void OnInitialized()
	{
		var model = IdeService.TextEditorService.Model_GetOrDefault(
			ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);

		if (model is null)
		{
			IdeService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
			{
				var terminalDecorationMapper = IdeService.TextEditorService.GetDecorationMapper(ExtensionNoPeriodFacts.TERMINAL);
				var terminalCompilerService = IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL);

				model = new TextEditorModel(
					ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
					DateTime.UtcNow,
					ExtensionNoPeriodFacts.TERMINAL,
					"initialContent:TestExplorerDetailsTextEditorResourceUri",
                    terminalDecorationMapper,
                    terminalCompilerService,
                    IdeService.TextEditorService);

				IdeService.TextEditorService.Model_RegisterCustom(editContext, model);

				IdeService.TextEditorService.ViewModel_Register(
					editContext,
					TestExplorerDetailsDisplay.DetailsTextEditorViewModelKey,
					ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
					new Category("terminal"));

				var modelModifier = editContext.GetModelModifier(model.PersistentState.ResourceUri);
		
				IdeService.TextEditorService.Model_AddPresentationModel(
					editContext,
					modelModifier,
					TerminalPresentationFacts.EmptyPresentationModel);

				IdeService.TextEditorService.Model_AddPresentationModel(
					editContext,
					modelModifier,
					CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);

				IdeService.TextEditorService.Model_AddPresentationModel(
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
		IdeService.TextEditorService.CommonService.TreeViewStateChanged += OnTreeViewStateChanged;
		IdeService.TerminalStateChanged += OnTerminalStateChanged;

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
		var terminalState = IdeService.GetTerminalState();
		var executionTerminal = terminalState.TerminalMap[TerminalFacts.EXECUTION_KEY];
		executionTerminal.KillProcess();
	}
	
	private bool GetIsKillProcessDisabled()
	{
		var terminalState = IdeService.GetTerminalState();
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
		IdeService.TextEditorService.CommonService.TreeViewStateChanged -= OnTreeViewStateChanged;
		IdeService.TerminalStateChanged -= OnTerminalStateChanged;
	}
}