using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Resizes.Displays;

public partial class ResizableRow : ComponentBase, IDisposable
{
    [Inject]
    private IDragService DragService { get; set; } = null!;
    [Inject]
    private IAppDimensionService AppDimensionService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions TopElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public ElementDimensions BottomElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public Func<Task> ReRenderFuncAsync { get; set; } = null!;

    private Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        DragService.DragStateChanged += DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged += OnAppOptionsStateChanged;

        base.OnInitialized();
    }
    
    private async void OnAppOptionsStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }

    private async void DragStateWrapOnStateChanged()
    {
        if (!DragService.GetDragState().ShouldDisplay)
        {
			bool wasTargetOfDragging = _dragEventHandler is not null;

            _dragEventHandler = null;
            _previousDragMouseEventArgs = null;

			if (wasTargetOfDragging)
				AppDimensionService.NotifyIntraAppResize();
        }
        else
        {
            var mouseEventArgs = DragService.GetDragState().MouseEventArgs;

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

    private void SubscribeToDragEvent(
        Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task> dragEventHandler)
    {
        _dragEventHandler = dragEventHandler;
        DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(true, null);
    }

    private async Task DragEventHandlerResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeNorth(
            TopElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        ResizeHelper.ResizeSouth(
            BottomElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DragService.DragStateChanged -= DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged -= OnAppOptionsStateChanged;
    }
}