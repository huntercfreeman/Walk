using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Drags.Models;

public record struct DragState(
    bool ShouldDisplay,
    MouseEventArgs? MouseEventArgs,
	IDrag? Drag)
{
    public DragState() : this (false, null, null)
    {
        
    }
}