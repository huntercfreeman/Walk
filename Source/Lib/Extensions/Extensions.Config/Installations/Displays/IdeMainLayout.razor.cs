using System.Text;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Settings.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.AppDatas.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    [Inject]
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    
    private ElementDimensions _bodyElementDimensions = new();
    private ElementDimensions _editorElementDimensions = new();
    
    // NOTE TO SELF: Don't put an event for Drag that makes the website unselectable,...
    // ...just ensure the drag start target is unselectable.
    // I'm pretty sure this works. If it doesn't make sure it isn't cause you're
    // doing drag start on a selectable element that then propagates the drag start event?
    
    /// <summary>
    /// This can only be set from the "UI thread".
    /// </summary>
    private bool _shouldRecalculateCssStrings = true;
    
    private string _classCssString;
    private string _styleCssString;
    private string _headerCssStyle;

    private PanelGroupParameter _leftPanelGroupParameter;
    private PanelGroupParameter _rightPanelGroupParameter;
    private PanelGroupParameter _bottomPanelGroupParameter;
    
    private ResizableColumnParameter _topLeftResizableColumnParameter;
    private ResizableColumnParameter _topRightResizableColumnParameter;
    
    private ResizableRowParameter _resizableRowParameter;
    
    private TabCascadingValueBatch _tabCascadingValueBatch = new();
    
    private IDialog _dialogRecord = new DialogViewModel(
        Key<IDynamicViewModel>.NewKey(),
        "Settings",
        typeof(SettingsDisplay),
        null,
        null,
        true,
        null);
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    private Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge _dirtyResourceUriBadge;
    private Walk.Common.RazorLib.Notifications.Models.NotificationBadge _notificationBadge;

    protected override void OnInitialized()
    {
        _dirtyResourceUriBadge = new Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge(DotNetService.TextEditorService);
        _notificationBadge = new Walk.Common.RazorLib.Notifications.Models.NotificationBadge(DotNetService.CommonService);
    
        InitializationHelper.InitializePanelTabs(DotNetService);
        InitializationHelper.HandleCompilerServicesAndDecorationMappers(DotNetService);

        DotNetService.TextEditorService.IdeBackgroundTaskApi = DotNetService.IdeService;

        var panelState = DotNetService.CommonService.GetPanelState();

        _topLeftResizableColumnParameter = new(
            panelState.TopLeftPanelGroup.ElementDimensions,
            _editorElementDimensions,
            () => InvokeAsync(StateHasChanged));

        _topRightResizableColumnParameter = new(
            _editorElementDimensions,
            panelState.TopRightPanelGroup.ElementDimensions,
            () => InvokeAsync(StateHasChanged));

        _resizableRowParameter = new(
            _bodyElementDimensions,
            panelState.BottomPanelGroup.ElementDimensions,
            () => InvokeAsync(StateHasChanged));

        _leftPanelGroupParameter = new(
            panelGroupKey: CommonFacts.LeftPanelGroupKey,
            adjacentElementDimensions: _editorElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Width,
            cssClassString: null);

        _rightPanelGroupParameter = new(
            panelGroupKey: CommonFacts.RightPanelGroupKey,
            adjacentElementDimensions: _editorElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Width,
            cssClassString: null);

        _bottomPanelGroupParameter = new(
            panelGroupKey: CommonFacts.BottomPanelGroupKey,
            cssClassString: "di_ide_footer",
            adjacentElementDimensions: _bodyElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Height);

        _bodyElementDimensions.HeightDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(78, DimensionUnitKind.Percentage),
            new DimensionUnit(
                DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract),
            new DimensionUnit(
                CommonFacts.Ide_Header_Height.Value / 2,
                CommonFacts.Ide_Header_Height.DimensionUnitKind,
                DimensionOperatorKind.Subtract)
        });

        _editorElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(
                33.3333,
                DimensionUnitKind.Percentage),
            new DimensionUnit(
                DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract)
        });

        InitPanelGroup(DotNetService, _leftPanelGroupParameter);
        InitPanelGroup(DotNetService, _rightPanelGroupParameter);
        InitPanelGroup(DotNetService, _bottomPanelGroupParameter);

        InitializationHelper.EnqueueOnInitializedSteps(DotNetService);

        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializationHelper.InitializeOnAfterRenderFirstRender(DotNetService, BrowserResizeInterop, _workerCancellationTokenSource);
        }
    }
    
    private void InitPanelGroup(DotNetService DotNetService, PanelGroupParameter panelGroupParameter)
    {
        var position = string.Empty;

        if (CommonFacts.LeftPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            position = "left";
            panelGroupParameter.DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_left;
        }
        else if (CommonFacts.RightPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            position = "right";
            panelGroupParameter.DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_right;
        }
        else if (CommonFacts.BottomPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            position = "bottom";
            panelGroupParameter.DimensionUnitPurposeKind = DimensionUnitPurposeKind.take_size_of_adjacent_hidden_panel_bottom;
        }

        panelGroupParameter.PanelPositionCss = $"di_ide_panel_{position}";

        panelGroupParameter.HtmlIdTabs = panelGroupParameter.PanelPositionCss + "_tabs";

        if (CommonFacts.LeftPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            _leftPanelGroupParameter = panelGroupParameter;
        }
        else if (CommonFacts.RightPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            _rightPanelGroupParameter = panelGroupParameter;
        }
        else if (CommonFacts.BottomPanelGroupKey == panelGroupParameter.PanelGroupKey)
        {
            _bottomPanelGroupParameter = panelGroupParameter;
        }
    }
    
    /// <summary>
    /// This can only be invoked from the UI thread due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private void CreateCssStrings()
    {
        if (_shouldRecalculateCssStrings)
        {
            _shouldRecalculateCssStrings = false;
        
            var uiStringBuilder = DotNetService.CommonService.UiStringBuilder;
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append("di_main-layout ");
            uiStringBuilder.Append(DotNetService.CommonService.Options_ThemeCssClassString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(DotNetService.TextEditorService.ThemeCssClassString);
            _classCssString = uiStringBuilder.ToString();
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append(DotNetService.CommonService.Options_FontSizeCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(DotNetService.CommonService.Options_FontFamilyCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(DotNetService.CommonService.Options_ColorSchemeCssStyleString);
            _styleCssString = uiStringBuilder.ToString();
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append("display: flex; justify-content: space-between; border-bottom: ");
            uiStringBuilder.Append(DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels);
            uiStringBuilder.Append("px solid var(--di_primary-border-color);");
            _headerCssStyle = uiStringBuilder.ToString();
        }
    }
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        switch (commonUiEventKind)
        {
            case CommonUiEventKind.PanelStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
        }
    }

    private async void AppOptionsOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.AppOptionsStateChanged)
        {
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    }

    private async void TextEditorOptionsStateWrap_StateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.StaticStateChanged)
        {
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
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

    private void PassAlongSizeIfNoActiveTab(PanelGroupParameter panelGroupParameter, PanelGroup panelGroup)
    {
        DimensionAttribute adjacentElementSizeDimensionAttribute;
        DimensionAttribute panelGroupSizeDimensionsAttribute;

        switch (panelGroupParameter.DimensionAttributeKind)
        {
            case DimensionAttributeKind.Width:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.WidthDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.WidthDimensionAttribute;
                break;
            case DimensionAttributeKind.Height:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.HeightDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.HeightDimensionAttribute;
                break;
            case DimensionAttributeKind.Left:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.LeftDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.LeftDimensionAttribute;
                break;
            case DimensionAttributeKind.Right:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.RightDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.RightDimensionAttribute;
                break;
            case DimensionAttributeKind.Top:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.TopDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.TopDimensionAttribute;
                break;
            case DimensionAttributeKind.Bottom:
                adjacentElementSizeDimensionAttribute = panelGroupParameter.AdjacentElementDimensions.BottomDimensionAttribute;
                panelGroupSizeDimensionsAttribute = panelGroup.ElementDimensions.BottomDimensionAttribute;
                break;
            default:
                return;
        }

        var indexOfPreviousPassAlong = adjacentElementSizeDimensionAttribute.DimensionUnitList.FindIndex(
            x => x.Purpose == panelGroupParameter.DimensionUnitPurposeKind);

        if (panelGroup.ActiveTab is null && indexOfPreviousPassAlong == -1)
        {
            var panelGroupPercentageSize = panelGroupSizeDimensionsAttribute.DimensionUnitList.First(
                x => x.DimensionUnitKind == DimensionUnitKind.Percentage);

            adjacentElementSizeDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
                panelGroupPercentageSize.Value,
                panelGroupPercentageSize.DimensionUnitKind,
                DimensionOperatorKind.Add,
                panelGroupParameter.DimensionUnitPurposeKind));
        }
        else if (panelGroup.ActiveTab is not null && indexOfPreviousPassAlong != -1)
        {
            adjacentElementSizeDimensionAttribute.DimensionUnitList.RemoveAt(indexOfPreviousPassAlong);
        }
    }

    private string GetElementDimensionsStyleString(PanelGroup? panelGroup)
    {
        if (panelGroup?.ActiveTab is null)
        {
            return "calc(" +
                   "var(--di_panel-tabs-font-size)" +
                   " + var(--di_panel-tabs-margin)" +
                   " + var(--di_panel-tabs-bug-are-not-aligning-need-to-fix-todo))";
        }

        return panelGroup?.ElementDimensions.GetStyleString(DotNetService.CommonService.UiStringBuilder) ?? string.Empty;
    }

    private Task TopDropzoneOnMouseUp(Key<PanelGroup> panelGroupKey, MouseEventArgs mouseEventArgs)
    {
        var panelState = DotNetService.CommonService.GetPanelState();

        PanelGroup panelGroup;

        if (panelGroupKey == panelState.TopLeftPanelGroup.Key)
        {
            panelGroup = panelState.TopLeftPanelGroup;
        }
        else if (panelGroupKey == panelState.TopRightPanelGroup.Key)
        {
            panelGroup = panelState.TopRightPanelGroup;
        }
        else if (panelGroupKey == panelState.BottomPanelGroup.Key)
        {
            panelGroup = panelState.BottomPanelGroup;
        }
        else
        {
            return Task.CompletedTask;
        }

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            DotNetService.CommonService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            DotNetService.CommonService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                true);

            DotNetService.CommonService.Panel_SetDragEventArgs(null);

            DotNetService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }

    private Task BottomDropzoneOnMouseUp(Key<PanelGroup> panelGroupKey, MouseEventArgs mouseEventArgs)
    {
        var panelState = DotNetService.CommonService.GetPanelState();

        PanelGroup panelGroup;

        if (panelGroupKey == panelState.TopLeftPanelGroup.Key)
        {
            panelGroup = panelState.TopLeftPanelGroup;
        }
        else if (panelGroupKey == panelState.TopRightPanelGroup.Key)
        {
            panelGroup = panelState.TopRightPanelGroup;
        }
        else if (panelGroupKey == panelState.BottomPanelGroup.Key)
        {
            panelGroup = panelState.BottomPanelGroup;
        }
        else
        {
            return Task.CompletedTask;
        }

        if (panelGroup is null)
            return Task.CompletedTask;

        var panelDragEventArgs = panelState.DragEventArgs;

        if (panelDragEventArgs is not null)
        {
            DotNetService.CommonService.DisposePanelTab(
                panelDragEventArgs.Value.PanelGroup.Key,
                panelDragEventArgs.Value.PanelTab.Key);

            DotNetService.CommonService.RegisterPanelTab(
                panelGroup.Key,
                panelDragEventArgs.Value.PanelTab,
                false);

            DotNetService.CommonService.Panel_SetDragEventArgs(null);

            DotNetService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(false, null);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetPanelElementCssClass(string panelPositionCss, string cssClassString)
    {
        DotNetService.CommonService.UiStringBuilder.Clear();
        DotNetService.CommonService.UiStringBuilder.Append("di_ide_panel ");
        DotNetService.CommonService.UiStringBuilder.Append(panelPositionCss);
        DotNetService.CommonService.UiStringBuilder.Append(" ");
        DotNetService.CommonService.UiStringBuilder.Append(cssClassString);

        return DotNetService.CommonService.UiStringBuilder.ToString();
    }

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
        uiStringBuilder.Append("di_dynamic-tab di_button di_unselectable ");
        uiStringBuilder.Append(GetIsActiveCssClass(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append(localTabGroup?.GetDynamicCss(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append("di_ide_panel-tab");

        return uiStringBuilder.ToString();
    }
    
    private Task RenderFileDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonFileId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyFile,
            DotNetService.IdeService.GetIdeState().MenuFile,
            IdeState.ButtonFileId,
            preventScroll: false);
    }
    
    private Task RenderToolsDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonToolsId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyTools,
            DotNetService.IdeService.GetIdeState().MenuTools,
            IdeState.ButtonToolsId,
            preventScroll: false);
    }
    
    private Task RenderViewDropdownOnClick()
    {
        InitializationHelper.InitializeMenuView(DotNetService);
    
        return DropdownHelper.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonViewId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyView,
            DotNetService.IdeService.GetIdeState().MenuView,
            IdeState.ButtonViewId,
            preventScroll: false);
    }
    
    private Task RenderRunDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonRunId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyRun,
            DotNetService.IdeService.GetIdeState().MenuRun,
            IdeState.ButtonRunId,
            preventScroll: false);
    }
    
    private Task OpenInfoDialogOnClick()
    {
        var dialogRecord = new DialogViewModel(
            InitializationHelper._infoDialogKey,
            "Info",
            typeof(IdeInfoDisplay),
            null,
            null,
            true,
            null);
    
        DotNetService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        
        DotNetService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
        
        BrowserResizeInterop.DisposeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
        
        _workerCancellationTokenSource.Cancel();
        _workerCancellationTokenSource.Dispose();
        
        DotNetService.CommonService.Continuous_StartAsyncTask = null;
        DotNetService.CommonService.Indefinite_StartAsyncTask = null;
    }
}
