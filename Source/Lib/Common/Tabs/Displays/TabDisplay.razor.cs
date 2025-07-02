using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Tabs.Displays;

public partial class TabDisplay : ComponentBase
{
    [CascadingParameter, EditorRequired]
    public TabCascadingValueBatch RenderBatch { get; set; } = null!;

	[Parameter, EditorRequired]
	public ITab Tab { get; set; } = null!;

	[Parameter]
	public string? CssClassString { get; set; }
	[Parameter]
	public bool ShouldDisplayCloseButton { get; set; } = true;

    private bool _thinksLeftMouseButtonIsDown;

	private Key<IDynamicViewModel> _dynamicViewModelKeyPrevious;

	private ElementReference? _tabButtonElementReference;
	
	private string _cssClass = string.Empty;

	private string HtmlId { get; set; } = string.Empty;

	private string IsActiveCssClass => (Tab.TabGroup?.GetIsActive(Tab) ?? false)
		? "di_active"
	    : string.Empty;

    private async Task OnClick(ITab localTabViewModel, MouseEventArgs e)
    {
		var localTabGroup = localTabViewModel.TabGroup;
		if (localTabGroup is null)
			return;
			
		await localTabGroup.OnClickAsync(localTabViewModel, e).ConfigureAwait(false);
    }

	private async Task CloseTabOnClickAsync()
	{
		var localTabViewModel = Tab;

        var localTabGroup = localTabViewModel.TabGroup;
		if (localTabGroup is null)
			return;
        
        await localTabGroup.CloseAsync(Tab).ConfigureAwait(false);
	}

	private async Task HandleOnMouseDownAsync(MouseEventArgs mouseEventArgs)
	{
		if (mouseEventArgs.Button == 0)
	        _thinksLeftMouseButtonIsDown = true;
		if (mouseEventArgs.Button == 1)
            await CloseTabOnClickAsync().ConfigureAwait(false);
		else if (mouseEventArgs.Button == 2)
			ManuallyPropagateOnContextMenu(mouseEventArgs, Tab);
	}

    private void ManuallyPropagateOnContextMenu(
        MouseEventArgs mouseEventArgs,
        ITab tab)
    {
		var localHandleTabButtonOnContextMenu = RenderBatch.HandleTabButtonOnContextMenu;

		if (localHandleTabButtonOnContextMenu is null)
			return;

		RenderBatch.CommonUtilityService.Enqueue(new CommonWorkArgs
		{
    		WorkKind = CommonWorkKind.Tab_ManuallyPropagateOnContextMenu,
			HandleTabButtonOnContextMenu = localHandleTabButtonOnContextMenu,
            TabContextMenuEventArgs = new TabContextMenuEventArgs(mouseEventArgs, tab, FocusAsync),
		});
    }

	private async Task FocusAsync()
	{
		try
		{
			var localTabButtonElementReference = _tabButtonElementReference;

			if (localTabButtonElementReference is not null)
			{
				await localTabButtonElementReference.Value
					.FocusAsync()
					.ConfigureAwait(false);
			}
		}
		catch (Exception)
		{
			// 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
			//             This bug is seemingly happening randomly. I have a suspicion
			//             that there are race-condition exceptions occurring with "FocusAsync"
			//             on an ElementReference.
		}
	}

	private void HandleOnMouseUp()
    {
        _thinksLeftMouseButtonIsDown = false;
    }
    
    private async Task HandleOnMouseOutAsync(MouseEventArgs mouseEventArgs)
    {
        if ((mouseEventArgs.Buttons & 1) == 0)
	        _thinksLeftMouseButtonIsDown = false;
    
        if (_thinksLeftMouseButtonIsDown && Tab is IDrag draggable)
        {
            _thinksLeftMouseButtonIsDown = false;
        
            // This needs to run synchronously to guarantee `dragState.DragElementDimensions` is in a threadsafe state
            // (keep any awaits after it).
            // (only the "UI thread" touches `dragState.DragElementDimensions`).
            var dragState = RenderBatch.DragService.GetDragState();

			dragState.DragElementDimensions.WidthDimensionAttribute.DimensionUnitList.Clear();

			dragState.DragElementDimensions.HeightDimensionAttribute.DimensionUnitList.Clear();

			dragState.DragElementDimensions.LeftDimensionAttribute.DimensionUnitList.Clear();
			dragState.DragElementDimensions.LeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
            	mouseEventArgs.ClientX,
            	DimensionUnitKind.Pixels));

			dragState.DragElementDimensions.TopDimensionAttribute.DimensionUnitList.Clear();
			dragState.DragElementDimensions.TopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
            	mouseEventArgs.ClientY,
            	DimensionUnitKind.Pixels));

            dragState.DragElementDimensions.ElementPositionKind = ElementPositionKind.Fixed;
            
            await draggable.OnDragStartAsync().ConfigureAwait(false);

            SubscribeToDragEventForScrolling(draggable);
        }
    }
    
    public void SubscribeToDragEventForScrolling(IDrag draggable)
    {
		RenderBatch.DragService.ReduceShouldDisplayAndMouseEventArgsAndDragSetAction(true, null, draggable);
    }

	/// <summary>
	/// This method can only be invoked from the "UI thread" due to the shared `UiStringBuilder` usage.
	/// </summary>
	private void CalculateCssClass(ITabGroup localTabGroup, ITab localTabViewModel)
	{
	    var uiStringBuilder = RenderBatch.CommonUtilityService.UiStringBuilder;
	    
	    uiStringBuilder.Clear();
	    uiStringBuilder.Append("di_polymorphic-tab di_button di_unselectable ");
	    uiStringBuilder.Append(IsActiveCssClass);
	    uiStringBuilder.Append(" ");
	    uiStringBuilder.Append(localTabGroup?.GetDynamicCss(localTabViewModel));
	    uiStringBuilder.Append(" ");
	    uiStringBuilder.Append(CssClassString);
	
	    _cssClass = uiStringBuilder.ToString();
	    
	    if (_dynamicViewModelKeyPrevious != Tab.DynamicViewModelKey)
	    {
	        _dynamicViewModelKeyPrevious = Tab.DynamicViewModelKey;
	        HtmlId = $"di_polymorphic-tab_{Tab.DynamicViewModelKey.Guid}";
	    }
	}
}
