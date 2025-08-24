using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Tabs.Displays;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Settings.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Extensions.DotNet;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    [Inject]
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    
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
    
    private TabCascadingValueBatch _tabCascadingValueBatch;
    
    private static readonly Key<IDynamicViewModel> _settingsDialogKey = Key<IDynamicViewModel>.NewKey();
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;
    
    private MainLayoutDragEventKind _mainLayoutDragEventKind;
    
    private bool _userInterfaceSawIsExecuting;
    
    private const string _startButtonElementId = "di_startup-controls_id";

    private Key<DropdownRecord> _startButtonDropdownKey = Key<DropdownRecord>.NewKey();

    private Key<IDynamicViewModel> _dynamicViewModelKeyPrevious;

    private string GetIsActiveCssClass(ITab localTabViewModel) => (localTabViewModel.TabGroup?.GetIsActive(localTabViewModel) ?? false)
        ? "di_active"
        : string.Empty;

    public string? SelectedStartupControlAbsolutePathValue
    {
        get
        {
            return DotNetService.IdeService.GetIdeStartupControlState().ActiveStartupProjectAbsolutePathValue;
        }
        set
        {
            DotNetService.IdeService.Ide_SetActiveStartupControlKey(value);
        }
    }
    
    protected override void OnInitialized()
    {
        _tabCascadingValueBatch = new()
        {
            SubscribeToDragEventForScrolling = SubscribeToDragEventForScrolling,
        };
    
        InitializationHelper.InitializePanelTabs(DotNetService);
        InitializationHelper.HandleCompilerServicesAndDecorationMappers(DotNetService);

        DotNetService.TextEditorService.IdeBackgroundTaskApi = DotNetService.IdeService;

        var panelState = DotNetService.CommonService.GetPanelState();
        
        _leftPanelGroupParameter = new(
            panelGroupKey: CommonFacts.LeftPanelGroupKey,
            cssClassString: null)
        {
            PanelPositionCss = "di_ide_panel_left",
            HtmlIdTabs = "di_ide_panel_left_tabs"
        };

        _rightPanelGroupParameter = new(
            panelGroupKey: CommonFacts.RightPanelGroupKey,
            cssClassString: null)
        {
            PanelPositionCss = "di_ide_panel_right",
            HtmlIdTabs = "di_ide_panel_right_tabs"
        };

        _bottomPanelGroupParameter = new(
            panelGroupKey: CommonFacts.BottomPanelGroupKey,
            cssClassString: "di_ide_footer")
        {
            PanelPositionCss = "di_ide_panel_bottom",
            HtmlIdTabs = "di_ide_panel_bottom_tabs"
        };
        
        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;
        DotNetService.IdeService.IdeStateChanged += OnIdeStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializationHelper.InitializeOnAfterRenderFirstRender(DotNetService, BrowserResizeInterop, _workerCancellationTokenSource);
            DotNetService.CommonService.Panel_OnUserAgent_AppDimensionStateChanged();
            await InvokeAsync(StateHasChanged);
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
            uiStringBuilder.Append("height: ");
            uiStringBuilder.Append(DotNetService.CommonService.Options_LineHeight * 2);
            uiStringBuilder.Append("px;");
            _headerCssStyle = uiStringBuilder.ToString();
        }
    }
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        switch (commonUiEventKind)
        {
            case CommonUiEventKind.AppOptionsStateChanged:
                await InvokeAsync(() =>
                {
                    _shouldRecalculateCssStrings = true;
                    StateHasChanged();
                }).ConfigureAwait(false);
                break;
            case CommonUiEventKind.UserAgent_AppDimensionStateChanged:
                DotNetService.CommonService.Panel_OnUserAgent_AppDimensionStateChanged();
                await InvokeAsync(StateHasChanged);
                break;
            case CommonUiEventKind.PanelStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
            case CommonUiEventKind.DragStateChanged:
                if (_dragEventHandler is not null)
                {
                    if (_mainLayoutDragEventKind == MainLayoutDragEventKind.TopLeftResizeColumn)
                    {
                        await Walk.Common.RazorLib.Resizes.Models.ResizableColumn.Do(
                            DotNetService.CommonService,
                            leftElementDimensions: null,
                            rightElementDimensions: null,
                            _dragEventHandler,
                            _previousDragMouseEventArgs,
                            x => _dragEventHandler = x,
                            x => _previousDragMouseEventArgs = x);
                    }
                    else if (_mainLayoutDragEventKind == MainLayoutDragEventKind.TopRightResizeColumn)
                    {
                        await Walk.Common.RazorLib.Resizes.Models.ResizableColumn.Do(
                            DotNetService.CommonService,
                            leftElementDimensions: null,
                            rightElementDimensions: null,
                            _dragEventHandler,
                            _previousDragMouseEventArgs,
                            x => _dragEventHandler = x,
                            x => _previousDragMouseEventArgs = x);
                    }
                    else if (_mainLayoutDragEventKind == MainLayoutDragEventKind.BottomResizeRow)
                    {
                        await Walk.Common.RazorLib.Resizes.Models.ResizableRow.Do(
                            DotNetService.CommonService,
                            topElementDimensions: null,
                            bottomElementDimensions: null,
                            _dragEventHandler,
                            _previousDragMouseEventArgs,
                            x => _dragEventHandler = x,
                            x => _previousDragMouseEventArgs = x);
                    }
                    
                    await InvokeAsync(StateHasChanged);
                }
                break;
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

    private string GetElementDimensionsStyleString(PanelGroup? panelGroup)
    {
        if (panelGroup?.ActiveTab is null)
        {
            return DotNetService.CommonService.Rotate_Options_LineHeight_CssStyle;
        }
    
        return panelGroup?.ElementDimensions.GetStyleString(DotNetService.CommonService.UiStringBuilder) ?? string.Empty;
    }
    
    private string GetTopPanelBodyStyle(PanelGroup? panelGroup)
    {
        if (panelGroup?.ActiveTab is null)
        {
            return "width: 0;";
        }
        
        return DotNetService.CommonService.TopPanel_Body_Options_LineHeight_CssStyle;
    }
    
    private string GetBottomPanelBodyStyle(PanelGroup? panelGroup)
    {
        if (panelGroup?.ActiveTab is null)
        {
            return "width: 0;";
        }
        
        return DotNetService.CommonService.BottomPanel_Body_Options_LineHeight_CssStyle;
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

    private void HandleOnMouseUp()
    {
        _tabCascadingValueBatch.ThinksLeftMouseButtonIsDown = false;
    }

    public void SubscribeToDragEventForScrolling(IDrag draggable)
    {
        DotNetService.CommonService.Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(true, null, draggable);
    }

    /// <summary>
    /// This method can only be invoked from the "UI thread" due to the shared `UiStringBuilder` usage.
    /// </summary>
    private string GetCssClass(ITab localTabViewModel)
    {
        var uiStringBuilder = DotNetService.CommonService.UiStringBuilder;

        uiStringBuilder.Clear();
        uiStringBuilder.Append("di_dynamic-tab di_button di_unselectable ");
        uiStringBuilder.Append(GetIsActiveCssClass(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append(localTabViewModel.TabGroup?.GetDynamicCss(localTabViewModel));
        uiStringBuilder.Append(" ");
        uiStringBuilder.Append("di_ide_panel-tab");

        return uiStringBuilder.ToString();
    }
    
    private Task RenderFileDropdownOnClick()
    {
        return CommonFacts.RenderDropdownAsync(
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
        return CommonFacts.RenderDropdownAsync(
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
    
        return CommonFacts.RenderDropdownAsync(
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
        return CommonFacts.RenderDropdownAsync(
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
    
    public void SubscribeToDragEvent(MainLayoutDragEventKind mainLayoutDragEventKind)
    {
        DotNetService.CommonService.MainLayoutDragEventKind = mainLayoutDragEventKind;
        _dragEventHandler = DotNetService.CommonService.DragEventHandlerResizeHandleAsync;
        
        DotNetService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }
    
    public void SubscribeToDragEventTopLeft()
    {
        SubscribeToDragEvent(MainLayoutDragEventKind.TopLeftResizeColumn);
    }
    
    public void SubscribeToDragEventTopRight()
    {
        SubscribeToDragEvent(MainLayoutDragEventKind.TopRightResizeColumn);
    }
    
    public void SubscribeToDragEventBottom()
    {
        SubscribeToDragEvent(MainLayoutDragEventKind.BottomResizeRow);
    }
    
    /* Start StartupControlDisplay.razor */
    private async Task StartProgramWithoutDebuggingOnClick()
    {
        var localStartupControlState = DotNetService.IdeService.GetIdeStartupControlState();
        var activeStartupControl = localStartupControlState.StartupControlList.FirstOrDefault(
            x => x.StartupProjectAbsolutePath.Value == localStartupControlState.ActiveStartupProjectAbsolutePathValue);
        
        if (activeStartupControl is null)
            return;
    
        if (_userInterfaceSawIsExecuting)
        {
            var menuOptionList = new List<MenuOptionRecord>();
            
            menuOptionList.Add(new MenuOptionRecord(
                "View Output",
                MenuOptionKind.Other,
                onClickFunc: () => 
                {
                    var panelState = DotNetService.CommonService.GetPanelState();
                    var outputPanel = panelState.PanelList.FirstOrDefault(x => x.Title == "Output");
                    DotNetService.CommonService.ShowOrAddPanelTab(outputPanel);
                    return Task.CompletedTask;
                }));
                
            menuOptionList.Add(new MenuOptionRecord(
                "View Terminal",
                MenuOptionKind.Other,
                onClickFunc: () => 
                {
                    DotNetService.IdeService.TerminalGroup_SetActiveTerminal(IdeFacts.EXECUTION_KEY);
                    var panelState = DotNetService.CommonService.GetPanelState();
                    var outputPanel = panelState.PanelList.FirstOrDefault(x => x.Title == "Terminal");
                    DotNetService.CommonService.ShowOrAddPanelTab(outputPanel);
                    return Task.CompletedTask;
                }));
                
            menuOptionList.Add(new MenuOptionRecord(
                "Stop Execution",
                MenuOptionKind.Other,
                onClickFunc: () =>
                {
                    var localStartupControlState = DotNetService.IdeService.GetIdeStartupControlState();
                    var activeStartupControl = localStartupControlState.StartupControlList.FirstOrDefault(
                        x => x.StartupProjectAbsolutePath.Value == localStartupControlState.ActiveStartupProjectAbsolutePathValue);
                    
                    if (activeStartupControl is null)
                        return Task.CompletedTask;
                        
                    return activeStartupControl.StopButtonOnClick(DotNetService);
                }));
                
            await CommonFacts.RenderDropdownAsync(
                DotNetService.CommonService,
                DotNetService.CommonService.JsRuntimeCommonApi,
                _startButtonElementId,
                DropdownOrientation.Bottom,
                _startButtonDropdownKey,
                new MenuRecord(menuOptionList),
                _startButtonElementId,
                preventScroll: false);
        }
        else
        {
            await activeStartupControl.StartButtonOnClick(DotNetService)
                .ConfigureAwait(false);
        }
    }
    
    private async void OnIdeStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.TerminalHasExecutingProcessStateChanged ||
            ideStateChangedKind == IdeStateChangedKind.Ide_StartupControlStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    /* End StartupControlDisplay.razor */
    
    private void DispatchRegisterDialogRecordAction()
    {
        InitializationHelper.DispatchRegisterDialogRecordAction(
            DotNetService,
            new DialogViewModel(
                _settingsDialogKey,
                "Settings",
                typeof(SettingsDisplay),
                null,
                null,
                true,
                null));
    }
    
    public void Dispose()
    {
        
        DotNetService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnIdeStateChanged;
        
        BrowserResizeInterop.DisposeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
        
        _workerCancellationTokenSource.Cancel();
        _workerCancellationTokenSource.Dispose();
        
        DotNetService.CommonService.Continuous_StartAsyncTask = null;
        DotNetService.CommonService.Indefinite_StartAsyncTask = null;
    }
}
