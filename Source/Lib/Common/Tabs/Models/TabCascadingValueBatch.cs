namespace Walk.Common.RazorLib.Tabs.Models;

public class TabCascadingValueBatch
{
    public CommonService CommonService { get; set; } = null!;
	public Func<TabContextMenuEventArgs, Task>? HandleTabButtonOnContextMenu { get; set; }
}
