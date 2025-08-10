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

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;
    
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
        _footerBadgeList.Add(new Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge(IdeService.TextEditorService));
        _footerBadgeList.Add(new Walk.Common.RazorLib.Notifications.Models.NotificationBadge(IdeService.CommonService));
    
        var panelState = IdeService.CommonService.GetPanelState();
    
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
            CommonFacts.GetBottomPanelGroup(IdeService.CommonService.GetPanelState()).ElementDimensions,
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
    
        IdeService.TextEditorService.IdeBackgroundTaskApi = IdeService;
    
        _bodyElementDimensions.HeightDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(78, DimensionUnitKind.Percentage),
            new DimensionUnit(
                IdeService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
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
                IdeService.CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract)
        });
    
        IdeService.CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
        IdeService.IdeStateChanged += OnIdeMainLayoutStateChanged;
        IdeService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;

        IdeService.CommonService.Continuous_Enqueue(new BackgroundTask(Key<IBackgroundTaskGroup>.Empty, () =>
        {
            InitializeMenuFile();
            InitializeMenuTools();
            InitializeMenuView();

            // AddAltKeymap(ideMainLayout);
            return ValueTask.CompletedTask;
        }));
        
        IdeService.Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.WalkIdeInitializerOnInit,
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await IdeService.TextEditorService.Options_SetFromLocalStorageAsync()
                .ConfigureAwait(false);

            await IdeService.CommonService.
                Options_SetFromLocalStorageAsync()
                .ConfigureAwait(false);
                
            if (IdeService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            {
                await IdeService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync("walkIde.preventDefaultBrowserKeybindings").ConfigureAwait(false);
            }
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
            
        if (_previousDragStateWrapShouldDisplay != IdeService.CommonService.GetDragState().ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = IdeService.CommonService.GetDragState().ShouldDisplay;
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
        
            var uiStringBuilder = IdeService.CommonService.UiStringBuilder;
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append("di_ide_main-layout ");
            uiStringBuilder.Append(UnselectableClassCss);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(IdeService.CommonService.Options_ThemeCssClassString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(IdeService.TextEditorService.ThemeCssClassString);
            _classCssString = uiStringBuilder.ToString();
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append(IdeService.CommonService.Options_FontSizeCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(IdeService.CommonService.Options_FontFamilyCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(IdeService.CommonService.Options_ColorSchemeCssStyleString);
            _styleCssString = uiStringBuilder.ToString();
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append("display: flex; justify-content: space-between; border-bottom: ");
            uiStringBuilder.Append(IdeService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels);
            uiStringBuilder.Append("px solid var(--di_primary-border-color);");
            _headerCssStyle = uiStringBuilder.ToString();
        }
    }
    
    public Task RenderFileDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            IdeService.CommonService,
            IdeService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonFileId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyFile,
            IdeService.GetIdeState().MenuFile,
            IdeState.ButtonFileId,
            preventScroll: false);
    }
    
    public Task RenderToolsDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            IdeService.CommonService,
            IdeService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonToolsId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyTools,
            IdeService.GetIdeState().MenuTools,
            IdeState.ButtonToolsId,
            preventScroll: false);
    }
    
    public Task RenderViewDropdownOnClick()
    {
        InitializeMenuView();
    
        return DropdownHelper.RenderDropdownAsync(
            IdeService.CommonService,
            IdeService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonViewId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyView,
            IdeService.GetIdeState().MenuView,
            IdeState.ButtonViewId,
            preventScroll: false);
    }
    
    public Task RenderRunDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            IdeService.CommonService,
            IdeService.CommonService.JsRuntimeCommonApi,
            IdeState.ButtonRunId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyRun,
            IdeService.GetIdeState().MenuRun,
            IdeState.ButtonRunId,
            preventScroll: false);
    }
    
    public void InitializeMenuView()
    {
        var menuOptionsList = new List<MenuOptionRecord>();
        var panelState = IdeService.CommonService.GetPanelState();
        var dialogState = IdeService.CommonService.GetDialogState();
    
        foreach (var panel in panelState.PanelList)
        {
            var menuOptionPanel = new MenuOptionRecord(
                panel.Title,
                MenuOptionKind.Delete,
                () => IdeService.CommonService.ShowOrAddPanelTab(panel));
    
            menuOptionsList.Add(menuOptionPanel);
        }
    
        if (menuOptionsList.Count == 0)
        {
            IdeService.Ide_SetMenuView(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
        }
        else
        {
            IdeService.Ide_SetMenuView(new MenuRecord(menuOptionsList));
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
    
        IdeService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    public void DispatchRegisterDialogRecordAction() =>
        IdeService.CommonService.Dialog_ReduceRegisterAction(_dialogRecord);

    private void InitializeMenuFile()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Open
        var menuOptionOpenFile = new MenuOptionRecord(
            "File",
            MenuOptionKind.Other,
            () =>
            {
                IdeService.Editor_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpenDirectory = new MenuOptionRecord(
            "Directory",
            MenuOptionKind.Other,
            () =>
            {
                IdeService.FolderExplorer_ShowInputFile();
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
                TextEditorCommandDefaultFunctions.TriggerSave_NoTextEditorFocus(IdeService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSave);

        var menuOptionSaveAll = new MenuOptionRecord(
            "Save All (Ctrl Shift s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSaveAll(IdeService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSaveAll);

        // Menu Option Permissions
        var menuOptionPermissions = new MenuOptionRecord(
            "Permissions",
            MenuOptionKind.Delete,
            ShowPermissionsDialog);

        menuOptionsList.Add(menuOptionPermissions);

        IdeService.Ide_SetMenuFile(new MenuRecord(menuOptionsList));
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
                IdeService.TextEditorService.Options_ShowFindAllDialog();
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionFindAll);

        // Menu Option Code Search
        var menuOptionCodeSearch = new MenuOptionRecord(
            "Code Search (Ctrl ,)",
            MenuOptionKind.Delete,
            () =>
            {
                IdeService.CodeSearchDialog ??= new DialogViewModel(
                    Key<IDynamicViewModel>.NewKey(),
                    "Code Search",
                    typeof(CodeSearchDisplay),
                    null,
                    null,
                    true,
                    null);

                IdeService.CommonService.Dialog_ReduceRegisterAction(IdeService.CodeSearchDialog);
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

        IdeService.Ide_SetMenuTools(new MenuRecord(menuOptionsList));
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

        IdeService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
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

    public void Dispose()
    {
        IdeService.CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
        IdeService.IdeStateChanged -= OnIdeMainLayoutStateChanged;
        IdeService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
    }
}
