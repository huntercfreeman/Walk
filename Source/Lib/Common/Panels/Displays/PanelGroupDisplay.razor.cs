using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Panels.Displays;

public partial class PanelGroupDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public PanelGroupParameter PanelGroupParameter { get; set; }
    
    private TabCascadingValueBatch _tabCascadingValueBatch = new();

    public DimensionUnitPurposeKind DimensionUnitPurposeKind { get; private set; }

    private string _panelPositionCss;
    private string _htmlIdTabs;

    protected override void OnInitialized()
    {
        var position = string.Empty;

        if (CommonFacts.LeftPanelGroupKey == PanelGroupParameter.PanelGroupKey)
        {
            position = "left";
            DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_left;
        }
        else if (CommonFacts.RightPanelGroupKey == PanelGroupParameter.PanelGroupKey)
        {
            position = "right";
            DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_right;
        }
        else if (CommonFacts.BottomPanelGroupKey == PanelGroupParameter.PanelGroupKey)
        {
            position = "bottom";
            DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_bottom;
        }

        _panelPositionCss = $"di_ide_panel_{position}";
        
        _htmlIdTabs = _panelPositionCss + "_tabs";
        
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
                x => x.Purpose == DimensionUnitPurposeKind);

            if (activePanelTab is null && indexOfPreviousPassAlong == -1)
            {
                var panelGroupPercentageSize = panelGroupSizeDimensionsAttribute.DimensionUnitList.First(
                    x => x.DimensionUnitKind == DimensionUnitKind.Percentage);

                adjacentElementSizeDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
                    panelGroupPercentageSize.Value,
                    panelGroupPercentageSize.DimensionUnitKind,
                    DimensionOperatorKind.Add,
                    DimensionUnitPurposeKind));

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
    
    #region TabDisplay
    private bool _thinksLeftMouseButtonIsDown;

    private Key<IDynamicViewModel> _dynamicViewModelKeyPrevious;

    private ElementReference? _tabButtonElementReference;
    
    private string GetIsActiveCssClass(ITab localTabViewModel) => (localTabViewModel.TabGroup?.GetIsActive(localTabViewModel) ?? false)
        ? "di_active"
        : string.Empty;

    private async Task OnClick(ITab localTabViewModel, MouseEventArgs e)
    {
        var localTabGroup = localTabViewModel.TabGroup;
        if (localTabGroup is null)
            return;
            
        await localTabGroup.OnClickAsync(localTabViewModel, e).ConfigureAwait(false);
    }

    private async Task CloseTabOnClickAsync(ITab localTabViewModel)
    {
        var localTabGroup = localTabViewModel.TabGroup;
        if (localTabGroup is null)
            return;
        
        await localTabGroup.CloseAsync(localTabViewModel).ConfigureAwait(false);
    }

    private async Task HandleOnMouseDownAsync(ITab localTabViewModel, MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.Button == 0)
            _thinksLeftMouseButtonIsDown = true;
        if (mouseEventArgs.Button == 1)
            await CloseTabOnClickAsync(localTabViewModel).ConfigureAwait(false);
        else if (mouseEventArgs.Button == 2)
            ManuallyPropagateOnContextMenu(mouseEventArgs, localTabViewModel);
    }

    private void ManuallyPropagateOnContextMenu(
        MouseEventArgs mouseEventArgs,
        ITab tab)
    {
        var localHandleTabButtonOnContextMenu = _tabCascadingValueBatch.HandleTabButtonOnContextMenu;
        if (localHandleTabButtonOnContextMenu is null)
            return;

        _tabCascadingValueBatch.CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.Tab_ManuallyPropagateOnContextMenu,
            HandleTabButtonOnContextMenu = localHandleTabButtonOnContextMenu,
            TabContextMenuEventArgs = new TabContextMenuEventArgs(mouseEventArgs, tab, () => Task.CompletedTask),
        });
    }

    private void HandleOnMouseUp()
    {
        _thinksLeftMouseButtonIsDown = false;
    }
    
    private async Task HandleOnMouseOutAsync(ITab localTabViewModel, MouseEventArgs mouseEventArgs)
    {
        if ((mouseEventArgs.Buttons & 1) == 0)
            _thinksLeftMouseButtonIsDown = false;
    
        if (_thinksLeftMouseButtonIsDown && localTabViewModel is IDrag draggable)
        {
            _thinksLeftMouseButtonIsDown = false;
        
            // This needs to run synchronously to guarantee `dragState.DragElementDimensions` is in a threadsafe state
            // (keep any awaits after it).
            // (only the "UI thread" touches `dragState.DragElementDimensions`).
            var dragState = _tabCascadingValueBatch.CommonService.GetDragState();

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
        _tabCascadingValueBatch.CommonService.Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(true, null, draggable);
    }

    /// <summary>
    /// This method can only be invoked from the "UI thread" due to the shared `UiStringBuilder` usage.
    /// </summary>
    private string GetCssClass(ITabGroup localTabGroup, ITab localTabViewModel)
    {
        var uiStringBuilder = _tabCascadingValueBatch.CommonService.UiStringBuilder;
        
        uiStringBuilder.Clear();
        uiStringBuilder.Append("di_polymorphic-tab di_button di_unselectable ");
        uiStringBuilder.Append(GetIsActiveCssClass(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append(localTabGroup?.GetDynamicCss(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append("di_ide_panel-tab");
    
        return uiStringBuilder.ToString();
    }
    #endregion
    
    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
    }
}
