using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib;
using Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays;

public partial class TestExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        var testExplorerState = DotNetService.GetTestExplorerState();
    
        var model = DotNetService.IdeService.TextEditorService.Model_GetOrDefault(
            ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);

        if (model is null)
        {
            DotNetService.IdeService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
            {
                var terminalDecorationMapper = DotNetService.IdeService.TextEditorService.GetDecorationMapper(CommonFacts.TERMINAL);
                var terminalCompilerService = DotNetService.IdeService.TextEditorService.GetCompilerService(CommonFacts.TERMINAL);

                model = new TextEditorModel(
                    ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri,
                    DateTime.UtcNow,
                    CommonFacts.TERMINAL,
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
        DotNetService.IdeService.TextEditorService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
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
        var executionTerminal = terminalState.ExecutionTerminal;
        executionTerminal.KillProcess();
    }
    
    private bool GetIsKillProcessDisabled()
    {
        var terminalState = DotNetService.IdeService.GetTerminalState();
        var executionTerminal = terminalState.ExecutionTerminal;
        return !executionTerminal.HasExecutingProcess;
    }
    
    private async void OnTestExplorerStateChanged(DotNetStateChangedKind dotNetStateChangedKind)
    {
        if (dotNetStateChangedKind == DotNetStateChangedKind.TestExplorerStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.TreeViewStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
        else if (commonUiEventKind == CommonUiEventKind.DragStateChanged)
        {
            if (_dragEventHandler is not null)
            {
                var testExplorerState = DotNetService.GetTestExplorerState();
                await ResizableColumn.Do(
                    DotNetService.CommonService,
                    testExplorerState.TreeViewElementDimensions,
                    testExplorerState.DetailsElementDimensions,
                    _dragEventHandler,
                    _previousDragMouseEventArgs,
                    x => _dragEventHandler = x,
                    x => _previousDragMouseEventArgs = x);
                
                await InvokeAsync(StateHasChanged);
            }
        }
    }
    
    private async void OnTerminalStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.TerminalHasExecutingProcessStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void SubscribeToDragEvent()
    {
        _dragEventHandler = ResizableColumn.DragEventHandlerResizeHandleAsync;
        DotNetService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }
    
    public void Dispose()
    {
        DotNetService.DotNetStateChanged -= OnTestExplorerStateChanged;
        DotNetService.IdeService.TextEditorService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnTerminalStateChanged;
    }
}
