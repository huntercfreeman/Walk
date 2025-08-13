using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Tabs.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public interface IPanelTab : ITab
{
    public Key<Panel> Key { get; }
    public CommonService CommonService { get; }
    
    public TabCascadingValueBatch TabCascadingValueBatch { get; set; }
    
    public Task OnClick(MouseEventArgs mouseEventArgs);
    
    public Task HandleOnMouseDownAsync(MouseEventArgs mouseEventArgs);
    
    public Task HandleOnMouseOutAsync(MouseEventArgs mouseEventArgs);
    
    public void ManuallyPropagateOnContextMenu(MouseEventArgs mouseEventArgs);
    
    public Task CloseTabOnClickAsync();
}
