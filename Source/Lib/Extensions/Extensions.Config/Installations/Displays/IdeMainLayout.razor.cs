using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Displays;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.Settings.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;

using System.Text;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.AppDatas.Models;

// CompilerServiceRegistry.cs
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.CompilerServices.Css;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Json;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Xml;
using Walk.TextEditor.RazorLib.CompilerServices;

// DecorationMapperRegistry.cs
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.CompilerServices.Css.Decoration;
using Walk.CompilerServices.Json.Decoration;
using Walk.CompilerServices.Xml.Html.Decoration;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.TextEditor.RazorLib.Options.Models;

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
    
    private readonly List<IBadgeModel> _footerBadgeList = new();
    
    private static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();
    
    private IDialog _dialogRecord = new DialogViewModel(
        Key<IDynamicViewModel>.NewKey(),
        "Settings",
        typeof(SettingsDisplay),
        null,
        null,
        true,
        null);

    private PanelGroupParameter _leftPanelGroupParameter;
    private PanelGroupParameter _rightPanelGroupParameter;
    private PanelGroupParameter _bottomPanelGroupParameter;
    
    private ResizableColumnParameter _topLeftResizableColumnParameter;
    private ResizableColumnParameter _topRightResizableColumnParameter;
    
    private ResizableRowParameter _resizableRowParameter;
    
    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
    
        InitializePanelTabs();
        HandleCompilerServicesAndDecorationMappers();
        
        ///////////
        
        MeasureLineHeight_UiRenderStep();
    
        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
    
        DotNetService.CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.WalkCommonInitializerWork
        });
        
        ///
    
        _footerBadgeList.Add(new Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge(DotNetService.TextEditorService));
        _footerBadgeList.Add(new Walk.Common.RazorLib.Notifications.Models.NotificationBadge(DotNetService.CommonService));
    
        var panelState = DotNetService.CommonService.GetPanelState();
    
        _topLeftResizableColumnParameter = new(
            CommonFacts.GetTopLeftPanelGroup(panelState).ElementDimensions,
            _editorElementDimensions,
            () => InvokeAsync(StateHasChanged));
            
        _topRightResizableColumnParameter = new(
            _editorElementDimensions,
            CommonFacts.GetTopRightPanelGroup(panelState).ElementDimensions,
            () => InvokeAsync(StateHasChanged));
            
        _resizableRowParameter = new(
            _bodyElementDimensions,
            CommonFacts.GetBottomPanelGroup(DotNetService.CommonService.GetPanelState()).ElementDimensions,
            () => InvokeAsync(StateHasChanged));
        
    
        _leftPanelGroupParameter = new(
            panelGroupKey: CommonFacts.LeftPanelGroupKey,
            adjacentElementDimensions: _editorElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Width,
            reRenderSelfAndAdjacentElementDimensionsFunc: () => InvokeAsync(StateHasChanged),
            cssClassString: null,
            badgeList: null);
    
        _rightPanelGroupParameter = new(
            panelGroupKey: CommonFacts.RightPanelGroupKey,
            adjacentElementDimensions: _editorElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Width,
            reRenderSelfAndAdjacentElementDimensionsFunc: () => InvokeAsync(StateHasChanged),
            cssClassString: null,
            badgeList: null);
        
        _bottomPanelGroupParameter = new(
            panelGroupKey: CommonFacts.BottomPanelGroupKey,
            cssClassString: "di_ide_footer",
            adjacentElementDimensions: _bodyElementDimensions,
            dimensionAttributeKind: DimensionAttributeKind.Height,
            reRenderSelfAndAdjacentElementDimensionsFunc: () => InvokeAsync(StateHasChanged),
            badgeList: _footerBadgeList);
    
        DotNetService.TextEditorService.IdeBackgroundTaskApi = DotNetService.IdeService;
    
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
    
        DotNetService.CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
        DotNetService.IdeService.IdeStateChanged += OnIdeMainLayoutStateChanged;
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;

        DotNetService.CommonService.Continuous_Enqueue(new BackgroundTask(Key<IBackgroundTaskGroup>.Empty, () =>
        {
            InitializeMenuFile();
            InitializeMenuTools();
            InitializeMenuView();

            // AddAltKeymap(ideMainLayout);
            return ValueTask.CompletedTask;
        }));
        
        DotNetService.IdeService.Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.WalkIdeInitializerOnInit,
        });
        
        _countOfTestCharacters = TEST_STRING_REPEAT_COUNT * TEST_STRING_FOR_MEASUREMENT.Length;
        
        DotNetService.TextEditorService.SecondaryChanged += OnNeedsMeasured;

        DotNetService.TextEditorService.Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DotNetService.Enqueue(new DotNetWorkArgs
            {
                WorkKind = DotNetWorkKind.WalkExtensionsDotNetInitializerOnAfterRender
            });
        
            var dotNetAppData = await DotNetService.AppDataService
                .ReadAppDataAsync<DotNetAppData>(
                    DotNetAppData.AssemblyName, DotNetAppData.TypeName, uniqueIdentifier: null, forceRefreshCache: false)
                .ConfigureAwait(false);
                
            await SetSolution(dotNetAppData).ConfigureAwait(false);
            
            if (DotNetService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            {
                // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
                // before you finish setting up the events?
                // (is this a thing, I'm just presuming this would be true).
                await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                    "walkConfig.appWideKeyboardEventsInitialize",
                    _dotNetHelper);
            }
        
            await DotNetService.TextEditorService.Options_SetFromLocalStorageAsync()
                .ConfigureAwait(false);

            await DotNetService.CommonService.
                Options_SetFromLocalStorageAsync()
                .ConfigureAwait(false);
                
            if (DotNetService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            {
                await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync("walkIde.preventDefaultBrowserKeybindings").ConfigureAwait(false);
            }
        }
        
        if (firstRender)
        {
            var token = _workerCancellationTokenSource.Token;

            if (DotNetService.CommonService.Continuous_StartAsyncTask is null)
            {
                DotNetService.CommonService.Continuous_StartAsyncTask = Task.Run(
                    () => DotNetService.CommonService.Continuous_ExecuteAsync(token),
                    token);
            }

            if (DotNetService.CommonService.WalkHostingInformation.WalkPurposeKind == WalkPurposeKind.Ide)
            {
                if (DotNetService.CommonService.Indefinite_StartAsyncTask is null)
                {
                    DotNetService.CommonService.Indefinite_StartAsyncTask = Task.Run(
                        () => DotNetService.CommonService.Indefinite_ExecuteAsync(token),
                        token);
                }
            }

            BrowserResizeInterop.SubscribeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
        
            await MeasureLineHeight_BackgroundTaskStep();
        }
        
        var tooltipModel = DotNetService.CommonService.GetTooltipState().TooltipModel;
        
        if (tooltipModel is not null && !tooltipModel.WasRepositioned && _tooltipModelPrevious != tooltipModel)
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
        
        if (firstRender)
        {
            await InvokeAsync(Ready);
            QueueRemeasureBackgroundTask();
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
            uiStringBuilder.Append("di_ide_main-layout ");
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
    
    public Task RenderFileDropdownOnClick()
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
    
    public Task RenderToolsDropdownOnClick()
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
    
    public Task RenderViewDropdownOnClick()
    {
        InitializeMenuView();
    
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
    
    public Task RenderRunDropdownOnClick()
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
    
    public void InitializeMenuView()
    {
        var menuOptionsList = new List<MenuOptionRecord>();
        var panelState = DotNetService.CommonService.GetPanelState();
        var dialogState = DotNetService.CommonService.GetDialogState();
    
        foreach (var panel in panelState.PanelList)
        {
            var menuOptionPanel = new MenuOptionRecord(
                panel.Title,
                MenuOptionKind.Delete,
                () => DotNetService.CommonService.ShowOrAddPanelTab(panel));
    
            menuOptionsList.Add(menuOptionPanel);
        }
    
        if (menuOptionsList.Count == 0)
        {
            DotNetService.IdeService.Ide_SetMenuView(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
        }
        else
        {
            DotNetService.IdeService.Ide_SetMenuView(new MenuRecord(menuOptionsList));
        }
    }
    
    private Task OpenInfoDialogOnClick()
    {
        var dialogRecord = new DialogViewModel(
            _infoDialogKey,
            "Info",
            typeof(IdeInfoDisplay),
            null,
            null,
            true,
            null);
    
        DotNetService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    public void DispatchRegisterDialogRecordAction() =>
        DotNetService.CommonService.Dialog_ReduceRegisterAction(_dialogRecord);

    private void InitializeMenuFile()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Open
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
            MenuOptionKind.Other,
            subMenu: new MenuRecord(new List<MenuOptionRecord>()
            {
                menuOptionOpenFile,
                menuOptionOpenDirectory,
            }));

        menuOptionsList.Add(menuOptionOpen);

        var menuOptionSave = new MenuOptionRecord(
            "Save (Ctrl s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSave_NoTextEditorFocus(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSave);

        var menuOptionSaveAll = new MenuOptionRecord(
            "Save All (Ctrl Shift s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSaveAll(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSaveAll);

        // Menu Option Permissions
        var menuOptionPermissions = new MenuOptionRecord(
            "Permissions",
            MenuOptionKind.Delete,
            ShowPermissionsDialog);

        menuOptionsList.Add(menuOptionPermissions);

        DotNetService.IdeService.Ide_SetMenuFile(new MenuRecord(menuOptionsList));
    }

    private void InitializeMenuTools()
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

        /*// Menu Option BackgroundTasks
        {
            var menuOptionBackgroundTasks = new MenuOptionRecord(
                "BackgroundTasks",
                MenuOptionKind.Delete,
                () =>
                {
                    var dialogRecord = new DialogViewModel(
                        _backgroundTaskDialogKey,
                        "Background Tasks",
                        typeof(BackgroundTaskDialogDisplay),
                        null,
                        null,
                        true,
                        null);

                    _dialogService.ReduceRegisterAction(dialogRecord);
                    return Task.CompletedTask;
                });

            menuOptionsList.Add(menuOptionBackgroundTasks);
        }*/

        //// Menu Option Solution Visualization
        //
        // NOTE: This UI element isn't useful yet, and its very unoptimized.
        //       Therefore, it is being commented out. Because given a large enough
        //       solution, clicking this by accident is a bit annoying.
        //
        //{
        //    var menuOptionSolutionVisualization = new MenuOptionRecord(
        //        "Solution Visualization",
        //        MenuOptionKind.Delete,
        //        () => 
        //        {
        //            var dialogRecord = new DialogViewModel(
        //                _solutionVisualizationDialogKey,
        //                "Solution Visualization",
        //                typeof(SolutionVisualizationDisplay),
        //                null,
        //                null,
        //                true);
        //    
        //            Dispatcher.Dispatch(new DialogState.RegisterAction(dialogRecord));
        //            return Task.CompletedTask;
        //        });
        //
        //    menuOptionsList.Add(menuOptionSolutionVisualization);
        //}

        DotNetService.IdeService.Ide_SetMenuTools(new MenuRecord(menuOptionsList));
    }

    private Task ShowPermissionsDialog()
    {
        var dialogRecord = new DialogViewModel(
            _permissionsDialogKey,
            "Permissions",
            typeof(PermissionsDisplay),
            null,
            null,
            true,
            null);

        DotNetService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add option to allow a user to disable the alt keymap to access to the header button dropdowns.
    /// </summary>
    /*private void AddAltKeymap(IdeMainLayout ideMainLayout)
    {
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "f",
                    Code = "KeyF",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = -1,
                },
                new CommonCommand("Open File Dropdown", "open-file-dropdown", false, async _ => await ideMainLayout.RenderFileDropdownOnClick()));

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "t",
                    Code = "KeyT",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = -1,
                },
                new CommonCommand("Open Tools Dropdown", "open-tools-dropdown", false, async _ => await ideMainLayout.RenderToolsDropdownOnClick()));

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "v",
                    Code = "KeyV",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = -1,
                },
                new CommonCommand("Open View Dropdown", "open-view-dropdown", false, async _ => await ideMainLayout.RenderViewDropdownOnClick()));

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs
            {
                Key = "r",
                Code = "KeyR",
                ShiftKey = false,
                CtrlKey = false,
                AltKey = true,
                MetaKey = false,
                LayerKey = -1,
            },
            new CommonCommand("Open Run Dropdown", "open-run-dropdown", false, async _ => await ideMainLayout.RenderRunDropdownOnClick()));
    }*/

    private static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();
    
    /* Start WalkConfigInitializer */
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();
    
    private DotNetObjectReference<IdeMainLayout>? _dotNetHelper;
    
    private enum CtrlTabKind
    {
        Dialogs,
        TextEditors,
    }
    
    private CtrlTabKind _ctrlTabKind = CtrlTabKind.Dialogs;
    
    private int _index;
    
    private bool _altIsDown;
    private bool _ctrlIsDown;
    
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

        /*
        if (!string.IsNullOrWhiteSpace(projectPersonalPath) &&
            await FileSystemProvider.File.ExistsAsync(projectPersonalPath).ConfigureAwait(false))
        {
            var projectAbsolutePath = EnvironmentProvider.AbsolutePathFactory(
                projectPersonalPath,
                false);

            var startupControl = StartupControlStateWrap.Value.StartupControlList.FirstOrDefault(
                x => x.StartupProjectAbsolutePath.Value == projectAbsolutePath.Value);
                
            if (startupControl is null)
                return;
            
            Dispatcher.Dispatch(new StartupControlState.SetActiveStartupControlKeyAction(startupControl.Key));    
        }
        */
    }
    
    private void HandleCompilerServicesAndDecorationMappers()
    {
        var cSharpCompilerService = new CSharpCompilerService(DotNetService.TextEditorService);
        var cSharpProjectCompilerService = new CSharpProjectCompilerService(DotNetService.TextEditorService);
        // var javaScriptCompilerService = new JavaScriptCompilerService(TextEditorService);
        var cssCompilerService = new CssCompilerService(DotNetService.TextEditorService);
        var dotNetSolutionCompilerService = new DotNetSolutionCompilerService(DotNetService.TextEditorService);
        var jsonCompilerService = new JsonCompilerService(DotNetService.TextEditorService);
        var razorCompilerService = new RazorCompilerService(DotNetService.TextEditorService, cSharpCompilerService);
        var xmlCompilerService = new XmlCompilerService(DotNetService.TextEditorService);
        var terminalCompilerService = new TerminalCompilerService(DotNetService.IdeService);
        var defaultCompilerService = new CompilerServiceDoNothing();

        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.HTML, xmlCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.XML, xmlCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT, cSharpProjectCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS, cSharpCompilerService);
        // DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT, JavaScriptCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, cSharpCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP, razorCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSHTML_CLASS, razorCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSS, cssCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JSON, jsonCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, dotNetSolutionCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, dotNetSolutionCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.TERMINAL, terminalCompilerService);
        
        //
        // Decoration Mapper starts after this point.
        //
        
        var cssDecorationMapper = new TextEditorCssDecorationMapper();
        var jsonDecorationMapper = new TextEditorJsonDecorationMapper();
        var genericDecorationMapper = new GenericDecorationMapper();
        var htmlDecorationMapper = new TextEditorHtmlDecorationMapper();
        var terminalDecorationMapper = new TerminalDecorationMapper();
        var defaultDecorationMapper = new TextEditorDecorationMapperDefault();

        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HTML, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.XML, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_PROJECT, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_CLASS, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_MARKUP, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSHTML_CLASS, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSS, cssDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JAVA_SCRIPT, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JSON, jsonDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TYPE_SCRIPT, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.F_SHARP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.PYTHON, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.H, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CPP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HPP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TERMINAL, terminalDecorationMapper);
    }
    
    private void InitializePanelTabs()
    {
        var panelState = DotNetService.CommonService.GetPanelState();
        var appOptionsState = DotNetService.CommonService.GetAppOptionsState();
    
        // InitializeLeftPanelTabs();
        var leftPanel = CommonFacts.GetTopLeftPanelGroup(panelState);
        leftPanel.CommonService = DotNetService.CommonService;
    
        // solutionExplorerPanel
        var solutionExplorerPanel = new Panel(
            "Solution Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.DotNetSolutions.Displays.SolutionExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(solutionExplorerPanel);
        ((List<IPanelTab>)leftPanel.TabList).Add(solutionExplorerPanel);
        
        // folderExplorerPanel
        var folderExplorerPanel = new Panel(
            "Folder Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(FolderExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(folderExplorerPanel);
        ((List<IPanelTab>)leftPanel.TabList).Add(folderExplorerPanel);
        
        // InitializeRightPanelTabs();
        var rightPanel = CommonFacts.GetTopRightPanelGroup(panelState);
        rightPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            rightPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // InitializeBottomPanelTabs();
        var bottomPanel = CommonFacts.GetBottomPanelGroup(panelState);
        bottomPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            bottomPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleRow));

        // terminalGroupPanel
        var terminalGroupPanel = new Panel(
            "Terminal",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Ide.RazorLib.Terminals.Displays.TerminalGroupDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(terminalGroupPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(terminalGroupPanel);

        // SetActivePanelTabAction
        //_panelService.SetActivePanelTab(bottomPanel.Key, terminalGroupPanel.Key);

        // outputPanel
        var outputPanel = new Panel(
            "Output",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.Outputs.Displays.OutputPanelDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(outputPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(outputPanel);

        // testExplorerPanel
        var testExplorerPanel = new Panel(
            "Test Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.TestExplorers.Displays.TestExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(testExplorerPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(testExplorerPanel);

        // nuGetPanel
        var nuGetPanel = new Panel(
            "NuGet",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.Nugets.Displays.NuGetPackageManager),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(nuGetPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(nuGetPanel);
        
        CodeSearch_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleRow));
    
        Panel_InitializeResizeHandleDimensionUnit(
            leftPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // terminalGroupPanel: This UI has resizable parts that need to be initialized.
        TerminalGroup_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // testExplorerPanel: This UI has resizable parts that need to be initialized.
        ReduceInitializeResizeHandleDimensionUnitAction(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
    
        // SetActivePanelTabAction
        DotNetService.CommonService.SetActivePanelTab(leftPanel.Key, solutionExplorerPanel.Key);
        
        // SetActivePanelTabAction
        DotNetService.CommonService.SetActivePanelTab(bottomPanel.Key, outputPanel.Key);
    }

    public void Panel_InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.CommonService.GetPanelState();

        var inPanelGroup = inState.PanelGroupList.FirstOrDefault(
            x => x.Key == panelGroupKey);

        if (inPanelGroup is not null)
        {
            if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow ||
                dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleColumn)
            {
                if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow)
                {
                    if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                            inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
                else if (dimensionUnit.Purpose != DimensionUnitPurposeKind.ResizableHandleColumn)
                {
                    if (inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                            inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
            }
        }
    }
    
    public void ReduceInitializeResizeHandleDimensionUnitAction(DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.GetTestExplorerState();

        if (dimensionUnit.Purpose != DimensionUnitPurposeKind.ResizableHandleColumn)
        {
            return;
        }

        // TreeViewElementDimensions
        {
            if (inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
            {
                return;
            }

            var existingDimensionUnit = inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList
                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

            if (existingDimensionUnit.Purpose != DimensionUnitPurposeKind.None)
            {
                return;
            }

            inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
        }

        // DetailsElementDimensions
        {
            if (inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
            {
                return;
            }

            var existingDimensionUnit = inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

            if (existingDimensionUnit.Purpose != DimensionUnitPurposeKind.None)
            {
                return;
            }

            inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
        }
    }
    
    public void CodeSearch_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        var codeSearchState = DotNetService.IdeService.GetCodeSearchState();
    
        if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow)
        {
            if (codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }
    
    public void TerminalGroup_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        var terminalGroupState = DotNetService.IdeService.GetTerminalGroupState();
    
        if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleColumn)
        {
            if (terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }
    /* End WalkConfigInitializer */
    
    #region WalkCommonInitializer
    
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    /// <summary>
    /// Only use this from the "UI thread".
    /// </summary>
    private readonly StringBuilder _styleBuilder = new();
    
    private readonly string _measureLineHeightElementId = "di_measure-line-height";
    
    /// <summary>The unit of measurement is Pixels (px)</summary>
    public const double OUTLINE_THICKNESS = 4;
    
    private string _lineHeightCssStyle;
    
    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
    
    public string TooltipRelativeX { get; set; } = string.Empty;
    public string TooltipRelativeY { get; set; } = string.Empty;
    
    private ITooltipModel? _tooltipModelPrevious = null;
    
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
                MeasureLineHeight_BackgroundTaskStep();
                break;
        }
    }
    
    private void MeasureLineHeight_UiRenderStep()
    {
        _lineHeightCssStyle = $"{DotNetService.CommonService.Options_FontFamilyCssStyleString} {DotNetService.CommonService.Options_FontSizeCssStyleString}";
        StateHasChanged();
    }
    
    private async Task MeasureLineHeight_BackgroundTaskStep()
    {
        DotNetService.CommonService.Continuous_Enqueue(new BackgroundTask(
            Key<IBackgroundTaskGroup>.Empty,
            async () =>
            {
                var lineHeight = await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeAsync<int>(
                    "walkCommon.getLineHeightInPixelsById",
                    _measureLineHeightElementId);
                DotNetService.CommonService.Options_SetLineHeight(lineHeight);
            }));
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
    
    private const string TEST_STRING_FOR_MEASUREMENT = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int TEST_STRING_REPEAT_COUNT = 6;
    
    private int _countOfTestCharacters;
    private string _measureCharacterWidthAndLineHeightElementId = "di_te_measure-character-width-and-line-height";
    
    private string _wrapperCssClass;
    private string _wrapperCssStyle;
    
    /// <summary>
    /// Only invoke this method from the UI thread due to the usage of the shared UiStringBuilder.
    /// </summary>
    private async Task Ready()
    {
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
            QueueRemeasureBackgroundTask();
        }
    }
    
    private void QueueRemeasureBackgroundTask()
    {
        DotNetService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            var charAndLineMeasurements = await DotNetService.TextEditorService.JsRuntimeTextEditorApi
                .GetCharAndLineMeasurementsInPixelsById(
                    _measureCharacterWidthAndLineHeightElementId,
                    _countOfTestCharacters)
                .ConfigureAwait(false);
                
            DotNetService.TextEditorService.Options_SetCharAndLineMeasurements(editContext, charAndLineMeasurements);
        });
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
