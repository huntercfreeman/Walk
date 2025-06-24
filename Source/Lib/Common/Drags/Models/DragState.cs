using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dialogs.Models;

namespace Walk.Common.RazorLib.Drags.Models;

public record struct DragState(
    bool ShouldDisplay,
    MouseEventArgs? MouseEventArgs,
	IDrag? Drag,
	ElementDimensions DragElementDimensions)
{
    public DragState() : this (false, null, null, DialogHelper.ConstructDefaultElementDimensions())
    {
        
    }
}