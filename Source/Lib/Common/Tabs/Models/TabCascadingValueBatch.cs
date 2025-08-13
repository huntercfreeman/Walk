using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Tabs.Models;

public class TabCascadingValueBatch
{
    public bool ThinksLeftMouseButtonIsDown { get; set; }
    public Func<TabContextMenuEventArgs, Task>? HandleTabButtonOnContextMenu { get; set; }
    public Action<IDrag>? SubscribeToDragEventForScrolling { get; set; }
}
