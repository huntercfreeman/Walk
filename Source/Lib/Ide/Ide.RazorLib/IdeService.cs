using System.Collections.Concurrent;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.FileSystems.Displays;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Terminals.Displays;
using Walk.Ide.RazorLib.Shareds.Displays;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService : IBackgroundTaskGroup
{
    public static readonly Key<TextEditorGroup> EditorTextEditorGroupKey = Key<TextEditorGroup>.NewKey();

    private readonly IServiceProvider _serviceProvider;

    public IdeService(
        WalkIdeConfig ideConfig,
        TextEditorService textEditorService,
        IServiceProvider serviceProvider)
    {
        IdeConfig = ideConfig;
        TextEditorService = textEditorService;
        _serviceProvider = serviceProvider;
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public WalkIdeConfig IdeConfig { get; }
    public TextEditorService TextEditorService { get; }
    public CommonService CommonService => TextEditorService.CommonService;

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly ConcurrentQueue<IdeWorkArgs> _workQueue = new();

    private static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();

    public void Enqueue(IdeWorkArgs workArgs)
    {
        _workQueue.Enqueue(workArgs);
        TextEditorService.CommonService.Continuous_Enqueue(this);
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out IdeWorkArgs workArgs))
            return ValueTask.CompletedTask;

        switch (workArgs.WorkKind)
        {
            case IdeWorkKind.WalkIdeInitializerOnInit:
                return Do_WalkIdeInitializerOnInit();
            case IdeWorkKind.IdeHeaderOnInit:
                return Do_IdeHeaderOnInit(workArgs.IdeMainLayout);
            case IdeWorkKind.FileContentsWereModifiedOnDisk:
                return Editor_Do_FileContentsWereModifiedOnDisk(
                    workArgs.StringValue, workArgs.TextEditorModel, workArgs.FileLastWriteTime, workArgs.NotificationInformativeKey);
            case IdeWorkKind.SaveFile:
                return Do_SaveFile(workArgs.AbsolutePath, workArgs.StringValue, workArgs.OnAfterSaveCompletedWrittenDateTimeFunc, workArgs.CancellationToken);
            case IdeWorkKind.SetFolderExplorerState:
                return Do_SetFolderExplorerState(workArgs.AbsolutePath);
            case IdeWorkKind.SetFolderExplorerTreeView:
                return Do_SetFolderExplorerTreeView(workArgs.AbsolutePath);
            case IdeWorkKind.RequestInputFileStateForm:
                return Do_RequestInputFileStateForm(
                    workArgs.StringValue, workArgs.OnAfterSubmitFunc, workArgs.SelectionIsValidFunc, workArgs.InputFilePatterns);
            case IdeWorkKind.OpenParentDirectoryAction:
            {
                return InputFile_Do_OpenParentDirectoryAction(
                    CommonService, workArgs.TreeViewAbsolutePath);
            }
            case IdeWorkKind.RefreshCurrentSelectionAction:
            {
                return InputFile_Do_RefreshCurrentSelectionAction(workArgs.TreeViewAbsolutePath);
            }
            case IdeWorkKind.PerformNewFile:
                return Do_PerformNewFile(
                    workArgs.StringValue,
                    workArgs.ExactMatchFileTemplate,
                    workArgs.RelatedMatchFileTemplatesList,
                    workArgs.NamespacePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformNewDirectory:
                return Do_PerformNewDirectory(
                    workArgs.StringValue,
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformDeleteFile:
                return Do_PerformDeleteFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformCopyFile:
                return Do_PerformCopyFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformCutFile:
                return Do_PerformCutFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformPasteFile:
                return Do_PerformPasteFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            default:
                Console.WriteLine($"{nameof(IdeService)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
        }
    }

    public ValueTask Do_WalkIdeInitializerOnInit()
    {
        AddExecutionTerminal();
        AddGeneralTerminal();

        CodeSearch_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW));

        InitializePanelResizeHandleDimensionUnit();
        InitializePanelTabs();
        // CommandFactory_Initialize();

        return ValueTask.CompletedTask;
    }

    private void InitializePanelResizeHandleDimensionUnit()
    {
        // Left
        var leftPanel = CommonFacts.GetTopLeftPanelGroup(CommonService.GetPanelState());
        leftPanel.CommonService = CommonService;
        CommonService.Panel_InitializeResizeHandleDimensionUnit(
            leftPanel.Key,
            new DimensionUnit(
                () => CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));

        // Right
        var rightPanel = CommonFacts.GetTopRightPanelGroup(CommonService.GetPanelState());
        rightPanel.CommonService = CommonService;
        CommonService.Panel_InitializeResizeHandleDimensionUnit(
            rightPanel.Key,
            new DimensionUnit(
                () => CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));

        // Bottom
        var bottomPanel = CommonFacts.GetBottomPanelGroup(CommonService.GetPanelState());
        bottomPanel.CommonService = CommonService;
        CommonService.Panel_InitializeResizeHandleDimensionUnit(
            bottomPanel.Key,
            new DimensionUnit(
                () => CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW));
    }

    private void InitializePanelTabs()
    {
        InitializeLeftPanelTabs();
        InitializeRightPanelTabs();
        InitializeBottomPanelTabs();
    }

    private void InitializeLeftPanelTabs()
    {
        var leftPanel = CommonFacts.GetTopLeftPanelGroup(CommonService.GetPanelState());
        leftPanel.CommonService = CommonService;

        // folderExplorerPanel
        var folderExplorerPanel = new Panel(
            "Folder Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            CommonFacts.FolderExplorerContext.ContextKey,
            typeof(FolderExplorerDisplay),
            null,
            CommonService);
        CommonService.RegisterPanel(folderExplorerPanel);
        CommonService.RegisterPanelTab(leftPanel.Key, folderExplorerPanel, false);

        // SetActivePanelTabAction
        CommonService.SetActivePanelTab(leftPanel.Key, folderExplorerPanel.Key);
    }

    private void InitializeRightPanelTabs()
    {
        var rightPanel = CommonFacts.GetTopRightPanelGroup(CommonService.GetPanelState());
        rightPanel.CommonService = CommonService;
    }

    private void InitializeBottomPanelTabs()
    {
        var bottomPanel = CommonFacts.GetBottomPanelGroup(CommonService.GetPanelState());
        bottomPanel.CommonService = CommonService;

        // terminalGroupPanel
        var terminalGroupPanel = new Panel(
            "Terminal",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            CommonFacts.TerminalContext.ContextKey,
            typeof(TerminalGroupDisplay),
            null,
            CommonService);
        CommonService.RegisterPanel(terminalGroupPanel);
        CommonService.RegisterPanelTab(bottomPanel.Key, terminalGroupPanel, false);
        // This UI has resizable parts that need to be initialized.
        TerminalGroup_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN));

        // SetActivePanelTabAction
        //_panelService.SetActivePanelTab(bottomPanel.Key, terminalGroupPanel.Key);
    }

    private void AddGeneralTerminal()
    {
        if (CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
        {
            Terminal_Register(
                new TerminalWebsite(
                    "General",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            TextEditorService)),
                    CommonService)
                {
                    Key = IdeFacts.GENERAL_KEY
                });
        }
        else
        {
            Terminal_Register(
                new Terminal(
                    "General",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            TextEditorService)),
                    this)
                {
                    Key = IdeFacts.GENERAL_KEY
                });
        }
    }

    private void AddExecutionTerminal()
    {
        if (CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
        {
            Terminal_Register(
                new TerminalWebsite(
                    "Execution",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            TextEditorService)),
                    CommonService)
                {
                    Key = IdeFacts.EXECUTION_KEY
                });
        }
        else
        {
            Terminal_Register(
                new Terminal(
                    "Execution",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            TextEditorService)),
                    this)
                {
                    Key = IdeFacts.EXECUTION_KEY
                });
        }
    }

    public ValueTask Do_IdeHeaderOnInit(IdeMainLayout ideMainLayout)
    {
        InitializeMenuFile();
        InitializeMenuTools();
        ideMainLayout.InitializeMenuView();

        // AddAltKeymap(ideMainLayout);
        return ValueTask.CompletedTask;
    }

    private void InitializeMenuFile()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Open
        var menuOptionOpenFile = new MenuOptionRecord(
            "File",
            MenuOptionKind.Other,
            () =>
            {
                Editor_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpenDirectory = new MenuOptionRecord(
            "Directory",
            MenuOptionKind.Other,
            () =>
            {
                FolderExplorer_ShowInputFile();
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

        // Menu Option Permissions
        var menuOptionPermissions = new MenuOptionRecord(
            "Permissions",
            MenuOptionKind.Delete,
            ShowPermissionsDialog);

        menuOptionsList.Add(menuOptionPermissions);

        Ide_SetMenuFile(new MenuRecord(menuOptionsList));
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
                TextEditorService.Options_ShowFindAllDialog();
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionFindAll);

        // Menu Option Code Search
        var menuOptionCodeSearch = new MenuOptionRecord(
            "Code Search (Ctrl ,)",
            MenuOptionKind.Delete,
            () =>
            {
                CodeSearchDialog ??= new DialogViewModel(
                    Key<IDynamicViewModel>.NewKey(),
                    "Code Search",
                    typeof(CodeSearchDisplay),
                    null,
                    null,
                    true,
                    null);

                CommonService.Dialog_ReduceRegisterAction(CodeSearchDialog);
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

        Ide_SetMenuTools(new MenuRecord(menuOptionsList));
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

        CommonService.Dialog_ReduceRegisterAction(dialogRecord);
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
    
    public void Editor_ShowInputFile()
    {
        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = "TextEditor",
            OnAfterSubmitFunc = absolutePath =>
            {
                // TODO: Why does 'isDirectory: false' not work?
                CommonService.EnvironmentProvider.DeletionPermittedRegister(new(absolutePath.Value, isDirectory: true));
            
                TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
                {
                    await TextEditorService.OpenInEditorAsync(
                        editContext,
                        absolutePath.Value,
                        true,
                        null,
                        new Category("main"),
                        Key<TextEditorViewModel>.NewKey());
                });
                return Task.CompletedTask;
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            InputFilePatterns = new()
            {
                new InputFilePattern("File", absolutePath => !absolutePath.IsDirectory)
            }
        });
    }

    public async Task Editor_FastParseFunc(FastParseArgs fastParseArgs)
    {
        /*var resourceUri = fastParseArgs.ResourceUri;

        var compilerService = _compilerServiceRegistry.GetCompilerService(fastParseArgs.ExtensionNoPeriod);

        compilerService.RegisterResource(
            fastParseArgs.ResourceUri,
            shouldTriggerResourceWasModified: false);
            
        var uniqueTextEditorWork = new UniqueTextEditorWork(TextEditorService, editContext =>
            compilerService.FastParseAsync(editContext, fastParseArgs.ResourceUri, _fileSystemProvider));
        
        TextEditorService.WorkerArbitrary.EnqueueUniqueTextEditorWork(uniqueTextEditorWork);*/
    }
    
    public async Task Editor_RegisterModelFunc(RegisterModelArgs registerModelArgs)
    {
        var model = TextEditorService.Model_GetOrDefault(registerModelArgs.ResourceUri);
        
        if (model is not null)
        {
            await Editor_CheckIfContentsWereModifiedAsync(
                    registerModelArgs.ResourceUri.Value,
                    model)
                .ConfigureAwait(false);
            return;
        }
            
        var resourceUri = registerModelArgs.ResourceUri;

        var fileLastWriteTime = await CommonService.FileSystemProvider.File
            .GetLastWriteTimeAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var content = await CommonService.FileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(resourceUri.Value, false);
        var decorationMapper = TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
        var compilerService = TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod);

        model = new TextEditorModel(
            resourceUri,
            fileLastWriteTime,
            absolutePath.ExtensionNoPeriod,
            content,
            decorationMapper,
            compilerService,
            TextEditorService);
            
        var modelModifier = new TextEditorModel(model);
        modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
        
        model = modelModifier;

        TextEditorService.Model_RegisterCustom(registerModelArgs.EditContext, model);
        
        model.PersistentState.CompilerService.RegisterResource(
            model.PersistentState.ResourceUri,
            shouldTriggerResourceWasModified: false);
        
        modelModifier = registerModelArgs.EditContext.GetModelModifier(resourceUri);

        if (modelModifier is null)
            return;

        await compilerService.ParseAsync(registerModelArgs.EditContext, modelModifier, shouldApplySyntaxHighlighting: false);
    }

    public async Task<Key<TextEditorViewModel>> Editor_TryRegisterViewModelFunc(TryRegisterViewModelArgs registerViewModelArgs)
    {
        var viewModelKey = Key<TextEditorViewModel>.NewKey();
        
        var model = TextEditorService.Model_GetOrDefault(registerViewModelArgs.ResourceUri);

        if (model is null)
        {
            NotificationHelper.DispatchDebugMessage(nameof(Editor_TryRegisterViewModelFunc), () => "model is null: " + registerViewModelArgs.ResourceUri.Value, CommonService, TimeSpan.FromSeconds(4));
            return Key<TextEditorViewModel>.Empty;
        }

        var viewModel = TextEditorService.Model_GetViewModelsOrEmpty(registerViewModelArgs.ResourceUri)
            .FirstOrDefault(x => x.PersistentState.Category == registerViewModelArgs.Category);

        if (viewModel is not null)
            return viewModel.PersistentState.ViewModelKey;

        viewModel = new TextEditorViewModel(
            viewModelKey,
            registerViewModelArgs.ResourceUri,
            TextEditorService,
            TextEditorVirtualizationResult.ConstructEmpty(),
            new TextEditorDimensions(0, 0, 0, 0),
            scrollLeft: 0,
            scrollTop: 0,
            scrollWidth: 0,
            scrollHeight: 0,
            marginScrollHeight: 0,
            registerViewModelArgs.Category);

        var firstPresentationLayerKeys = new List<Key<TextEditorPresentationModel>>
        {
            TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
            TextEditorFacts.FindOverlayPresentation_PresentationKey,
        };

        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
            registerViewModelArgs.ResourceUri.Value,
            false);

        viewModel.PersistentState.OnSaveRequested = Editor_HandleOnSaveRequested;
        viewModel.PersistentState.GetTabDisplayNameFunc = _ => absolutePath.NameWithExtension;
        viewModel.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;
        
        TextEditorService.ViewModel_Register(registerViewModelArgs.EditContext, viewModel);
        return viewModelKey;
    }
    
    private void Editor_HandleOnSaveRequested(TextEditorModel innerTextEditor)
    {
        var innerContent = innerTextEditor.GetAllText_WithOriginalLineEndings();
        
        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
            innerTextEditor.PersistentState.ResourceUri.Value,
            false);

        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.SaveFile,
            AbsolutePath = absolutePath,
            StringValue = innerContent,
            OnAfterSaveCompletedWrittenDateTimeFunc = writtenDateTime =>
            {
                if (writtenDateTime is not null)
                {
                    TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                    {
                        var modelModifier = editContext.GetModelModifier(innerTextEditor.PersistentState.ResourceUri);
                        if (modelModifier is null)
                            return ValueTask.CompletedTask;
                    
                        TextEditorService.Model_SetResourceData(
                            editContext,
                            modelModifier,
                            writtenDateTime.Value);
                        return ValueTask.CompletedTask;
                    });
                }

                return Task.CompletedTask;
            },
            CancellationToken = CancellationToken.None
        });
    }

    public async Task<bool> Editor_TryShowViewModelFunc(TryShowViewModelArgs showViewModelArgs)
    {
        TextEditorService.Group_Register(EditorTextEditorGroupKey);

        var viewModel = TextEditorService.ViewModel_GetOrDefault(showViewModelArgs.ViewModelKey);

        if (viewModel is null)
            return false;

        if (viewModel.PersistentState.Category == new Category("main") &&
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            showViewModelArgs = new TryShowViewModelArgs(
                showViewModelArgs.ViewModelKey,
                EditorTextEditorGroupKey,
                showViewModelArgs.ShouldSetFocusToEditor,
                showViewModelArgs.CommonService,
                showViewModelArgs.IdeBackgroundTaskApi);
        }

        if (showViewModelArgs.ViewModelKey == Key<TextEditorViewModel>.Empty ||
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            return false;
        }

        TextEditorService.Group_AddViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);

        TextEditorService.Group_SetActiveViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);
            
        if (showViewModelArgs.ShouldSetFocusToEditor)
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(showViewModelArgs.ViewModelKey);
                return viewModel.FocusAsync();
            });
        }

        return true;
    }

    private async Task Editor_CheckIfContentsWereModifiedAsync(
        string inputFileAbsolutePathString,
        TextEditorModel textEditorModel)
    {
        var fileLastWriteTime = await CommonService.FileSystemProvider.File
            .GetLastWriteTimeAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        if (fileLastWriteTime > textEditorModel.ResourceLastWriteTime)
        {
            var notificationInformativeKey = Key<IDynamicViewModel>.NewKey();

            var notificationInformative = new NotificationViewModel(
                notificationInformativeKey,
                "File contents were modified on disk",
                typeof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay),
                new Dictionary<string, object?>
                {
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.Message),
                            "File contents were modified on disk"
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.AcceptOptionTextOverride),
                            "Reload"
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.OnAfterAcceptFunc),
                            new Func<Task>(() =>
                            {
                                Enqueue(new IdeWorkArgs
                                {
                                    WorkKind = IdeWorkKind.FileContentsWereModifiedOnDisk,
                                    StringValue = inputFileAbsolutePathString,
                                    TextEditorModel = textEditorModel,
                                    FileLastWriteTime = fileLastWriteTime,
                                    NotificationInformativeKey = notificationInformativeKey,
                                });

                                return Task.CompletedTask;
                            })
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.OnAfterDeclineFunc),
                            new Func<Task>(() =>
                            {
                                CommonService.Notification_ReduceDisposeAction(notificationInformativeKey);
                                return Task.CompletedTask;
                            })
                        },
                },
                TimeSpan.FromSeconds(20),
                true,
                null);

            CommonService.Notification_ReduceRegisterAction(notificationInformative);
        }
    }

    private async ValueTask Editor_Do_FileContentsWereModifiedOnDisk(string inputFileAbsolutePathString, TextEditorModel textEditorModel, DateTime fileLastWriteTime, Key<IDynamicViewModel> notificationInformativeKey)
    {
        CommonService.Notification_ReduceDisposeAction(notificationInformativeKey);

        var content = await CommonService.FileSystemProvider.File
            .ReadAllTextAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(textEditorModel.PersistentState.ResourceUri);
            if (modelModifier is null)
                return ValueTask.CompletedTask;

            TextEditorService.Model_Reload(
                editContext,
                modelModifier,
                content,
                fileLastWriteTime);

            if (modelModifier.PersistentState.CompilerService is not null)    
                modelModifier.PersistentState.CompilerService.ResourceWasModified(modelModifier.PersistentState.ResourceUri, Array.Empty<TextEditorTextSpan>());
            return ValueTask.CompletedTask;
        });
    }
    
    private async ValueTask Do_SaveFile(
        AbsolutePath absolutePath,
        string content,
        Func<DateTime?, Task> onAfterSaveCompletedWrittenDateTimeFunc,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var absolutePathString = absolutePath.Value;

        if (absolutePathString is not null &&
            await CommonService.FileSystemProvider.File.ExistsAsync(absolutePathString).ConfigureAwait(false))
        {
            await CommonService.FileSystemProvider.File.WriteAllTextAsync(absolutePathString, content).ConfigureAwait(false);
        }
        else
        {
            // TODO: Save As to make new file
            NotificationHelper.DispatchInformative("Save Action", "File not found. TODO: Save As", CommonService, TimeSpan.FromSeconds(7));
        }

        DateTime? fileLastWriteTime = null;

        if (absolutePathString is not null)
        {
            fileLastWriteTime = await CommonService.FileSystemProvider.File.GetLastWriteTimeAsync(
                    absolutePathString,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        if (onAfterSaveCompletedWrittenDateTimeFunc is not null)
            await onAfterSaveCompletedWrittenDateTimeFunc.Invoke(fileLastWriteTime);
    }
    
    private ValueTask Do_SetFolderExplorerState(AbsolutePath folderAbsolutePath)
    {
        FolderExplorer_With(
            inFolderExplorerState => inFolderExplorerState with
            {
                AbsolutePath = folderAbsolutePath
            });

        return Do_SetFolderExplorerTreeView(folderAbsolutePath);
    }

    private async ValueTask Do_SetFolderExplorerTreeView(AbsolutePath folderAbsolutePath)
    {
        FolderExplorer_With(inFolderExplorerState => inFolderExplorerState with
        {
            IsLoadingFolderExplorer = true
        });
        
        CommonService.EnvironmentProvider.DeletionPermittedRegister(new(folderAbsolutePath.Value, true));

        var rootNode = new TreeViewAbsolutePath(
            folderAbsolutePath,
            CommonService,
            true,
            true);

        await rootNode.LoadChildListAsync().ConfigureAwait(false);

        if (!CommonService.TryGetTreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                out var treeViewState))
        {
            CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                new List<TreeViewNoType>() { rootNode }));
        }
        else
        {
            CommonService.TreeView_WithRootNodeAction(FolderExplorerState.TreeViewContentStateKey, rootNode);

            CommonService.TreeView_SetActiveNodeAction(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                true,
                false);
        }

        FolderExplorer_With(inFolderExplorerState => inFolderExplorerState with
        {
            IsLoadingFolderExplorer = false
        });
    }

    public void FolderExplorer_ShowInputFile()
    {
        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = "Folder Explorer",
            OnAfterSubmitFunc = async absolutePath =>
            {
                if (absolutePath.ExactInput is not null)
                    await Do_SetFolderExplorerState(absolutePath).ConfigureAwait(false);
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null || !absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            InputFilePatterns = [
                new InputFilePattern("Directory", absolutePath => absolutePath.IsDirectory)
            ]
        });
    }
    
    private ValueTask Do_RequestInputFileStateForm(
        string message,
        Func<AbsolutePath, Task> onAfterSubmitFunc,
        Func<AbsolutePath, Task<bool>> selectionIsValidFunc,
        List<InputFilePattern> inputFilePatternsList)
    {
        InputFile_StartInputFileStateForm(
            message,
            onAfterSubmitFunc,
            selectionIsValidFunc,
            inputFilePatternsList);

        var inputFileDialog = new DialogViewModel(
            CommonFacts.InputFileDialogKey,
            "Input File",
            typeof(Walk.Ide.RazorLib.InputFiles.Displays.InputFileDisplay),
            null,
            Walk.Ide.RazorLib.Htmls.Models.HtmlFacts.Classes.DIALOG_PADDING_0,
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(inputFileDialog);

        return ValueTask.CompletedTask;
    }
}
