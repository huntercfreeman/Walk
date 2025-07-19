using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private DragState _dragState = new();
    
    public event Action? DragStateChanged;
    
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
        
        DragStateChanged?.Invoke();
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
        
        DragStateChanged?.Invoke();
        return;
    }
}
