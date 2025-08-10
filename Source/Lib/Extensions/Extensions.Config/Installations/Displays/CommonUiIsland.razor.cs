using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.Extensions.DotNet;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class CommonUiIsland : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private CtrlTabKind _ctrlTabKind = CtrlTabKind.Dialogs;
    
    private int _index;
    
    private bool _altIsDown;
    private bool _ctrlIsDown;

    private int _countEventReceived;
    private int _countEventHandled;
    
    private bool _doTextEditorMeasure = true;
    private bool _doCommonMeasure = true;

    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
    
    public string TooltipRelativeX { get; set; } = string.Empty;
    public string TooltipRelativeY { get; set; } = string.Empty;
    
    private ITooltipModel? _tooltipModelPrevious = null;
    
    private readonly string _measureLineHeightElementId = "di_measure-lineHeight";
    
    /// <summary>The unit of measurement is Pixels (px)</summary>
    public const double OUTLINE_THICKNESS = 4;
    
    private string _lineHeightCssStyle;
    
    private string _measureCharacterWidthAndLineHeightElementId = "di_te_measure-charWidth-lineHeight";
    
    private string _wrapperCssClass;
    private string _wrapperCssStyle;
    
    private DotNetObjectReference<CommonUiIsland>? _dotNetHelper;
    
    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
        
        MeasureLineHeight_UiRenderStep();
        
        DotNetService.TextEditorService.SecondaryChanged += OnNeedsMeasured;
        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
    }
    
    /// <summary>TODO: Thread safety</summary>
    protected override bool ShouldRender()
    {    
        // This is expected to cause "eventual thread safety" due to perspective.
        //
        // The '_countEventHandled < _countEventReceived' when true is guaranteed to continue being true
        // until the increment to '_countEventHandled'.
        //
        // The increment to '_countEventHandled' occurs inside this if statement.
        // 
        // The closest failure would be the event firing and incrementing '_countEventReceived'
        // without having fully executed various required code,
        // and then a cascading render from the parent causes the increment to '_countEventHandled'.
        //
        // This is avoided by incrementing '_countEventReceived' from within 'InvokeAsync(...)'
        // just prior to the inner 'StateHasChanged();'.
        //
        // The ints going from int.MaxValue to int.MinValue without an exception messes this logic up
        // (I can't think of the name for this so I described it).
        //
        // Probably have to put any incrementations "in an if statement" to reset to 0 if you're going to wrap around to the negative values.
        // 
        if (_countEventHandled < _countEventReceived)
        {
            if (_countEventHandled == int.MaxValue)
            {
                _countEventHandled = 0;
            }
            else
            {
                ++_countEventHandled;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    
    protected async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (DotNetService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            {
                // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
                // before you finish setting up the events?
                // (is this a thing, I'm just presuming this would be true).
                await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                    "walkConfig.appWideKeyboardEventsInitialize",
                    _dotNetHelper);
            }
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
    
    private async Task IslandStateHasChanged()
    {
        await InvokeAsync(() =>
        {
            if (_countEventReceived == int.MaxValue)
            {
                _countEventReceived = 0;
            }
            else
            {
                ++_countEventReceived;
            }
            StateHasChanged();
        });
    }
    
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
        }
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

    [JSInvokable]
    public async Task ReceiveOnKeyDown(string key, bool shiftKey)
    {
        if (key == "Alt")
        {
            _altIsDown = true;
            StateHasChanged();
        }
        else if (key == "Control")
        {
            _ctrlIsDown = true;
            StateHasChanged();
        }
        else if (key == "Tab")
        {
            _ctrlIsDown = true;
            if (!shiftKey)
            {
                if (_ctrlTabKind == CtrlTabKind.Dialogs)
                {
                    if (_index >= DotNetService.CommonService.GetDialogState().DialogList.Count - 1)
                    {
                        var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                        if ((textEditorGroup?.ViewModelKeyList.Count ?? 0) > 0)
                        {
                            _ctrlTabKind = CtrlTabKind.TextEditors;
                            _index = 0;
                        }
                        else
                        {
                            _index = 0;
                        }
                    }
                    else
                    {
                        _index++;
                    }
                }
                else if (_ctrlTabKind == CtrlTabKind.TextEditors)
                {
                    var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                    if (_index >= textEditorGroup.ViewModelKeyList.Count - 1)
                    {
                        if (DotNetService.CommonService.GetDialogState().DialogList.Count > 0)
                        {
                            _ctrlTabKind = CtrlTabKind.Dialogs;
                            _index = 0;
                        }
                        else
                        {
                            _index = 0;
                        }
                    }
                    else
                    {
                        _index++;
                    }
                }
            }
            else
            {
                if (_ctrlTabKind == CtrlTabKind.Dialogs)
                {
                    if (_index <= 0)
                    {
                        var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                        if (textEditorGroup.ViewModelKeyList.Count >= 0)
                        {
                            _ctrlTabKind = CtrlTabKind.TextEditors;
                            _index = textEditorGroup.ViewModelKeyList.Count - 1;
                        }
                        else
                        {
                            _index = 0;
                        }
                    }
                    else
                    {
                        _index--;
                    }
                }
                else if (_ctrlTabKind == CtrlTabKind.TextEditors)
                {
                    var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                    if (_index <= 0)
                    {
                        if (DotNetService.CommonService.GetDialogState().DialogList.Count > 0)
                        {
                            _ctrlTabKind = CtrlTabKind.Dialogs;
                            _index = DotNetService.CommonService.GetDialogState().DialogList.Count - 1;
                        }
                        else
                        {
                            _index = 0;
                        }
                    }
                    else
                    {
                        _index--;
                    }
                }
            }

            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task ReceiveOnKeyUp(string key)
    {
        if (key == "Alt")
        {
            _altIsDown = false;
            StateHasChanged();
        }
        else if (key == "Control")
        {
            _ctrlIsDown = false;

            if (_ctrlTabKind == CtrlTabKind.Dialogs)
            {
                var dialogState = DotNetService.CommonService.GetDialogState();
                if (_index < dialogState.DialogList.Count)
                {
                    var dialog = dialogState.DialogList[_index];
                    await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                        "walkCommon.focusHtmlElementById",
                        dialog.DialogFocusPointHtmlElementId,
                        /*preventScroll:*/ true);
                }
            }
            else if (_ctrlTabKind == CtrlTabKind.TextEditors)
            {
                var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                if (_index < textEditorGroup.ViewModelKeyList.Count)
                {
                    var viewModelKey = textEditorGroup.ViewModelKeyList[_index];
                    DotNetService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
                    {
                        var activeViewModel = editContext.GetViewModelModifier(textEditorGroup.ActiveViewModelKey);
                        await activeViewModel.FocusAsync();
                        DotNetService.TextEditorService.Group_SetActiveViewModel(
                            Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey,
                            viewModelKey);
                    });
                }
            }

            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task OnWindowBlur()
    {
        _altIsDown = false;
        _ctrlIsDown = false;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task ReceiveWidgetOnKeyDown()
    {
        /*DotNetService.CommonService.SetWidget(new Walk.Common.RazorLib.Widgets.Models.WidgetModel(
            typeof(Walk.Ide.RazorLib.CommandBars.Displays.CommandBarDisplay),
            componentParameterMap: null,
            cssClass: null,
            cssStyle: "width: 80vw; height: 5em; left: 10vw; top: 0;"));*/
    }

    /// <summary>
    /// TODO: This triggers when you save with 'Ctrl + s' in the text editor itself...
    /// ...the redundant save doesn't go through though since this app wide save will check the DirtyResourceUriState.
    /// </summary>
    [JSInvokable]
    public async Task SaveFileOnKeyDown()
    {
        TextEditorCommandDefaultFunctions.TriggerSave_NoTextEditorFocus(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
    }

    [JSInvokable]
    public async Task SaveAllFileOnKeyDown()
    {
        TextEditorCommandDefaultFunctions.TriggerSaveAll(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
    }

    [JSInvokable]
    public async Task FindAllOnKeyDown()
    {
        DotNetService.TextEditorService.Options_ShowFindAllDialog();
    }

    [JSInvokable]
    public async Task CodeSearchOnKeyDown()
    {
        DotNetService.CommonService.Dialog_ReduceRegisterAction(DotNetService.IdeService.CodeSearchDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Code Search",
            typeof(Walk.Ide.RazorLib.CodeSearches.Displays.CodeSearchDisplay),
            null,
            null,
            true,
            null));
    }

    [JSInvokable]
    public async Task EscapeOnKeyDown()
    {
        var textEditorGroup = DotNetService.TextEditorService.Group_GetOrDefault(Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
        var viewModelKey = textEditorGroup.ActiveViewModelKey;

        DotNetService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var viewModel = editContext.GetViewModelModifier(viewModelKey);
            var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
            viewModel.FocusAsync();
            return ValueTask.CompletedTask;
        });
    }
    
    private async void OnNeedsMeasured(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.NeedsMeasured)
        {
            await InvokeAsync(Ready);
        }
    }
    
    private void MeasureLineHeight_UiRenderStep()
    {
        _lineHeightCssStyle = $"{DotNetService.CommonService.Options_FontFamilyCssStyleString} {DotNetService.CommonService.Options_FontSizeCssStyleString}";
        _doCommonMeasure = true;
        StateHasChanged();
    }
    
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
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
        
        DotNetService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= OnNeedsMeasured;
    }
}
