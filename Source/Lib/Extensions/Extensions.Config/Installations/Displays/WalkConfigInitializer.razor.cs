using System.Text;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
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

namespace Walk.Extensions.Config.Installations.Displays;

public partial class WalkConfigInitializer : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();
    
    private DotNetObjectReference<WalkConfigInitializer>? _dotNetHelper;
    
    private enum CtrlTabKind
    {
        Dialogs,
        TextEditors,
    }
    
    private CtrlTabKind _ctrlTabKind = CtrlTabKind.Dialogs;
    
    private int _index;
    
    private bool _altIsDown;
    private bool _ctrlIsDown;

    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
    
        InitializePanelTabs();
        HandleCompilerServicesAndDecorationMappers();
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
        }
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
        var appOptionsState = DotNetService.IdeService.TextEditorService.CommonService.GetAppOptionsState();
    
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
            DotNetService.IdeService.TextEditorService.CommonService);
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
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));
        
        // InitializeBottomPanelTabs();
        var bottomPanel = CommonFacts.GetBottomPanelGroup(panelState);
        bottomPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            bottomPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW));

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
            DotNetService.IdeService.TextEditorService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(outputPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(outputPanel);

        // testExplorerPanel
        var testExplorerPanel = new Panel(
            "Test Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.TestExplorers.Displays.TestExplorerDisplay),
            null,
            DotNetService.IdeService.TextEditorService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(testExplorerPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(testExplorerPanel);

        // nuGetPanel
        var nuGetPanel = new Panel(
            "NuGet",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.Nugets.Displays.NuGetPackageManager),
            null,
            DotNetService.IdeService.TextEditorService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(nuGetPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(nuGetPanel);
        
        CodeSearch_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW));
    
        Panel_InitializeResizeHandleDimensionUnit(
            leftPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));
        
        // terminalGroupPanel: This UI has resizable parts that need to be initialized.
        TerminalGroup_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));
        
        // testExplorerPanel: This UI has resizable parts that need to be initialized.
        ReduceInitializeResizeHandleDimensionUnitAction(
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));
    
        // SetActivePanelTabAction
        DotNetService.CommonService.SetActivePanelTab(leftPanel.Key, solutionExplorerPanel.Key);
        
        // SetActivePanelTabAction
        DotNetService.IdeService.TextEditorService.CommonService.SetActivePanelTab(bottomPanel.Key, outputPanel.Key);
    }

    public void Panel_InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.CommonService.GetPanelState();

        var inPanelGroup = inState.PanelGroupList.FirstOrDefault(
            x => x.Key == panelGroupKey);

        if (inPanelGroup is not null)
        {
            if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW ||
                dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
            {
                if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW)
                {
                    if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose is null)
                            inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
                else if (dimensionUnit.Purpose != CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
                {
                    if (inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose is null)
                            inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
            }
        }
    }
    
    public void ReduceInitializeResizeHandleDimensionUnitAction(DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.GetTestExplorerState();

        if (dimensionUnit.Purpose != CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
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

            if (existingDimensionUnit.Purpose is not null)
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

            if (existingDimensionUnit.Purpose is not null)
            {
                return;
            }

            inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
        }
    }
    
    public void CodeSearch_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        var codeSearchState = DotNetService.IdeService.GetCodeSearchState();
    
        if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW)
        {
            if (codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose is null)
                    codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose is null)
                    codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }
    
    public void TerminalGroup_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        var terminalGroupState = DotNetService.IdeService.GetTerminalGroupState();
    
        if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
        {
            if (terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose is null)
                    terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose is null)
                    terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }

    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
