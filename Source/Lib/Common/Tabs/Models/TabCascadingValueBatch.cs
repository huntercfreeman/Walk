using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Tabs.Models;

public class TabCascadingValueBatch
{
    public CommonUtilityService CommonUtilityService { get; set; } = null!;
	public Func<TabContextMenuEventArgs, Task>? HandleTabButtonOnContextMenu { get; set; }
}
