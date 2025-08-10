using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Displays;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Extensions.DotNet;

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

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout
{
    private const string TEST_STRING_FOR_MEASUREMENT = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int TEST_STRING_REPEAT_COUNT = 6;

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
}
