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
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Settings.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.DotNetSolutions.Models;

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
    
    public static readonly Key<DropdownRecord> DropdownKeyFile = Key<DropdownRecord>.NewKey();
    public const string ButtonFileId = "di_header-button-file";

    public static readonly Key<DropdownRecord> DropdownKeyTools = Key<DropdownRecord>.NewKey();
    public const string ButtonToolsId = "di_header-button-tools";

    public static readonly Key<DropdownRecord> DropdownKeyView = Key<DropdownRecord>.NewKey();
    public const string ButtonViewId = "di_header-button-view";

    public static readonly Key<DropdownRecord> DropdownKeyRun = Key<DropdownRecord>.NewKey();
    public const string ButtonRunId = "di_header-button-run";

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
            ButtonFileId,
            DropdownOrientation.Bottom,
            DropdownKeyFile,
            InitializeMenuFile(DotNetService),
            ButtonFileId,
            preventScroll: false);
    }
    
    private Task RenderToolsDropdownOnClick()
    {
        return CommonFacts.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            ButtonToolsId,
            DropdownOrientation.Bottom,
            DropdownKeyTools,
            InitializeMenuTools(DotNetService),
            ButtonToolsId,
            preventScroll: false);
    }
    
    private Task RenderViewDropdownOnClick()
    {
        return CommonFacts.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            ButtonViewId,
            DropdownOrientation.Bottom,
            DropdownKeyView,
            InitializeMenuView(DotNetService),
            ButtonViewId,
            preventScroll: false);
    }
    
    private Task RenderRunDropdownOnClick()
    {
        return CommonFacts.RenderDropdownAsync(
            DotNetService.CommonService,
            DotNetService.CommonService.JsRuntimeCommonApi,
            ButtonRunId,
            DropdownOrientation.Bottom,
            DropdownKeyRun,
            InitializeMenuRun(DotNetService),
            ButtonRunId,
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
        
        if (activeStartupControl.StartupProjectAbsolutePath.Value is null)
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
                    
                    if (activeStartupControl.StartupProjectAbsolutePath.Value is null)
                        return Task.CompletedTask;
                        
                    return DotNetService.StopButtonOnClick(activeStartupControl);
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
            await DotNetService.StartButtonOnClick(activeStartupControl)
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
    
    public static MenuRecord InitializeMenuRun(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Build Project (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = DotNetService.IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
                    x => x.StartupProjectAbsolutePath.Value == startupControlState.ActiveStartupProjectAbsolutePathValue);

                if (activeStartupControl.StartupProjectAbsolutePath.Value is not null)
                    InitializationHelper.BuildProjectOnClick(DotNetService, activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    CommonFacts.DispatchError(nameof(InitializationHelper.BuildProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = DotNetService.IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
                    x => x.StartupProjectAbsolutePath.Value == startupControlState.ActiveStartupProjectAbsolutePathValue);

                if (activeStartupControl.StartupProjectAbsolutePath.Value is not null)
                    InitializationHelper.CleanProjectOnClick(DotNetService, activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    CommonFacts.DispatchError(nameof(InitializationHelper.CleanProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Build Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = DotNetService.GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    InitializationHelper.BuildSolutionOnClick(DotNetService, dotNetSolutionModel.AbsolutePath.Value);
                else
                    CommonFacts.DispatchError(nameof(InitializationHelper.BuildSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = DotNetService.GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    InitializationHelper.CleanSolutionOnClick(DotNetService, dotNetSolutionModel.AbsolutePath.Value);
                else
                    CommonFacts.DispatchError(nameof(InitializationHelper.CleanSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        return new MenuRecord(menuOptionsList);
    }
    
    public static MenuRecord InitializeMenuView(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();
        var panelState = DotNetService.CommonService.GetPanelState();
        var dialogState = DotNetService.CommonService.GetDialogState();
    
        foreach (var panel in panelState.PanelList)
        {
            menuOptionsList.Add(new MenuOptionRecord(
                panel.Title,
                MenuOptionKind.Delete,
                () => DotNetService.CommonService.ShowOrAddPanelTab(panel)));
        }
    
        if (menuOptionsList.Count == 0)
        {
            return new MenuRecord(MenuRecord.NoMenuOptionsExistList);
        }
        else
        {
            return new MenuRecord(menuOptionsList);
        }
    }
    
    public static MenuRecord InitializeMenuFile(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option New
        var menuOptionNewDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            MenuOptionKind.Other,
            DotNetService.OpenNewDotNetSolutionDialog);

        var menuOptionNew = new MenuOptionRecord(
            "New",
            MenuOptionKind.Other/*,
            subMenu: new MenuRecord(new List<MenuOptionRecord> { menuOptionNewDotNetSolution })*/);

        menuOptionsList.Add(menuOptionNew);
        
        // Menu Option Open
        var menuOptionOpenDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            MenuOptionKind.Other,
            () =>
            {
                DotNetSolutionState.ShowInputFile(DotNetService.IdeService, DotNetService);
                return Task.CompletedTask;
            });
        
        var menuOptionOpenFile = new MenuOptionRecord(
            "File",
            MenuOptionKind.Other,
            () =>
            {
                DotNetService.IdeService.Editor_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpenDirectory = new MenuOptionRecord(
            "Directory",
            MenuOptionKind.Other,
            () =>
            {
                DotNetService.IdeService.FolderExplorer_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpen = new MenuOptionRecord(
            "Open",
            MenuOptionKind.Other/*,
            subMenu: new MenuRecord(new List<MenuOptionRecord>()
            {
                menuOptionOpenDotNetSolution,
                menuOptionOpenFile,
                menuOptionOpenDirectory,
            })*/);

        menuOptionsList.Add(menuOptionOpen);

        var menuOptionSave = new MenuOptionRecord(
            "Save (Ctrl s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSave_NoTextEditorFocus(DotNetService.TextEditorService, Walk.TextEditor.RazorLib.TextEditorService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSave);

        var menuOptionSaveAll = new MenuOptionRecord(
            "Save All (Ctrl Shift s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSaveAll(DotNetService.TextEditorService, Walk.TextEditor.RazorLib.TextEditorService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSaveAll);

        // Menu Option Permissions
        var menuOptionPermissions = new MenuOptionRecord(
            "Permissions",
            MenuOptionKind.Delete,
            () => InitializationHelper.ShowPermissionsDialog(DotNetService));

        menuOptionsList.Add(menuOptionPermissions);

        return new MenuRecord(menuOptionsList);
    }

    public static MenuRecord InitializeMenuTools(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Find All
        var menuOptionFindAll = new MenuOptionRecord(
            "Find All (Ctrl Shift f)",
            MenuOptionKind.Delete,
            () =>
            {
                DotNetService.TextEditorService.Options_ShowFindAllDialog();
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionFindAll);

        // Menu Option Code Search
        var menuOptionCodeSearch = new MenuOptionRecord(
            "Code Search (Ctrl ,)",
            MenuOptionKind.Delete,
            () =>
            {
                DotNetService.IdeService.CodeSearchDialog ??= new DialogViewModel(
                    Key<IDynamicViewModel>.NewKey(),
                    "Code Search",
                    typeof(CodeSearchDisplay),
                    null,
                    null,
                    true,
                    null);

                DotNetService.CommonService.Dialog_ReduceRegisterAction(DotNetService.IdeService.CodeSearchDialog);
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionCodeSearch);

        return new MenuRecord(menuOptionsList);
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
