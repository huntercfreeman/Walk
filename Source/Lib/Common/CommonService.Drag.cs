using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private DragState _dragState = new();
    
    public DragState GetDragState() => _dragState;
    
    public void Drag_ShouldDisplayAndMouseEventArgsSetAction(
        bool shouldDisplay,
        MouseEventArgs? mouseEventArgs)
    {
        var inState = GetDragState();
    
        _dragState = inState with
        {
            ShouldDisplay = shouldDisplay,
            MouseEventArgs = mouseEventArgs,
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DragStateChanged);
        return;
    }
    
    public void Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(
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
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DragStateChanged);
        return;
    }
}
