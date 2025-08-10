using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Settings.Displays;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;

using System.Text;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.AppDatas.Models;

// CompilerServiceRegistry.cs
using Walk.TextEditor.RazorLib.TextEditors.Models;

// DecorationMapperRegistry.cs
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.TextEditor.RazorLib.Options.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    [Inject]
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    
    private bool _previousDragStateWrapShouldDisplay;
    private ElementDimensions _bodyElementDimensions = new();
    private ElementDimensions _editorElementDimensions = new();

    private string UnselectableClassCss => _previousDragStateWrapShouldDisplay ? "di_unselectable" : string.Empty;
    
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
    
    private DotNetObjectReference<IdeMainLayout>? _dotNetHelper;
    
    private IDialog _dialogRecord = new DialogViewModel(
        Key<IDynamicViewModel>.NewKey(),
        "Settings",
        typeof(SettingsDisplay),
        null,
        null,
        true,
        null);
    
    private CtrlTabKind _ctrlTabKind = CtrlTabKind.Dialogs;
    
    private int _index;
    
    private bool _altIsDown;
    private bool _ctrlIsDown;
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    /// <summary>
    /// Only use this from the "UI thread".
    /// </summary>
    private readonly StringBuilder _styleBuilder = new();
    
    private readonly string _measureLineHeightElementId = "di_measure-lineHeight";
    
    /// <summary>The unit of measurement is Pixels (px)</summary>
    public const double OUTLINE_THICKNESS = 4;
    
    private string _lineHeightCssStyle;
    
    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
    
    public string TooltipRelativeX { get; set; } = string.Empty;
    public string TooltipRelativeY { get; set; } = string.Empty;
    
    private ITooltipModel? _tooltipModelPrevious = null;
    
    private string _measureCharacterWidthAndLineHeightElementId = "di_te_measure-charWidth-lineHeight";
    
    private string _wrapperCssClass;
    private string _wrapperCssStyle;
    
    private Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge _dirtyResourceUriBadge;
    private Walk.Common.RazorLib.Notifications.Models.NotificationBadge _notificationBadge;
    
    private bool _doTextEditorMeasure = true;
    private bool _doCommonMeasure = true;
    
    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
    
        InitializePanelTabs();
        HandleCompilerServicesAndDecorationMappers();
        
        ///////////
        
        MeasureLineHeight_UiRenderStep();
    
        DotNetService.CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.WalkCommonInitializerWork
        });

        NoiseyOnInitializedSteps();

        EnqueueOnInitializedSteps();

        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;

        DotNetService.CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
        DotNetService.IdeService.IdeStateChanged += OnIdeMainLayoutStateChanged;
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;
        
        DotNetService.TextEditorService.SecondaryChanged += OnNeedsMeasured;

        DotNetService.TextEditorService.Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeOnAfterRenderFirstRender();
        }
        
        var tooltipModel = DotNetService.CommonService.GetTooltipState().TooltipModel;
        if (tooltipModel is not null && !tooltipModel.WasRepositioned && _tooltipModelPrevious != tooltipModel)
        {
            await HandleTooltipRepositioning(tooltipModel);
        }
        
        if (_doTextEditorMeasure)
        {
            await HandleMeasureTextEditor();
        }
        
        if (_doCommonMeasure)
        {
            await HandleMeasureCommon();
        }
    }

    /// <summary>Needs UI thread to ensure the measured element is rendered.</summary>
    private async Task HandleMeasureCommon()
    {
        _doCommonMeasure = false;
        var lineHeight = await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeAsync<int>(
            "walkCommon.getLineHeightInPixelsById",
            _measureLineHeightElementId);
        Console.WriteLine($"lineHeight: {lineHeight}");
        
        DotNetService.CommonService.Options_SetLineHeight(lineHeight);
    }

    /// <summary>Needs UI thread to ensure the measured element is rendered.</summary>
    private async Task HandleMeasureTextEditor()
    {
        _doTextEditorMeasure = false;
        var charAndLineMeasurements = await DotNetService.TextEditorService.JsRuntimeTextEditorApi
                .GetCharAndLineMeasurementsInPixelsById(_measureCharacterWidthAndLineHeightElementId);
        Console.WriteLine($"{charAndLineMeasurements.CharacterWidth} {charAndLineMeasurements.LineHeight}");

        DotNetService.TextEditorService.Options_SetCharAndLineMeasurements(new(), charAndLineMeasurements);
    }

    private async Task HandleTooltipRepositioning(ITooltipModel tooltipModel)
    {
        _tooltipModelPrevious = tooltipModel;

        var tooltip_HtmlElementDimensions = await DotNetService.CommonService.JsRuntimeCommonApi.MeasureElementById(
            DotNetService.CommonService.Tooltip_HtmlElementId);
        var tooltip_GlobalHtmlElementDimensions = await DotNetService.CommonService.JsRuntimeCommonApi.MeasureElementById(
            CommonFacts.RootHtmlElementId);

        var xLarge = false;
        var yLarge = false;

        if (tooltipModel.X + tooltip_HtmlElementDimensions.WidthInPixels > tooltip_GlobalHtmlElementDimensions.WidthInPixels)
        {
            xLarge = true;
        }

        if (tooltipModel.Y + tooltip_HtmlElementDimensions.HeightInPixels > tooltip_GlobalHtmlElementDimensions.HeightInPixels)
        {
            yLarge = true;
        }

        tooltipModel.WasRepositioned = true;

        if (xLarge)
        {
            tooltipModel.X = tooltip_GlobalHtmlElementDimensions.WidthInPixels - tooltip_HtmlElementDimensions.WidthInPixels - 5;
            if (tooltipModel.X < 0)
                tooltipModel.X = 0;
        }

        if (yLarge)
        {
            tooltipModel.Y = tooltip_GlobalHtmlElementDimensions.HeightInPixels - tooltip_HtmlElementDimensions.HeightInPixels - 5;
            if (tooltipModel.Y < 0)
                tooltipModel.Y = 0;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void InitPanelGroup(PanelGroupParameter panelGroupParameter)
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

    private async void DragStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.AppOptionsStateChanged)
        {
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    
        if (commonUiEventKind != CommonUiEventKind.DragStateChanged)
            return;
            
        if (_previousDragStateWrapShouldDisplay != DotNetService.CommonService.GetDragState().ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = DotNetService.CommonService.GetDragState().ShouldDisplay;
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    }

    private async void OnIdeMainLayoutStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.Ide_IdeStateChanged)
        {
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
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
            uiStringBuilder.Append(UnselectableClassCss);
            uiStringBuilder.Append(" ");
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
    /* Start WalkConfigInitializer */
    
    public async Task SetFocus(string elementId)
    {
        await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
            "walkCommon.focusHtmlElementById",
            elementId,
            /*preventScroll:*/ true);
    }
    
    private async Task SetSolution(DotNetAppData dotNetAppData)
    {
        var solutionMostRecent = dotNetAppData?.SolutionMostRecent;
    
        if (solutionMostRecent is null)
            return;
    
        var slnAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            solutionMostRecent,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder());

        DotNetService.Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.SetDotNetSolution,
            DotNetSolutionAbsolutePath = slnAbsolutePath,
        });
    }
    /* End WalkConfigInitializer */
    
    #region WalkCommonInitializer
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        switch (commonUiEventKind)
        {
            case CommonUiEventKind.DialogStateChanged:
            case CommonUiEventKind.WidgetStateChanged:
            case CommonUiEventKind.NotificationStateChanged:
            case CommonUiEventKind.DropdownStateChanged:
            case CommonUiEventKind.OutlineStateChanged:
            case CommonUiEventKind.TooltipStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
            case CommonUiEventKind.LineHeightNeedsMeasured:
                await InvokeAsync(MeasureLineHeight_UiRenderStep);
                break;
            case CommonUiEventKind.PanelStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
        }
    }
    
    private void MeasureLineHeight_UiRenderStep()
    {
        _lineHeightCssStyle = $"{DotNetService.CommonService.Options_FontFamilyCssStyleString} {DotNetService.CommonService.Options_FontSizeCssStyleString}";
        StateHasChanged();
    }
    
    private Task WIDGET_RemoveWidget()
    {
        DotNetService.CommonService.SetWidget(null);
        return Task.CompletedTask;
    }
    
    private async Task DROPDOWN_ClearActiveKeyList()
    {
        var firstDropdown = DotNetService.CommonService.GetDropdownState().DropdownList.FirstOrDefault();
        
        if (firstDropdown is not null)
        {
            var restoreFocusOnCloseFunc = firstDropdown.RestoreFocusOnClose;
            
            if (restoreFocusOnCloseFunc is not null)
                await restoreFocusOnCloseFunc.Invoke();
        }
        
        DotNetService.CommonService.Dropdown_ReduceClearAction();
    }
    
    public string OUTLINE_GetStyleCssLeft(OutlineState localOutlineState)
    {
        var width = OUTLINE_THICKNESS;
        
        var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
    
        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
        
        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
        
        _styleBuilder.Clear();
        
        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append($"left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");
        
        return _styleBuilder.ToString();
    }
    
    public string OUTLINE_GetStyleCssRight(OutlineState localOutlineState)
    {
        var width = OUTLINE_THICKNESS;
        
        var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
    
        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels +
            localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels -
            OUTLINE_THICKNESS;
        
        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
            
        _styleBuilder.Clear();
        
        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");
        
        return _styleBuilder.ToString();
    }
    
    public string OUTLINE_GetStyleCssTop(OutlineState localOutlineState)
    {
        var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
        
        var height = OUTLINE_THICKNESS;
    
        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
        
        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
        
        _styleBuilder.Clear();
        
        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");
        
        return _styleBuilder.ToString();
    }
    
    public string OUTLINE_GetStyleCssBottom(OutlineState localOutlineState)
    {
        var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
        
        var height = OUTLINE_THICKNESS;
    
        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
        
        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels +
            localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels -
            OUTLINE_THICKNESS;
            
        _styleBuilder.Clear();
        
        _styleBuilder.Append($"width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append($"height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append($"left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");
        
        _styleBuilder.Append($"top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");
        
        return _styleBuilder.ToString();
    }
    #endregion
    
    #region StartWalkTextEditorInitializer
    
    /// <summary>
    /// Only invoke this method from the UI thread due to the usage of the shared UiStringBuilder.
    /// </summary>
    private async Task Ready()
    {
        _doTextEditorMeasure = true;
    
        DotNetService.CommonService.UiStringBuilder.Clear();
        DotNetService.CommonService.UiStringBuilder.Append("di_te_text-editor-css-wrapper ");
        DotNetService.CommonService.UiStringBuilder.Append(DotNetService.TextEditorService.ThemeCssClassString);
        _wrapperCssClass = DotNetService.CommonService.UiStringBuilder.ToString();
        
        var options = DotNetService.TextEditorService.Options_GetTextEditorOptionsState().Options;
        
        var fontSizeInPixels = TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS;
        if (options.CommonOptions?.FontSizeInPixels is not null)
            fontSizeInPixels = options!.CommonOptions.FontSizeInPixels;
        DotNetService.CommonService.UiStringBuilder.Clear();
        DotNetService.CommonService.UiStringBuilder.Append("font-size: ");
        DotNetService.CommonService.UiStringBuilder.Append(fontSizeInPixels.ToCssValue());
        DotNetService.CommonService.UiStringBuilder.Append("px;");
        var fontSizeCssStyle = DotNetService.CommonService.UiStringBuilder.ToString();
        
        var fontFamily = TextEditorVirtualizationResult.DEFAULT_FONT_FAMILY;
        if (!string.IsNullOrWhiteSpace(options?.CommonOptions?.FontFamily))
            fontFamily = options!.CommonOptions!.FontFamily;
        DotNetService.CommonService.UiStringBuilder.Clear();
        DotNetService.CommonService.UiStringBuilder.Append("font-family: ");
        DotNetService.CommonService.UiStringBuilder.Append(fontFamily);
        DotNetService.CommonService.UiStringBuilder.Append(";");
        var fontFamilyCssStyle = DotNetService.CommonService.UiStringBuilder.ToString();
        
        DotNetService.CommonService.UiStringBuilder.Clear();
        DotNetService.CommonService.UiStringBuilder.Append(fontSizeCssStyle);
        DotNetService.CommonService.UiStringBuilder.Append(" ");
        DotNetService.CommonService.UiStringBuilder.Append(fontFamilyCssStyle);
        DotNetService.CommonService.UiStringBuilder.Append(" position:absolute;");
        _wrapperCssStyle = DotNetService.CommonService.UiStringBuilder.ToString();
        
        // I said "Only invoke this method from the UI thread due to the usage of the shared UiStringBuilder."
        // But I'm still going to keep this InvokeAsync for the StateHasChanged due to superstituous anxiety.
        await InvokeAsync(StateHasChanged);
    }
    
    private async void OnNeedsMeasured(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.NeedsMeasured)
        {
            await InvokeAsync(Ready);
        }
    }
    
    #endregion
    private TabCascadingValueBatch _tabCascadingValueBatch = new();

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
        uiStringBuilder.Append("di_dynamic-tab di_button di_unselectable ");
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
        DotNetService.CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnIdeMainLayoutStateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= OnNeedsMeasured;
        
        _dotNetHelper?.Dispose();
        
        BrowserResizeInterop.DisposeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
        
        _workerCancellationTokenSource.Cancel();
        _workerCancellationTokenSource.Dispose();
        
        DotNetService.CommonService.Continuous_StartAsyncTask = null;
        DotNetService.CommonService.Indefinite_StartAsyncTask = null;
        
        DotNetService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        
        var notificationState = DotNetService.CommonService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            DotNetService.CommonService.Notification_ReduceDisposeAction(notification.DynamicViewModelKey);
        }
    }
}
