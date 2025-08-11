using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Drags.Displays;

public partial class DragInitializer : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;
    
    private string StyleCss => CommonService.GetDragState().ShouldDisplay
        ? string.Empty
        : "display: none;";

    private ThrottleOptimized<MouseEvent> _throttle;
    
    public struct MouseEvent
    {
        public MouseEvent(bool isOnMouseMove, MouseEventArgs mouseEventArgs)
        {
            IsOnMouseMove = isOnMouseMove;
            MouseEventArgs = mouseEventArgs;
        }
    
        public bool IsOnMouseMove { get; }
        public MouseEventArgs MouseEventArgs { get; }
    }

    private IDropzone? _onMouseOverDropzone = null;
    
    protected override void OnInitialized()
    {
        _throttle = new(CommonFacts.ThrottleFacts_TwentyFour_Frames_Per_Second, async (args, _) =>
        {
            if (args.IsOnMouseMove)
            {
                if ((args.MouseEventArgs.Buttons & 1) != 1)
                    DRAG_DispatchClearDragStateAction();
                else
                    CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, args.MouseEventArgs);
                
                var dragState = CommonService.GetDragState();
                
                if (dragState.Drag?.DragComponentType is not null)
                {
                    dragState.DragElementDimensions.Left_Offset = new DimensionUnit(args.MouseEventArgs.ClientX, DimensionUnitKind.Pixels);
                    dragState.DragElementDimensions.Top_Offset = new DimensionUnit(args.MouseEventArgs.ClientY, DimensionUnitKind.Pixels);
                }
    
                return;
            }
            else
            {
                var dragState = CommonService.GetDragState();
                var localOnMouseOverDropzone = _onMouseOverDropzone;
            
                DRAG_DispatchClearDragStateAction();
    
                var draggableViewModel = dragState.Drag;
                if (draggableViewModel is not null)
                {
                    await draggableViewModel
                        .OnDragEndAsync(args.MouseEventArgs, localOnMouseOverDropzone)
                        .ConfigureAwait(false);
                }
            }
        });
    
        CommonService.CommonUiStateChanged += OnDragStateChanged;
    }
    
    private async void OnDragStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.DragStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private void DRAG_DispatchClearDragStateAction()
    {
        _onMouseOverDropzone = null;
        
        CommonService.Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(
            false,
            null,
            null);
    }

    private void DRAG_DispatchSetDragStateActionOnMouseMove(MouseEventArgs mouseEventArgs)
    {
        _throttle.Run(new(isOnMouseMove: true, mouseEventArgs));
    }

    private void DRAG_DispatchSetDragStateActionOnMouseUp(MouseEventArgs mouseEventArgs)
    {
        _throttle.Run(new(isOnMouseMove: false, mouseEventArgs));
    }

    private string DRAG_GetIsActiveCssClass(IDropzone dropzone)
    {
        var onMouseOverDropzoneKey = _onMouseOverDropzone?.DropzoneKey ?? Key<IDropzone>.Empty;

        return onMouseOverDropzoneKey == dropzone.DropzoneKey
            ? "di_active"
            : string.Empty;
    }
    
    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnDragStateChanged;
    }
}
