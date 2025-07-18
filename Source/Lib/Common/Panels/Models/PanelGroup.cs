using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Panels.Models;

/// <summary>
/// Once the 'TabList' is exposed publically,
/// it should NOT be modified.
/// Make a shallow copy 'new List<IPanelTab>(panelGroup.TabList);'
/// and modify the shallow copy if modification of the list
/// after exposing it publically is necessary.
/// </summary>
public record PanelGroup(
	    Key<PanelGroup> Key,
	    Key<Panel> ActiveTabKey,
	    ElementDimensions ElementDimensions,
	    IReadOnlyList<IPanelTab> TabList)
	: ITabGroup
{
	/// <summary>
	/// TODO: Make this property immutable. Until then in a hack needs to be done where this gets set...
	///       ...for Walk.Ide this is done in WalkIdeInitializer.razor.cs (2024-04-08)
	/// </summary>
	public CommonService CommonService { get; set; } = null!;

    public bool GetIsActive(ITab tab)
	{
		if (tab is not IPanelTab panelTab)
			return false;

		return ActiveTabKey == panelTab.Key;
	}

	public Task OnClickAsync(ITab tab, MouseEventArgs mouseEventArgs)
	{
		if (tab is not IPanelTab panelTab)
			return Task.CompletedTask;

		if (GetIsActive(tab))
			CommonService.SetActivePanelTab(Key, Key<Panel>.Empty);
		else
			CommonService.SetActivePanelTab(Key, panelTab.Key);
		
		return Task.CompletedTask;
	}

	public string GetDynamicCss(ITab tab)
	{
		return string.Empty;
	}

	public Task CloseAsync(ITab tab)
	{
		if (tab is not IPanelTab panelTab)
			return Task.CompletedTask;

		CommonService.DisposePanelTab(Key, panelTab.Key);
		return Task.CompletedTask;
	}

	public async Task CloseAllAsync()
	{
		var localTabList = TabList;

		foreach (var tab in localTabList)
		{
			await CloseAsync(tab).ConfigureAwait(false);
		}
	}

	public async Task CloseOthersAsync(ITab safeTab)
    {
        var localTabList = TabList;

		if (safeTab is not IPanelTab safePanelTab)
			return;
		
		// Invoke 'OnClickAsync' to set the active tab to the "safe tab"
		// OnClickAsync does not currently use its mouse event args argument.
		await OnClickAsync(safeTab, null);

        foreach (var tab in localTabList)
        {
			var shouldClose = safePanelTab.Key != tab.Key;

			if (shouldClose)
				await CloseAsync(tab).ConfigureAwait(false);
        }
    }
}