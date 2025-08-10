using Microsoft.JSInterop;
using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Displays;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.CompilerServices.Css;
using Walk.CompilerServices.Css.Decoration;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Json;
using Walk.CompilerServices.Json.Decoration;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Xml;
using Walk.CompilerServices.Xml.Html.Decoration;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.AppDatas.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout
{
    private static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();
    
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();
    
    private enum CtrlTabKind
    {
        Dialogs,
        TextEditors,
    }

    private async Task InitializeOnAfterRenderFirstRender()
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
    }

    public void NoiseyOnInitializedSteps()
    {
        DotNetService.TextEditorService.IdeBackgroundTaskApi = DotNetService.IdeService;

        _dirtyResourceUriBadge = new Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge(DotNetService.TextEditorService);
        _notificationBadge = new Walk.Common.RazorLib.Notifications.Models.NotificationBadge(DotNetService.CommonService);

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

        InitPanelGroup(_leftPanelGroupParameter);
        InitPanelGroup(_rightPanelGroupParameter);
        InitPanelGroup(_bottomPanelGroupParameter);
    }

    public async Task EnqueueOnInitializedSteps()
    {
        await DotNetService.CommonService.Options_SetFromLocalStorageAsync();
        await DotNetService.TextEditorService.Options_SetFromLocalStorageAsync();

        InitializeMenuFile();
        InitializeMenuTools();
        InitializeMenuView();
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
        var leftPanel = panelState.TopLeftPanelGroup;
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
        var rightPanel = panelState.TopRightPanelGroup;
        rightPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            rightPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // InitializeBottomPanelTabs();
        var bottomPanel = panelState.BottomPanelGroup;
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

        PanelGroup inPanelGroup;
            
        if (panelGroupKey == inState.TopLeftPanelGroup.Key)
        {
            inPanelGroup = inState.TopLeftPanelGroup;
        }
        else if (panelGroupKey == inState.TopRightPanelGroup.Key)
        {
            inPanelGroup = inState.TopRightPanelGroup;
        }
        else if (panelGroupKey == inState.BottomPanelGroup.Key)
        {
            inPanelGroup = inState.BottomPanelGroup;
        }
        else
        {
            return;
        }

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

    // private const string TEST_STRING_FOR_MEASUREMENT = "abcdefghijklmnopqrstuvwxyz0123456789";

    // TODO: Just define all this in JavaScript to avoid the JSInterop for the parameter?
    //
    //      CharacterWidth         | LineHeight
    // 99,000 => 5.021239618406285 | 20
    // 60,000 => 7.146064814814815 | 20
    // 30,000 => 7.146064814814815 | 20
    // 25,000 => 7.146064444444445 | 20
    // 18,000 => 7.146064814814815 | 20
    //  9,000 => 7.146064814814815 | 20
    //  5,000 => 7.146066666666667 | 20
    //  1,000 => 7.146055555555556 | 20
    //    100 => 7.146111111111111 | 20
    //     40 => 7.145833333333333 | 20
    //     20 => 7.145833333333333 | 20
    //     11 => 7.146464646464646 | 20
    //     10 => 7.147222222222222 | 20
    //      9 => 7.145061728395062 | 20
    //      8 => 7.145833333333333 | 20
    //      7 => 7.146825396825397 | 20
    //      6 => 7.148148148148148 | 20
    //      5 => 7.144444444444445 | 20
    //      4 => 7.145833333333333 | 20
    //      3 => 7.148148148148148 | 20
    //      2 => 7.152777777777778 | 20
    //      1 => 7.138888888888889 | 20
    // private const int TEST_STRING_REPEAT_COUNT = 11;
}
