using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;

namespace Walk.Common.RazorLib.Tabs.Models;

public class TabCascadingValueBatch
{
    public IDragService DragService { get; set; } = null!;
    public INotificationService NotificationService { get; set; } = null!;
    public IAppOptionsService AppOptionsService { get; set; } = null!;
	public CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
	public ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
	public Func<TabContextMenuEventArgs, Task>? HandleTabButtonOnContextMenu { get; set; }
}
