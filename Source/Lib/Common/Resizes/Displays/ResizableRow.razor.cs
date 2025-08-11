using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Resizes.Displays;

public partial class ResizableRow : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ResizableRowParameter ResizableRowParameter { get; set; }

    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
    }
    
    private async void DragStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind != CommonUiEventKind.DragStateChanged)
            return;
        
        await Do(
            CommonService,
            ResizableRowParameter.TopElementDimensions,
            ResizableRowParameter.BottomElementDimensions,
            _dragEventHandler,
            _previousDragMouseEventArgs,
            x => _dragEventHandler = x,
            x => _previousDragMouseEventArgs = x);
        await ResizableRowParameter.ReRenderFuncAsync.Invoke().ConfigureAwait(false);
    }
    
    public static async Task Do(
        CommonService commonService,
        ElementDimensions topElementDimensions,
        ElementDimensions bottomElementDimensions,
        Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? dragEventHandler,
        MouseEventArgs? previousDragMouseEventArgs,
        Action<Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>?> setDragEventHandler,
        Action<MouseEventArgs?> setPreviousDragMouseEventArgs)
    {
        if (!commonService.GetDragState().ShouldDisplay)
        {
            bool wasTargetOfDragging = dragEventHandler is not null;

            setDragEventHandler.Invoke(null);
            setPreviousDragMouseEventArgs.Invoke(null);

            if (wasTargetOfDragging)
                commonService.AppDimension_NotifyIntraAppResize();
        }
        else
        {
            var mouseEventArgs = commonService.GetDragState().MouseEventArgs;

            if (dragEventHandler is not null)
            {
                if (previousDragMouseEventArgs is not null && mouseEventArgs is not null)
                {
                    await dragEventHandler
                        .Invoke(topElementDimensions, bottomElementDimensions, (previousDragMouseEventArgs, mouseEventArgs))
                        .ConfigureAwait(false);
                }

                setPreviousDragMouseEventArgs.Invoke(mouseEventArgs);
            }
        }
    }
    
    public static void SubscribeToDragEvent(
        CommonService commonService,
        Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task> dragEventHandler,
        Action<Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>?> setDragEventHandler)
    {
        setDragEventHandler.Invoke(dragEventHandler);
        commonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }

    public static Task DragEventHandlerResizeHandleAsync(
        ElementDimensions topElementDimensions,
        ElementDimensions bottomElementDimensions,
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeNorth(
            topElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        ResizeHelper.ResizeSouth(
            bottomElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
    }
}
