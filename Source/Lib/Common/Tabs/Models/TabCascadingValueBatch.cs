using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Tabs.Models;

public class TabCascadingValueBatch
{
    public IDragService DragService { get; set; } = null!;
    public ICommonUtilityService CommonUtilityService { get; set; } = null!;
	public ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
	public Func<TabContextMenuEventArgs, Task>? HandleTabButtonOnContextMenu { get; set; }
}
