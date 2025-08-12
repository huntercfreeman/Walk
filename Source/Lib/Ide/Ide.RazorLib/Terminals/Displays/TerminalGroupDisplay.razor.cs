using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Resizes.Displays;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib.Terminals.Displays;

public partial class TerminalGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private Key<IDynamicViewModel> _addIntegratedTerminalDialogKey = Key<IDynamicViewModel>.NewKey();
    
    private ResizableColumnParameter _resizableColumnParameter;
    
    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        var terminalGroupDisplayState = IdeService.GetTerminalGroupState();
    
        _resizableColumnParameter = new(
            terminalGroupDisplayState.BodyElementDimensions,
            terminalGroupDisplayState.TabsElementDimensions,
            () => InvokeAsync(StateHasChanged));
    
        IdeService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        IdeService.IdeStateChanged += OnTerminalGroupStateChanged;
    }

    private void DispatchSetActiveTerminalAction(Key<ITerminal> terminalKey)
    {
        IdeService.TerminalGroup_SetActiveTerminal(terminalKey);
    }
    
    private void ClearTerminalOnClick(Key<ITerminal> terminalKey)
    {
        IdeService.GetTerminalState().TerminalMap[terminalKey]?.ClearFireAndForget();
    }
    
    private async void OnTerminalGroupStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
    
        if (ideStateChangedKind == IdeStateChangedKind.TerminalGroupStateChanged ||
            ideStateChangedKind == IdeStateChangedKind.TerminalHasExecutingProcessStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.DragStateChanged)
        {
            if (_dragEventHandler is not null)
            {
                await ResizableColumn.Do(
                    IdeService.CommonService,
                    _resizableColumnParameter.LeftElementDimensions,
                    _resizableColumnParameter.RightElementDimensions,
                    _dragEventHandler,
                    _previousDragMouseEventArgs,
                    x => _dragEventHandler = x,
                    x => _previousDragMouseEventArgs = x);
                
                await InvokeAsync(StateHasChanged);
            }
        }
    }
    
    public void SubscribeToDragEvent()
    {
        _dragEventHandler = ResizableColumn.DragEventHandlerResizeHandleAsync;
        IdeService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }

    public void Dispose()
    {
        IdeService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        IdeService.IdeStateChanged -= OnTerminalGroupStateChanged;
    }
}
