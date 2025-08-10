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
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Settings.Displays;
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

        NoiseyOnInitializedSteps();

        EnqueueOnInitializedSteps();

        SubscribeEvents();

        MeasureLineHeight_UiRenderStep();
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
    
    #region WalkCommonInitializer
    private void MeasureLineHeight_UiRenderStep()
    {
        _lineHeightCssStyle = $"{DotNetService.CommonService.Options_FontFamilyCssStyleString} {DotNetService.CommonService.Options_FontSizeCssStyleString}";
        _doCommonMeasure = true;
        StateHasChanged();
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
    #endregion
    
    
    public void Dispose()
    {
        DisposeEvents();

        _dotNetHelper?.Dispose();
        
        BrowserResizeInterop.DisposeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
        
        _workerCancellationTokenSource.Cancel();
        _workerCancellationTokenSource.Dispose();
        
        DotNetService.CommonService.Continuous_StartAsyncTask = null;
        DotNetService.CommonService.Indefinite_StartAsyncTask = null;
    }
}
