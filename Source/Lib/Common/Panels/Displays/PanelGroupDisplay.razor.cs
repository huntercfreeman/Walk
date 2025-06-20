using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Panels.Displays;

public partial class PanelGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IPanelService PanelService { get; set; } = null!;
    [Inject]
    private IDragService DragService { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<PanelGroup> PanelGroupKey { get; set; } = Key<PanelGroup>.Empty;
    [Parameter, EditorRequired]
    public ElementDimensions AdjacentElementDimensions { get; set; }
    [Parameter, EditorRequired]
    public DimensionAttributeKind DimensionAttributeKind { get; set; }
    [Parameter, EditorRequired]
    public Func<Task> ReRenderSelfAndAdjacentElementDimensionsFunc { get; set; } = null!;

    [Parameter]
    public string CssClassString { get; set; } = null!;
    [Parameter]
    public RenderFragment? JustifyEndRenderFragment { get; set; } = null;

    public string DimensionAttributeModificationPurpose => $"take_size_of_adjacent_hidden_panel_{PanelGroupKey}";

    private string PanelPositionCssClass => GetPanelPositionCssClass();
    
    protected override void OnInitialized()
    {
    	PanelService.PanelStateChanged += OnPanelStateChanged;
    
    	base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
			// TODO: Why is 'PassAlongSizeIfNoActiveTab()' only invoked if its the firstRender?
            await PassAlongSizeIfNoActiveTab()
                .ConfigureAwait(false);
        }
    }

	private List<IPanelTab> GetTabList(PanelGroup panelGroup)
	{
		var tabList = new List<IPanelTab>();

		foreach (var panelTab in panelGroup.TabList)
		{
            panelTab.TabGroup = panelGroup;
			tabList.Add(panelTab);
		}

		return tabList;
	}

    private string GetHtmlId()
	{
		return GetPanelPositionCssClass();
	}

    private string GetPanelPositionCssClass()
    {
        var position = string.Empty;

        if (PanelFacts.LeftPanelGroupKey == PanelGroupKey)
            position = "left";
        else if (PanelFacts.RightPanelGroupKey == PanelGroupKey)
            position = "right";
        else if (PanelFacts.BottomPanelGroupKey == PanelGroupKey)
            position = "bottom";

        return $"di_ide_panel_{position}";
    }

    private async Task PassAlongSizeIfNoActiveTab()
    {
        var panelState = PanelService.GetPanelState();
        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupKey);

        if (panelGroup is not null)
        {
            var activePanelTab = panelGroup.TabList.FirstOrDefault(
                x => x.Key == panelGroup.ActiveTabKey);
            
            DimensionAttribute adjacentElementSizeDimensionAttribute;
            DimensionAttribute panelGroupSizeDimensionsAttribute;
            
            switch (DimensionAttributeKind)
            {
            	case DimensionAttributeKind.Width:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.WidthDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.WidthDimensionAttribute;
            		break;
			    case DimensionAttributeKind.Height:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.HeightDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.HeightDimensionAttribute;
            		break;
			    case DimensionAttributeKind.Left:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.LeftDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.LeftDimensionAttribute;
            		break;
			    case DimensionAttributeKind.Right:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.RightDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.RightDimensionAttribute;
            		break;
			    case DimensionAttributeKind.Top:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.TopDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.TopDimensionAttribute;
            		break;
			    case DimensionAttributeKind.Bottom:
            		adjacentElementSizeDimensionAttribute = AdjacentElementDimensions.BottomDimensionAttribute;
            		panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.BottomDimensionAttribute;
            		break;
			    default:
			    	return;
            }
            
            var indexOfPreviousPassAlong = adjacentElementSizeDimensionAttribute.DimensionUnitList.FindIndex(
                x => x.Purpose == DimensionAttributeModificationPurpose);

            if (activePanelTab is null && indexOfPreviousPassAlong == -1)
            {
                var panelGroupPercentageSize = panelGroupSizeDimensionsAttribute.DimensionUnitList.First(
                    x => x.DimensionUnitKind == DimensionUnitKind.Percentage);

                adjacentElementSizeDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
                	panelGroupPercentageSize.Value,
                	panelGroupPercentageSize.DimensionUnitKind,
                	DimensionOperatorKind.Add,
                	DimensionAttributeModificationPurpose));

                await ReRenderSelfAndAdjacentElementDimensionsFunc
                    .Invoke()
                    .ConfigureAwait(false);
            }
            else if (activePanelTab is not null && indexOfPreviousPassAlong != -1)
            {
                adjacentElementSizeDimensionAttribute.DimensionUnitList.RemoveAt(indexOfPreviousPassAlong);

                await ReRenderSelfAndAdjacentElementDimensionsFunc
                    .Invoke()
                    .ConfigureAwait(false);
            }
        }
    }

    private string GetElementDimensionsStyleString(PanelGroup? panelGroup, IPanelTab? activePanelTab)
    {
        if (activePanelTab is null)
        {
            return "calc(" +
                   "var(--di_ide_panel-tabs-font-size)" +
                   " + var(--di_ide_panel-tabs-margin)" +
                   " + var(--di_ide_panel-tabs-bug-are-not-aligning-need-to-fix-todo))";
        }

        return panelGroup?.ElementDimensions.GetStyleString(CommonBackgroundTaskApi.UiStringBuilder) ?? string.Empty;
    }

    private Task TopDropzoneOnMouseUp(MouseEventArgs mouseEventArgs)
    {
        var panelState = PanelService.GetPanelState();

        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupKey);

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            PanelService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            PanelService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                true);

            PanelService.SetDragEventArgs(null);

			DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }

    private Task BottomDropzoneOnMouseUp(MouseEventArgs mouseEventArgs)
    {
        var panelState = PanelService.GetPanelState();

        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupKey);

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            PanelService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            PanelService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                false);

            PanelService.SetDragEventArgs(null);

			DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }
    
    private async void OnPanelStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	PanelService.PanelStateChanged -= OnPanelStateChanged;
    }
}