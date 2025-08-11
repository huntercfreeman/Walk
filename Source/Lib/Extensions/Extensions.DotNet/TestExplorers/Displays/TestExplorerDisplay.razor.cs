using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib;
using Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Resizes.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays;

public partial class TestExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private ResizableColumnParameter _resizableColumnParameter;

    protected override void OnInitialized()
    {
        var testExplorerState = DotNetService.GetTestExplorerState();
        
        _resizableColumnParameter = new(
            testExplorerState.TreeViewElementDimensions,
            testExplorerState.DetailsElementDimensions,
            () => InvokeAsync(StateHasChanged));
    
        var model = DotNetService.IdeService.TextEditorService.Model_GetOrDefault(
            ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);

        if (model is null)
        {
            DotNetService.IdeService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
            {
                var terminalDecorationMapper = DotNetService.IdeService.TextEditorService.GetDecorationMapper(ExtensionNoPeriodFacts.TERMINAL);
                var terminalCompilerService = DotNetService.IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL);

                model = new TextEditorModel(
                    ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
                    DateTime.UtcNow,
                    ExtensionNoPeriodFacts.TERMINAL,
                    "initialContent:TestExplorerDetailsTextEditorResourceUri",
                    terminalDecorationMapper,
                    terminalCompilerService,
                    DotNetService.IdeService.TextEditorService);

                DotNetService.IdeService.TextEditorService.Model_RegisterCustom(editContext, model);

                DotNetService.IdeService.TextEditorService.ViewModel_Register(
                    editContext,
                    TestExplorerDetailsDisplay.DetailsTextEditorViewModelKey,
                    ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
                    new Category("terminal"));

                var modelModifier = editContext.GetModelModifier(model.PersistentState.ResourceUri);
        
                DotNetService.IdeService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    IdeFacts.Terminal_EmptyPresentationModel);

                DotNetService.IdeService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);

                DotNetService.IdeService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);

                model.PersistentState.CompilerService.RegisterResource(
                    model.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);

                var viewModelModifier = editContext.GetViewModelModifier(TestExplorerDetailsDisplay.DetailsTextEditorViewModelKey);

                var firstPresentationLayerKeys = new List<Key<TextEditorPresentationModel>>
                {
                    IdeFacts.Terminal_PresentationKey,
                    TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
                    TextEditorFacts.FindOverlayPresentation_PresentationKey,
                };

                viewModelModifier.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;

                // await InvokeAsync(StateHasChanged);
            });
        }
    
        DotNetService.DotNetStateChanged += OnTestExplorerStateChanged;
        DotNetService.IdeService.TextEditorService.CommonService.CommonUiStateChanged += OnTreeViewStateChanged;
        DotNetService.IdeService.IdeStateChanged += OnTerminalStateChanged;

        _ = Task.Run(async () =>
        {
            await DotNetService.HandleUserInterfaceWasInitializedEffect()
                .ConfigureAwait(false);
        });
    }

    private void DispatchShouldDiscoverTestsEffect()
    {
        _ = Task.Run(async () =>
        {
            await DotNetService.HandleShouldDiscoverTestsEffect()
                .ConfigureAwait(false);
        });
    }
    
    private void KillExecutionProcessOnClick()
    {
        var terminalState = DotNetService.IdeService.GetTerminalState();
        var executionTerminal = terminalState.TerminalMap[IdeFacts.EXECUTION_KEY];
        executionTerminal.KillProcess();
    }
    
    private bool GetIsKillProcessDisabled()
    {
        var terminalState = DotNetService.IdeService.GetTerminalState();
        var executionTerminal = terminalState.TerminalMap[IdeFacts.EXECUTION_KEY];
        return !executionTerminal.HasExecutingProcess;
    }
    
    private async void OnTestExplorerStateChanged(DotNetStateChangedKind dotNetStateChangedKind)
    {
        if (dotNetStateChangedKind == DotNetStateChangedKind.TestExplorerStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async void OnTreeViewStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.TreeViewStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async void OnTerminalStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.TerminalHasExecutingProcessStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        DotNetService.DotNetStateChanged -= OnTestExplorerStateChanged;
        DotNetService.IdeService.TextEditorService.CommonService.CommonUiStateChanged -= OnTreeViewStateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnTerminalStateChanged;
    }
}
