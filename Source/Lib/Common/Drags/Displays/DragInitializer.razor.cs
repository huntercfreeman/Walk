using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Drags.Displays;

public partial class DragInitializer : ComponentBase, IDisposable
{
    [Inject]
    private IDragService DragService { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    
    /* Start DragInitializer */
    private string StyleCss => DragService.GetDragState().ShouldDisplay
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
    /* End DragInitializer */
    
    protected override void OnInitialized()
    {
        _throttle = new(ThrottleFacts.TwentyFour_Frames_Per_Second, async (args, _) =>
	    {
	    	if (args.IsOnMouseMove)
	    	{
	    		if ((args.MouseEventArgs.Buttons & 1) != 1)
	                DRAG_DispatchClearDragStateAction();
	            else
	                DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(true, args.MouseEventArgs);
	            
	            var drag = DragService.GetDragState().Drag;
	            
	            if (drag?.DragComponentType is not null)
	            {
    				drag.DragElementDimensions.LeftDimensionAttribute.DimensionUnitList.Clear();
    				drag.DragElementDimensions.LeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
    	            	args.MouseEventArgs.ClientX,
    	            	DimensionUnitKind.Pixels));
    
    				drag.DragElementDimensions.TopDimensionAttribute.DimensionUnitList.Clear();
    				drag.DragElementDimensions.TopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
    	            	args.MouseEventArgs.ClientY,
    	            	DimensionUnitKind.Pixels));
	            }
	
	            return;
	    	}
	    	else
	    	{
	    		var dragState = DragService.GetDragState();
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
    
        DragService.DragStateChanged += OnDragStateChanged;
    }
    
    /* Start DragInitializer */
    private async void OnDragStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End DragInitializer */
    
    private void DRAG_DispatchClearDragStateAction()
    {
		_onMouseOverDropzone = null;
		
        DragService.ReduceShouldDisplayAndMouseEventArgsAndDragSetAction(
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
        DragService.DragStateChanged -= OnDragStateChanged;
    }
}
