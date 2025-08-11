using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Resizes.Displays;

public partial class ResizableColumn : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ResizableColumnParameter ResizableColumnParameter { get; set; }

    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    protected override void OnInitialized()
    {
        CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
    }
    
    public async void DragStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind != CommonUiEventKind.DragStateChanged)
            return;
            
        await Do(
            CommonService,
            ResizableColumnParameter.LeftElementDimensions,
            ResizableColumnParameter.RightElementDimensions,
            _dragEventHandler,
            _previousDragMouseEventArgs,
            x => _dragEventHandler = x,
            x => _previousDragMouseEventArgs = x);
        await ResizableColumnParameter.ReRenderFuncAsync.Invoke().ConfigureAwait(false);
    }
    
    public static async Task Do(
        CommonService commonService,
        ElementDimensions leftElementDimensions,
        ElementDimensions rightElementDimensions,
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
                        .Invoke(leftElementDimensions, rightElementDimensions, (previousDragMouseEventArgs, mouseEventArgs))
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
        ElementDimensions leftElementDimensions,
        ElementDimensions rightElementDimensions,
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        ResizeHelper.ResizeWest(
            leftElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);

        ResizeHelper.ResizeEast(
            rightElementDimensions,
            mouseEventArgsTuple.firstMouseEventArgs,
            mouseEventArgsTuple.secondMouseEventArgs);
    
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
    }
}
