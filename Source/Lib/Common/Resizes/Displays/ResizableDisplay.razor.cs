using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Resizes.Displays;

public partial class ResizableDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions ElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public Func<Task> ReRenderFuncAsync { get; set; } = null!;

    [Parameter]
    public IDrag? Drag { get; set; } = null!;

    public const double RESIZE_HANDLE_SQUARE_PIXELS = 10;

    private Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    private ElementDimensions _northResizeHandleDimensions = new();
    private ElementDimensions _eastResizeHandleDimensions = new();
    private ElementDimensions _southResizeHandleDimensions = new();
    private ElementDimensions _westResizeHandleDimensions = new();
    private ElementDimensions _northEastResizeHandleDimensions = new();
    private ElementDimensions _southEastResizeHandleDimensions = new();
    private ElementDimensions _southWestResizeHandleDimensions = new();
    private ElementDimensions _northWestResizeHandleDimensions = new();

    protected override void OnInitialized()
    {
        CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
    }

    private async void DragStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind != CommonUiEventKind.DragStateChanged)
            return;
        
        if (!CommonService.GetDragState().ShouldDisplay)
        {
            var wasTargetOfDragging = _dragEventHandler is not null;

            _dragEventHandler = null;
            _previousDragMouseEventArgs = null;

            if (wasTargetOfDragging)
                CommonService.AppDimension_NotifyIntraAppResize();
        }
        else
        {
            var mouseEventArgs = CommonService.GetDragState().MouseEventArgs;

            if (_dragEventHandler is not null)
            {
                if (_previousDragMouseEventArgs is not null && mouseEventArgs is not null)
                {
                    await _dragEventHandler
                        .Invoke((_previousDragMouseEventArgs, mouseEventArgs))
                        .ConfigureAwait(false);
                }

                _previousDragMouseEventArgs = mouseEventArgs;
                await ReRenderFuncAsync.Invoke().ConfigureAwait(false);
            }
        }
    }

    private async Task SubscribeToDragEventAsync(
        Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task> dragEventHandler)
    {
        _dragEventHandler = dragEventHandler;

        if (Drag is not null)
            await Drag.OnDragStartAsync().ConfigureAwait(false);

        CommonService.Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(true, null, Drag);
    }

    public Task SubscribeToDragEventWithMoveHandle()
    {
        return SubscribeToDragEventAsync(DragEventHandlerMoveHandleAsync);
    }

    #region ResizeHandleStyleCss

    private string GetNorthResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _northResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;
        
        // Width
        _northResizeHandleDimensions.Width_Base_0 = parentElementDimensions.Width_Base_0;
        _northResizeHandleDimensions.Width_Base_1 = parentElementDimensions.Width_Offset;
        _northResizeHandleDimensions.Width_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Height
        _northResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _northResizeHandleDimensions.Left_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        // Top
        _northResizeHandleDimensions.Top_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        return _northResizeHandleDimensions.GetStyleString(CommonService.UiStringBuilder);
    }

    private string GetEastResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _eastResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _eastResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _eastResizeHandleDimensions.Height_Base_0 = parentElementDimensions.Height_Base_0;
        _eastResizeHandleDimensions.Height_Base_1 = parentElementDimensions.Height_Offset;
        _eastResizeHandleDimensions.Height_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Left
        _eastResizeHandleDimensions.Left_Base_0 = parentElementDimensions.Width_Base_0;
        _eastResizeHandleDimensions.Left_Base_1 = parentElementDimensions.Width_Offset;
        _eastResizeHandleDimensions.Left_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Top
        _eastResizeHandleDimensions.Top_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        return _eastResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    private string GetSouthResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _southResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _southResizeHandleDimensions.Width_Base_0 = parentElementDimensions.Width_Base_0;
        _southResizeHandleDimensions.Width_Base_1 = parentElementDimensions.Width_Offset;
        _southResizeHandleDimensions.Width_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Height
        _southResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _southResizeHandleDimensions.Left_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);
                
        // Top
        _southResizeHandleDimensions.Top_Base_0 = parentElementDimensions.Height_Base_0;
        _southResizeHandleDimensions.Top_Base_1 = parentElementDimensions.Height_Offset;
        _southResizeHandleDimensions.Top_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        return _southResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    private string GetWestResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _westResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _westResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _westResizeHandleDimensions.Height_Base_0 = parentElementDimensions.Height_Base_0;
        _westResizeHandleDimensions.Height_Base_1 = parentElementDimensions.Height_Offset;
        _westResizeHandleDimensions.Height_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Left
        _westResizeHandleDimensions.Left_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        // Top
        _westResizeHandleDimensions.Top_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        return _westResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    private string GetNorthEastResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _northEastResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _northEastResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _northEastResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _northEastResizeHandleDimensions.Left_Base_0 = parentElementDimensions.Width_Base_0;
        _northEastResizeHandleDimensions.Left_Base_1 = parentElementDimensions.Width_Offset;
        _northEastResizeHandleDimensions.Left_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Top
        _northEastResizeHandleDimensions.Top_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        return _northEastResizeHandleDimensions.GetStyleString(CommonService.UiStringBuilder);
    }

    private string GetSouthEastResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _southEastResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _southEastResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _southEastResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _southEastResizeHandleDimensions.Left_Base_0 = parentElementDimensions.Width_Base_0;
        _southEastResizeHandleDimensions.Left_Base_1 = parentElementDimensions.Width_Offset;
        _southEastResizeHandleDimensions.Left_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // Top
        _southEastResizeHandleDimensions.Top_Base_0 = parentElementDimensions.Height_Base_0;
        _southEastResizeHandleDimensions.Top_Base_1 = parentElementDimensions.Height_Offset;
        _southEastResizeHandleDimensions.Top_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        return _southEastResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    private string GetSouthWestResizeHandleStyleCss()
    {
        var parentElementDimensions = ElementDimensions;

        _southWestResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _southWestResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _southWestResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _southWestResizeHandleDimensions.Left_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        // Top
        _southWestResizeHandleDimensions.Top_Base_0 = parentElementDimensions.Height_Base_0;
        _southWestResizeHandleDimensions.Top_Base_1 = parentElementDimensions.Height_Offset;
        _southWestResizeHandleDimensions.Top_Offset = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        return _southWestResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    private string GetNorthWestResizeHandleStyleCss()
    {
        _northWestResizeHandleDimensions.ElementPositionKind = ElementPositionKind.Absolute;

        // Width
        _northWestResizeHandleDimensions.Width_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Height
        _northWestResizeHandleDimensions.Height_Base_0 = new DimensionUnit(RESIZE_HANDLE_SQUARE_PIXELS, DimensionUnitKind.Pixels);

        // Left
        _northWestResizeHandleDimensions.Left_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        // Top
        _northWestResizeHandleDimensions.Top_Base_0 = new DimensionUnit(-1 * RESIZE_HANDLE_SQUARE_PIXELS / 2, DimensionUnitKind.Pixels);

        return _northWestResizeHandleDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder);
    }

    #endregion

    #region DragEventHandlers

    private async Task DragEventHandlerNorthResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeNorth(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerEastResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeEast(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerSouthResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeSouth(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerWestResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeWest(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerNorthEastResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeNorthEast(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerSouthEastResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeSouthEast(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerSouthWestResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeSouthWest(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerNorthWestResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeNorthWest(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    private async Task DragEventHandlerMoveHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.Move(
            ElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
    }
}
