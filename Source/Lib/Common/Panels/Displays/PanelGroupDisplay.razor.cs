using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Tabs.Models;

namespace Walk.Common.RazorLib.Panels.Displays;

public partial class PanelGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public PanelGroupParameter PanelGroupParameter { get; set; }
    
    private TabCascadingValueBatch _tabCascadingValueBatch = new();

    public string DimensionAttributeModificationPurpose { get; private set; }

    private string _panelPositionCss;
    private string _htmlIdTabs;

    protected override void OnInitialized()
    {
        var position = string.Empty;

        if (CommonFacts.LeftPanelGroupKey == PanelGroupParameter.PanelGroupKey)
            position = "left";
        else if (CommonFacts.RightPanelGroupKey == PanelGroupParameter.PanelGroupKey)
            position = "right";
        else if (CommonFacts.BottomPanelGroupKey == PanelGroupParameter.PanelGroupKey)
            position = "bottom";

        _panelPositionCss = $"di_ide_panel_{position}";
        
        _htmlIdTabs = _panelPositionCss + "_tabs";
        
        DimensionAttributeModificationPurpose = $"take_size_of_adjacent_hidden_panel_{PanelGroupParameter.PanelGroupKey}";
    
        CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
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

    private async Task PassAlongSizeIfNoActiveTab()
    {
        var panelState = CommonService.GetPanelState();
        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupParameter.PanelGroupKey);

        if (panelGroup is not null)
        {
            var activePanelTab = panelGroup.TabList.FirstOrDefault(
                x => x.Key == panelGroup.ActiveTabKey);
            
            DimensionAttribute adjacentElementSizeDimensionAttribute;
            DimensionAttribute panelGroupSizeDimensionsAttribute;
            
            switch (PanelGroupParameter.DimensionAttributeKind)
            {
                case DimensionAttributeKind.Width:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.WidthDimensionAttribute;
                    panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.WidthDimensionAttribute;
                    break;
                case DimensionAttributeKind.Height:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.HeightDimensionAttribute;
                    panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.HeightDimensionAttribute;
                    break;
                case DimensionAttributeKind.Left:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.LeftDimensionAttribute;
                    panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.LeftDimensionAttribute;
                    break;
                case DimensionAttributeKind.Right:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.RightDimensionAttribute;
                    panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.RightDimensionAttribute;
                    break;
                case DimensionAttributeKind.Top:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.TopDimensionAttribute;
                    panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.TopDimensionAttribute;
                    break;
                case DimensionAttributeKind.Bottom:
                    adjacentElementSizeDimensionAttribute = PanelGroupParameter.AdjacentElementDimensions.BottomDimensionAttribute;
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

                await PanelGroupParameter.ReRenderSelfAndAdjacentElementDimensionsFunc
                    .Invoke()
                    .ConfigureAwait(false);
            }
            else if (activePanelTab is not null && indexOfPreviousPassAlong != -1)
            {
                adjacentElementSizeDimensionAttribute.DimensionUnitList.RemoveAt(indexOfPreviousPassAlong);

                await PanelGroupParameter.ReRenderSelfAndAdjacentElementDimensionsFunc
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

        return panelGroup?.ElementDimensions.GetStyleString((System.Text.StringBuilder)CommonService.UiStringBuilder) ?? string.Empty;
    }

    private Task TopDropzoneOnMouseUp(MouseEventArgs mouseEventArgs)
    {
        var panelState = CommonService.GetPanelState();

        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupParameter.PanelGroupKey);

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            CommonService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            CommonService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                true);

            CommonService.Panel_SetDragEventArgs(null);

            CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }

    private Task BottomDropzoneOnMouseUp(MouseEventArgs mouseEventArgs)
    {
        var panelState = CommonService.GetPanelState();

        var panelGroup = panelState.PanelGroupList.FirstOrDefault(x => x.Key == PanelGroupParameter.PanelGroupKey);

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            CommonService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            CommonService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                false);

            CommonService.Panel_SetDragEventArgs(null);

            CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.PanelStateChanged)
            await InvokeAsync(StateHasChanged);
    }
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetPanelElementCssClass()
    {
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("di_ide_panel ");
        CommonService.UiStringBuilder.Append(_panelPositionCss);
        CommonService.UiStringBuilder.Append(" ");
        CommonService.UiStringBuilder.Append(PanelGroupParameter.CssClassString);
    
        return CommonService.UiStringBuilder.ToString();
    }
    
    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
    }
}
