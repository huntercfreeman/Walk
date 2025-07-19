using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Tabs.Models;

namespace Walk.Common.RazorLib.Tabs.Displays;

public partial class TabListDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

	/// <summary>
	/// The list provided should not be modified after passing it as a parameter..
	/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
	/// </summary>
	[Parameter, EditorRequired]
	public List<ITab> TabList { get; set; } = null!;
	
	private TabCascadingValueBatch _tabCascadingValueBatch = new();
	
    public async Task NotifyStateChangedAsync()
	{
		await InvokeAsync(StateHasChanged);
	}

	private Task HandleTabButtonOnContextMenu(TabContextMenuEventArgs tabContextMenuEventArgs)
    {
		var dropdownRecord = new DropdownRecord(
			TabContextMenu.ContextMenuEventDropdownKey,
			tabContextMenuEventArgs.MouseEventArgs.ClientX,
			tabContextMenuEventArgs.MouseEventArgs.ClientY,
			typeof(TabContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(TabContextMenu.TabContextMenuEventArgs),
					tabContextMenuEventArgs
				}
			},
			restoreFocusOnClose: null);

        CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }
}