using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Resizes.Displays;

public partial class ResizableColumn : ComponentBase, IDisposable
{
    [Inject]
    private ICommonUtilityService CommonUtilityService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions LeftElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public ElementDimensions RightElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public Func<Task> ReRenderFuncAsync { get; set; } = null!;

    private Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        CommonUtilityService.DragStateChanged += DragStateWrapOnStateChanged;
        CommonUtilityService.AppOptionsStateChanged += OnAppOptionsStateChanged;
    }
    
    private async void OnAppOptionsStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }

    private async void DragStateWrapOnStateChanged()
    {
        if (!CommonUtilityService.GetDragState().ShouldDisplay)
        {
			bool wasTargetOfDragging = _dragEventHandler is not null;

            _dragEventHandler = null;
            _previousDragMouseEventArgs = null;

			if (wasTargetOfDragging)
				CommonUtilityService.AppDimension_NotifyIntraAppResize();
        }
        else
        {
            var mouseEventArgs = CommonUtilityService.GetDragState().MouseEventArgs;

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
		CommonUtilityService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }

    private async Task DragEventHandlerResizeHandleAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeWest(
            LeftElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        ResizeHelper.ResizeEast(
            RightElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        CommonUtilityService.DragStateChanged -= DragStateWrapOnStateChanged;
        CommonUtilityService.AppOptionsStateChanged -= OnAppOptionsStateChanged;
    }
}