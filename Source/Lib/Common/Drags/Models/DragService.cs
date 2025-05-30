using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Drags.Models;

public class DragService : IDragService
{
    private DragState _dragState = new();
    
    public event Action? DragStateChanged;
    
    public DragState GetDragState() => _dragState;
    
    public void ReduceShouldDisplayAndMouseEventArgsSetAction(
        bool shouldDisplay,
        MouseEventArgs? mouseEventArgs)
    {
    	var inState = GetDragState();
    
        _dragState = inState with
        {
        	ShouldDisplay = shouldDisplay,
            MouseEventArgs = mouseEventArgs,
        };
        
        DragStateChanged?.Invoke();
        return;
    }
    
    public void ReduceShouldDisplayAndMouseEventArgsAndDragSetAction(
        bool shouldDisplay,
		MouseEventArgs? mouseEventArgs,
		IDrag? drag)
    {
    	var inState = GetDragState();
    	
        _dragState = inState with
        {
        	ShouldDisplay = shouldDisplay,
            MouseEventArgs = mouseEventArgs,
            Drag = drag,
        };
        
        DragStateChanged?.Invoke();
        return;
    }
}