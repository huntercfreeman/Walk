using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Resizes.Models;

/// <summary>
/// TODO: Rename this type
/// </summary>
public static class ResizableColumn
{
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
}
