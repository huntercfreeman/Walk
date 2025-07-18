using System.Collections.Concurrent;
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
using Walk.TextEditor.RazorLib.Diffs.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Terminals.Displays;
using Walk.Ide.RazorLib.Shareds.Displays;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;

using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Displays;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Ide.RazorLib.Shareds.Models;

using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.Clipboards.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

using Walk.Ide.RazorLib.Menus.Models;

namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public class IdeBackgroundTaskApi : IBackgroundTaskGroup
{
	public static readonly Key<TextEditorGroup> EditorTextEditorGroupKey = Key<TextEditorGroup>.NewKey();

	private readonly IServiceProvider _serviceProvider;

    public IdeBackgroundTaskApi(
        WalkIdeConfig ideConfig,
        IIdeComponentRenderers ideComponentRenderers,
        TextEditorService textEditorService,
        IServiceProvider serviceProvider)
    {
        IdeConfig = ideConfig;
        IdeComponentRenderers = ideComponentRenderers;
        TextEditorService = textEditorService;
        _serviceProvider = serviceProvider;
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public WalkIdeConfig IdeConfig { get; }
    public IIdeComponentRenderers IdeComponentRenderers { get; }
    public TextEditorService TextEditorService { get; }
    public CommonUtilityService CommonUtilityService => TextEditorService.CommonUtilityService;

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly ConcurrentQueue<IdeBackgroundTaskApiWorkArgs> _workQueue = new();

    private static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();

    public void Enqueue(IdeBackgroundTaskApiWorkArgs workArgs)
    {
        _workQueue.Enqueue(workArgs);
        TextEditorService.CommonUtilityService.Continuous_EnqueueGroup(this);
    }

    public ValueTask Do_WalkIdeInitializerOnInit()
    {
        AddExecutionTerminal();
        AddGeneralTerminal();

        CodeSearch_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW));

        InitializePanelResizeHandleDimensionUnit();
        InitializePanelTabs();
        CommandFactory_Initialize();

        return ValueTask.CompletedTask;
    }

    private void InitializePanelResizeHandleDimensionUnit()
    {
        // Left
        {
            var leftPanel = PanelFacts.GetTopLeftPanelGroup(CommonUtilityService.GetPanelState());
            leftPanel.CommonUtilityService = CommonUtilityService;

            CommonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                leftPanel.Key,
                new DimensionUnit(
                    () => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                    DimensionUnitKind.Pixels,
                    DimensionOperatorKind.Subtract,
                    DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));
        }

        // Right
        {
            var rightPanel = PanelFacts.GetTopRightPanelGroup(CommonUtilityService.GetPanelState());
            rightPanel.CommonUtilityService = CommonUtilityService;

            CommonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                rightPanel.Key,
                new DimensionUnit(
                    () => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                    DimensionUnitKind.Pixels,
                    DimensionOperatorKind.Subtract,
                    DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));
        }

        // Bottom
        {
            var bottomPanel = PanelFacts.GetBottomPanelGroup(CommonUtilityService.GetPanelState());
            bottomPanel.CommonUtilityService = CommonUtilityService;

            CommonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                bottomPanel.Key,
                new DimensionUnit(
                    () => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                    DimensionUnitKind.Pixels,
                    DimensionOperatorKind.Subtract,
                    DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW));
        }
    }

    private void InitializePanelTabs()
    {
        InitializeLeftPanelTabs();
        InitializeRightPanelTabs();
        InitializeBottomPanelTabs();
    }

    private void InitializeLeftPanelTabs()
    {
        var leftPanel = PanelFacts.GetTopLeftPanelGroup(CommonUtilityService.GetPanelState());
        leftPanel.CommonUtilityService = CommonUtilityService;

        // folderExplorerPanel
        var folderExplorerPanel = new Panel(
            "Folder Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.FolderExplorerContext.ContextKey,
            typeof(FolderExplorerDisplay),
            null,
            CommonUtilityService);
        CommonUtilityService.RegisterPanel(folderExplorerPanel);
        CommonUtilityService.RegisterPanelTab(leftPanel.Key, folderExplorerPanel, false);

        // SetActivePanelTabAction
        CommonUtilityService.SetActivePanelTab(leftPanel.Key, folderExplorerPanel.Key);
    }

    private void InitializeRightPanelTabs()
    {
        var rightPanel = PanelFacts.GetTopRightPanelGroup(CommonUtilityService.GetPanelState());
        rightPanel.CommonUtilityService = CommonUtilityService;
    }

    private void InitializeBottomPanelTabs()
    {
        var bottomPanel = PanelFacts.GetBottomPanelGroup(CommonUtilityService.GetPanelState());
        bottomPanel.CommonUtilityService = CommonUtilityService;

        // terminalGroupPanel
        var terminalGroupPanel = new Panel(
            "Terminal",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.TerminalContext.ContextKey,
            typeof(TerminalGroupDisplay),
            null,
            CommonUtilityService);
        CommonUtilityService.RegisterPanel(terminalGroupPanel);
        CommonUtilityService.RegisterPanelTab(bottomPanel.Key, terminalGroupPanel, false);
        // This UI has resizable parts that need to be initialized.
        TerminalGroup_InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));

        // SetActivePanelTabAction
        //_panelService.SetActivePanelTab(bottomPanel.Key, terminalGroupPanel.Key);
    }

    private void AddGeneralTerminal()
    {
        if (CommonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            CommonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
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
                    CommonUtilityService)
                {
                    Key = TerminalFacts.GENERAL_KEY
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
                    Key = TerminalFacts.GENERAL_KEY
                });
        }
    }

    private void AddExecutionTerminal()
    {
        if (CommonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            CommonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
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
                    CommonUtilityService)
                {
                    Key = TerminalFacts.EXECUTION_KEY
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
                    Key = TerminalFacts.EXECUTION_KEY
                });
        }
    }

    public ValueTask Do_IdeHeaderOnInit(IdeMainLayout ideMainLayout)
    {
        InitializeMenuFile();
        InitializeMenuTools();
        ideMainLayout.InitializeMenuView();

        AddAltKeymap(ideMainLayout);
        return ValueTask.CompletedTask;
    }

    private void InitializeMenuFile()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Open
        {
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
        }

        // Menu Option Permissions
        {
            var menuOptionPermissions = new MenuOptionRecord(
                "Permissions",
                MenuOptionKind.Delete,
                ShowPermissionsDialog);

            menuOptionsList.Add(menuOptionPermissions);
        }

        Ide_SetMenuFile(new MenuRecord(menuOptionsList));
    }

    private void InitializeMenuTools()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Find All
        {
            var menuOptionFindAll = new MenuOptionRecord(
                "Find All (Ctrl Shift f)",
                MenuOptionKind.Delete,
                () =>
                {
                    TextEditorService.Options_ShowFindAllDialog();
                    return Task.CompletedTask;
                });

            menuOptionsList.Add(menuOptionFindAll);
        }

        // Menu Option Code Search
        {
            var menuOptionCodeSearch = new MenuOptionRecord(
                "Code Search (Ctrl ,)",
                MenuOptionKind.Delete,
                () =>
                {
                    CommandFactory_CodeSearchDialog ??= new DialogViewModel(
                        Key<IDynamicViewModel>.NewKey(),
                        "Code Search",
                        typeof(CodeSearchDisplay),
                        null,
                        null,
                        true,
                        null);

                    CommonUtilityService.Dialog_ReduceRegisterAction(CommandFactory_CodeSearchDialog);
                    return Task.CompletedTask;
                });

            menuOptionsList.Add(menuOptionCodeSearch);
        }

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
        //		"Solution Visualization",
        //        MenuOptionKind.Delete,
        //        () => 
        //        {
        //			var dialogRecord = new DialogViewModel(
        //	            _solutionVisualizationDialogKey,
        //	            "Solution Visualization",
        //	            typeof(SolutionVisualizationDisplay),
        //	            null,
        //	            null,
        //				true);
        //	
        //	        Dispatcher.Dispatch(new DialogState.RegisterAction(dialogRecord));
        //	        return Task.CompletedTask;
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

        CommonUtilityService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add option to allow a user to disable the alt keymap to access to the header button dropdowns.
    /// </summary>
    private void AddAltKeymap(IdeMainLayout ideMainLayout)
    {
        _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "f",
                    Code = "KeyF",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = Key<KeymapLayer>.Empty,
                },
                new CommonCommand("Open File Dropdown", "open-file-dropdown", false, async _ => await ideMainLayout.RenderFileDropdownOnClick()));

        _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "t",
                    Code = "KeyT",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = Key<KeymapLayer>.Empty,
                },
                new CommonCommand("Open Tools Dropdown", "open-tools-dropdown", false, async _ => await ideMainLayout.RenderToolsDropdownOnClick()));

        _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "v",
                    Code = "KeyV",
                    ShiftKey = false,
                    CtrlKey = false,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = Key<KeymapLayer>.Empty,
                },
                new CommonCommand("Open View Dropdown", "open-view-dropdown", false, async _ => await ideMainLayout.RenderViewDropdownOnClick()));

        _ = ContextFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs
            {
                Key = "r",
                Code = "KeyR",
                ShiftKey = false,
                CtrlKey = false,
                AltKey = true,
                MetaKey = false,
                LayerKey = Key<KeymapLayer>.Empty,
            },
            new CommonCommand("Open Run Dropdown", "open-run-dropdown", false, async _ => await ideMainLayout.RenderRunDropdownOnClick()));
    }
    
    public void Editor_ShowInputFile()
    {
        Enqueue(new IdeBackgroundTaskApiWorkArgs
        {
        	WorkKind = IdeBackgroundTaskApiWorkKind.RequestInputFileStateForm,
            Message = "TextEditor",
            OnAfterSubmitFunc = absolutePath =>
            {
            	// TODO: Why does 'isDirectory: false' not work?
				CommonUtilityService.EnvironmentProvider.DeletionPermittedRegister(new(absolutePath.Value, isDirectory: true));
            
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

        var fileLastWriteTime = await CommonUtilityService.FileSystemProvider.File
            .GetLastWriteTimeAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var content = await CommonUtilityService.FileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(resourceUri.Value, false);
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
        modelModifier.PerformRegisterPresentationModelAction(CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(FindOverlayPresentationFacts.EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(DiffPresentationFacts.EmptyInPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(DiffPresentationFacts.EmptyOutPresentationModel);
        
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
        	NotificationHelper.DispatchDebugMessage(nameof(Editor_TryRegisterViewModelFunc), () => "model is null: " + registerViewModelArgs.ResourceUri.Value, CommonUtilityService, TimeSpan.FromSeconds(4));
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
            CompilerServiceDiagnosticPresentationFacts.PresentationKey,
            FindOverlayPresentationFacts.PresentationKey,
        };

        var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
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
        
        var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
            innerTextEditor.PersistentState.ResourceUri.Value,
            false);

        Enqueue(new IdeBackgroundTaskApiWorkArgs
        {
        	WorkKind = IdeBackgroundTaskApiWorkKind.SaveFile,
            AbsolutePath = absolutePath,
            Content = innerContent,
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
                showViewModelArgs.CommonUtilityService,
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
        var fileLastWriteTime = await CommonUtilityService.FileSystemProvider.File
            .GetLastWriteTimeAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        if (fileLastWriteTime > textEditorModel.ResourceLastWriteTime &&
            IdeComponentRenderers.BooleanPromptOrCancelRendererType is not null)
        {
            var notificationInformativeKey = Key<IDynamicViewModel>.NewKey();

            var notificationInformative = new NotificationViewModel(
                notificationInformativeKey,
                "File contents were modified on disk",
                IdeComponentRenderers.BooleanPromptOrCancelRendererType,
                new Dictionary<string, object?>
                {
                        {
                            nameof(IBooleanPromptOrCancelRendererType.Message),
                            "File contents were modified on disk"
                        },
                        {
                            nameof(IBooleanPromptOrCancelRendererType.AcceptOptionTextOverride),
                            "Reload"
                        },
                        {
                            nameof(IBooleanPromptOrCancelRendererType.OnAfterAcceptFunc),
                            new Func<Task>(() =>
                            {
                            	Enqueue(new IdeBackgroundTaskApiWorkArgs
                            	{
                            		WorkKind = IdeBackgroundTaskApiWorkKind.FileContentsWereModifiedOnDisk,
                            		InputFileAbsolutePathString = inputFileAbsolutePathString,
                            		TextEditorModel = textEditorModel,
                            		FileLastWriteTime = fileLastWriteTime,
                            		NotificationInformativeKey = notificationInformativeKey,
                            	});

								return Task.CompletedTask;
							})
                        },
                        {
                            nameof(IBooleanPromptOrCancelRendererType.OnAfterDeclineFunc),
                            new Func<Task>(() =>
                            {
                                CommonUtilityService.Notification_ReduceDisposeAction(notificationInformativeKey);
                                return Task.CompletedTask;
                            })
                        },
                },
                TimeSpan.FromSeconds(20),
                true,
                null);

            CommonUtilityService.Notification_ReduceRegisterAction(notificationInformative);
        }
    }

    private async ValueTask Editor_Do_FileContentsWereModifiedOnDisk(string inputFileAbsolutePathString, TextEditorModel textEditorModel, DateTime fileLastWriteTime, Key<IDynamicViewModel> notificationInformativeKey)
    {
        CommonUtilityService.Notification_ReduceDisposeAction(notificationInformativeKey);

        var content = await CommonUtilityService.FileSystemProvider.File
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

            editContext.TextEditorService.Model_ApplySyntaxHighlighting(
                editContext,
                modelModifier);
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
            await CommonUtilityService.FileSystemProvider.File.ExistsAsync(absolutePathString).ConfigureAwait(false))
        {
            await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(absolutePathString, content).ConfigureAwait(false);
        }
        else
        {
            // TODO: Save As to make new file
            NotificationHelper.DispatchInformative("Save Action", "File not found. TODO: Save As", CommonUtilityService, TimeSpan.FromSeconds(7));
        }

        DateTime? fileLastWriteTime = null;

        if (absolutePathString is not null)
        {
            fileLastWriteTime = await CommonUtilityService.FileSystemProvider.File.GetLastWriteTimeAsync(
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
        
		CommonUtilityService.EnvironmentProvider.DeletionPermittedRegister(new(folderAbsolutePath.Value, true));

        var rootNode = new TreeViewAbsolutePath(
            folderAbsolutePath,
            IdeComponentRenderers,
            CommonUtilityService,
            true,
            true);

        await rootNode.LoadChildListAsync().ConfigureAwait(false);

        if (!CommonUtilityService.TryGetTreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                out var treeViewState))
        {
            CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                new List<TreeViewNoType>() { rootNode }));
        }
        else
        {
            CommonUtilityService.TreeView_WithRootNodeAction(FolderExplorerState.TreeViewContentStateKey, rootNode);

            CommonUtilityService.TreeView_SetActiveNodeAction(
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
        Enqueue(new IdeBackgroundTaskApiWorkArgs
        {
        	WorkKind = IdeBackgroundTaskApiWorkKind.RequestInputFileStateForm,
            Message = "Folder Explorer",
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
            DialogFacts.InputFileDialogKey,
            "Input File",
            IdeComponentRenderers.InputFileRendererType,
            null,
            Walk.Ide.RazorLib.Htmls.Models.HtmlFacts.Classes.DIALOG_PADDING_0,
            true,
            null);

        CommonUtilityService.Dialog_ReduceRegisterAction(inputFileDialog);

        return ValueTask.CompletedTask;
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out IdeBackgroundTaskApiWorkArgs workArgs))
            return ValueTask.CompletedTask;

        switch (workArgs.WorkKind)
        {
            case IdeBackgroundTaskApiWorkKind.WalkIdeInitializerOnInit:
                return Do_WalkIdeInitializerOnInit();
            case IdeBackgroundTaskApiWorkKind.IdeHeaderOnInit:
            	return Do_IdeHeaderOnInit(workArgs.IdeMainLayout);
            case IdeBackgroundTaskApiWorkKind.FileContentsWereModifiedOnDisk:
	            return Editor_Do_FileContentsWereModifiedOnDisk(
	                workArgs.InputFileAbsolutePathString, workArgs.TextEditorModel, workArgs.FileLastWriteTime, workArgs.NotificationInformativeKey);
			case IdeBackgroundTaskApiWorkKind.SaveFile:
                return Do_SaveFile(workArgs.AbsolutePath, workArgs.Content, workArgs.OnAfterSaveCompletedWrittenDateTimeFunc, workArgs.CancellationToken);
            case IdeBackgroundTaskApiWorkKind.SetFolderExplorerState:
                return Do_SetFolderExplorerState(workArgs.AbsolutePath);
            case IdeBackgroundTaskApiWorkKind.SetFolderExplorerTreeView:
                return Do_SetFolderExplorerTreeView(workArgs.AbsolutePath);
			case IdeBackgroundTaskApiWorkKind.RequestInputFileStateForm:
                return Do_RequestInputFileStateForm(
                    workArgs.Message, workArgs.OnAfterSubmitFunc, workArgs.SelectionIsValidFunc, workArgs.InputFilePatterns);
            default:
                Console.WriteLine($"{nameof(IdeBackgroundTaskApi)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
        }
    }
    
    /* Start ITerminalService */
    private readonly object _stateModificationLock = new();

    private TerminalState _terminalState = new();

	public event Action? TerminalStateChanged;

	public TerminalState GetTerminalState() => _terminalState;

    public void Terminal_Register(ITerminal terminal)
    {
        lock (_stateModificationLock)
        {
            if (!_terminalState.TerminalMap.ContainsKey(terminal.Key))
            {
                var nextMap = new Dictionary<Key<ITerminal>, ITerminal>(_terminalState.TerminalMap);
                nextMap.Add(terminal.Key, terminal);
    
                _terminalState = _terminalState with { TerminalMap = nextMap };
            }
        }

        TerminalStateChanged?.Invoke();
    }

    public void Terminal_StateHasChanged()
    {
    	TerminalStateChanged?.Invoke();
    }

    public void Terminal_Dispose(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            var nextMap = new Dictionary<Key<ITerminal>, ITerminal>(_terminalState.TerminalMap);
            nextMap.Remove(terminalKey);

            _terminalState = _terminalState with { TerminalMap = nextMap };
        }

        TerminalStateChanged?.Invoke();
    }
    /* End ITerminalService */
    
    /* Start IInputFileService */
    private InputFileState _inputFileState = new();
	
	public event Action? InputFileStateChanged;
	
	public InputFileState GetInputFileState() => _inputFileState;

    private readonly Queue<InputFileServiceWorkKind> _workKindQueue = new();
    private readonly object _workLock = new();

    public void InputFile_StartInputFileStateForm(
        string message,
        Func<AbsolutePath, Task> onAfterSubmitFunc,
        Func<AbsolutePath, Task<bool>> selectionIsValidFunc,
        List<InputFilePattern> inputFilePatterns)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectionIsValidFunc = selectionIsValidFunc,
                OnAfterSubmitFunc = onAfterSubmitFunc,
                InputFilePatternsList = inputFilePatterns,
                SelectedInputFilePattern = inputFilePatterns.First(),
                Message = message
            };
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_SetSelectedTreeViewModel(TreeViewAbsolutePath? selectedTreeViewModel)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedTreeViewModel = selectedTreeViewModel
            };
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_SetOpenedTreeViewModel(
    	TreeViewAbsolutePath treeViewModel,
        IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService)
    {
        lock (_stateModificationLock)
        {
            if (treeViewModel.Item.IsDirectory)
            {
                _inputFileState = InputFileState.NewOpenedTreeViewModelHistory(
                    _inputFileState,
                    treeViewModel,
                    ideComponentRenderers,
                    commonUtilityService);
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_SetSelectedInputFilePattern(InputFilePattern inputFilePattern)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedInputFilePattern = inputFilePattern
            };
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_MoveBackwardsInHistory()
    {
        lock (_stateModificationLock)
        {
            if (_inputFileState.CanMoveBackwardsInHistory)
            {
                _inputFileState = _inputFileState with { IndexInHistory = _inputFileState.IndexInHistory - 1 };
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_MoveForwardsInHistory()
    {
        lock (_stateModificationLock)
        {
            if (_inputFileState.CanMoveForwardsInHistory)
            {
                _inputFileState = _inputFileState with { IndexInHistory = _inputFileState.IndexInHistory + 1 };
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        InputFileStateChanged?.Invoke();
    }

    public void InputFile_OpenParentDirectory(
        IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            var currentSelection = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            // If has a ParentDirectory select it
            if (currentSelection.Item.ParentDirectory is not null)
            {
                var parentDirectory = currentSelection.Item.ParentDirectory;

                var parentDirectoryAbsolutePath = commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                    parentDirectory,
                    true);

                parentDirectoryTreeViewModel = new TreeViewAbsolutePath(
                    parentDirectoryAbsolutePath,
                    ideComponentRenderers,
                    commonUtilityService,
                    false,
                    true);
            }

            if (parentDirectoryTreeViewModel is not null)
            {
                _inputFileState = InputFileState.NewOpenedTreeViewModelHistory(
                    inState,
                    parentDirectoryTreeViewModel,
                    ideComponentRenderers,
                    commonUtilityService);

                goto finalize;
            }

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    public void InputFile_RefreshCurrentSelection(TreeViewAbsolutePath? currentSelection)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            currentSelection = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    public void InputFile_SetSearchQuery(string searchQuery)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            var openedTreeViewModel = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            foreach (var treeViewModel in openedTreeViewModel.ChildList)
            {
                var treeViewAbsolutePath = (TreeViewAbsolutePath)treeViewModel;

                treeViewModel.IsHidden = !treeViewAbsolutePath.Item.NameWithExtension.Contains(
                    searchQuery,
                    StringComparison.InvariantCultureIgnoreCase);
            }

            _inputFileState = inState with { SearchQuery = searchQuery };

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    private readonly
        Queue<(IIdeComponentRenderers ideComponentRenderers, CommonUtilityService commonUtilityService, TreeViewAbsolutePath? parentDirectoryTreeViewModel)>
        InputFile_queue_OpenParentDirectoryAction = new();

    public void InputFile_Enqueue_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
        {
            lock (_workLock)
            {
                _workKindQueue.Enqueue(InputFileServiceWorkKind.OpenParentDirectoryAction);

                InputFile_queue_OpenParentDirectoryAction.Enqueue((
                    ideComponentRenderers, commonUtilityService, parentDirectoryTreeViewModel));

                commonUtilityService.Continuous_EnqueueGroup(this);
            }
        }
    }
    
    public async ValueTask InputFile_Do_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
            await parentDirectoryTreeViewModel.LoadChildListAsync().ConfigureAwait(false);
    }

    private readonly Queue<(CommonUtilityService, TreeViewAbsolutePath)> InputFile_queue_RefreshCurrentSelectionAction = new();

    public void InputFile_Enqueue_RefreshCurrentSelectionAction(CommonUtilityService commonUtilityService, TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
        {
            currentSelection.ChildList.Clear();

            lock (_workLock)
            {
                _workKindQueue.Enqueue(InputFileServiceWorkKind.RefreshCurrentSelectionAction);
                InputFile_queue_RefreshCurrentSelectionAction.Enqueue((commonUtilityService, currentSelection));
                commonUtilityService.Continuous_EnqueueGroup(this);
            }
        }
    }
    
    public async ValueTask InputFile_Do_RefreshCurrentSelectionAction(TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
            await currentSelection.LoadChildListAsync().ConfigureAwait(false);
    }

    public ValueTask InputFile_HandleEvent()
    {
        InputFileServiceWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case InputFileServiceWorkKind.OpenParentDirectoryAction:
            {
                var args = InputFile_queue_OpenParentDirectoryAction.Dequeue();
                return InputFile_Do_OpenParentDirectoryAction(
                    args.ideComponentRenderers, args.commonUtilityService, args.parentDirectoryTreeViewModel);
            }
            case InputFileServiceWorkKind.RefreshCurrentSelectionAction:
            {
                var args = _queue_RefreshCurrentSelectionAction.Dequeue();
                return InputFile_Do_RefreshCurrentSelectionAction(args.Item2);
            }
            default:
            {
                Console.WriteLine($"{nameof(InputFileService)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
            }
        }
    }
    /* End IInputFileService */
    
    /* Start IFolderExplorerService */
    private FolderExplorerState _folderExplorerState = new();
	
	public event Action? FolderExplorerStateChanged;
	
	public FolderExplorerState GetFolderExplorerState() => _folderExplorerState;

    public void FolderExplorer_With(Func<FolderExplorerState, FolderExplorerState> withFunc)
    {
        lock (_stateModificationLock)
        {
            _folderExplorerState = withFunc.Invoke(_folderExplorerState);
        }

        FolderExplorerStateChanged?.Invoke();
    }
    /* End IFolderExplorerService */
    
    /* Start ICodeSearchService */
	private readonly Throttle CodeSearch_throttle = new(TimeSpan.FromMilliseconds(300));
    
    // Moving things from 'CodeSearchDisplay.razor.cs'
    private Key<TextEditorViewModel> CodeSearch_previousTextEditorViewModelKey = Key<TextEditorViewModel>.Empty;
	public Throttle CodeSearch_updateContentThrottle { get; } = new Throttle(TimeSpan.FromMilliseconds(333));

    private CodeSearchState _codeSearchState = new();
    
    public event Action? CodeSearchStateChanged;
    
    public CodeSearchState GetCodeSearchState() => _codeSearchState;
    
    public void CodeSearch_With(Func<CodeSearchState, CodeSearchState> withFunc)
    {
        lock (_stateModificationLock)
        {
            var outState = withFunc.Invoke(_codeSearchState);

            if (outState.Query.StartsWith("f:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Files
                };
            }
            else if (outState.Query.StartsWith("t:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Types
                };
            }
            else if (outState.Query.StartsWith("m:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Members
                };
            }
            else
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.None
                };
            }

            _codeSearchState = outState;
        }

        CodeSearchStateChanged?.Invoke();
    }

    public void CodeSearch_AddResult(string result)
    {
        lock (_stateModificationLock)
        {
            var outResultList = new List<string>(_codeSearchState.ResultList);
            outResultList.Add(result);

			_codeSearchState = _codeSearchState with
            {
                ResultList = outResultList
			};
        }

        CodeSearchStateChanged?.Invoke();
    }

    public void CodeSearch_ClearResultList()
    {
        lock (_stateModificationLock)
        {
            _codeSearchState = _codeSearchState with
            {
                ResultList = new List<string>()
            };
        }

        CodeSearchStateChanged?.Invoke();
    }
    
    public void CodeSearch_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW)
            {
                if (_codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
    
                if (_codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
                    
                    if (existingDimensionUnit.Purpose is null)
                        _codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
            }
        }

        CodeSearchStateChanged?.Invoke();
    }

    /// <summary>
    /// TODO: This method makes use of <see cref="IThrottle"/> and yet is accessing...
    ///       ...searchEffect.CancellationToken.
    ///       The issue here is that the search effect parameter to this method
    ///       could be out of date by the time that the throttle delay is completed.
    ///       This should be fixed. (2024-05-02)
    /// </summary>
    /// <param name="searchEffect"></param>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    public Task CodeSearch_HandleSearchEffect(CancellationToken cancellationToken = default)
    {
        _throttle.Run(async _ =>
        {
            ClearResultList();

            var codeSearchState = GetCodeSearchState();
            ConstructTreeView(codeSearchState);

            var startingAbsolutePathForSearch = codeSearchState.StartingAbsolutePathForSearch;

            if (string.IsNullOrWhiteSpace(startingAbsolutePathForSearch) ||
            	string.IsNullOrWhiteSpace(codeSearchState.Query))
            {
                return;
            }

            await RecursiveHandleSearchEffect(startingAbsolutePathForSearch).ConfigureAwait(false);
            
            ConstructTreeView(GetCodeSearchState());

            async Task RecursiveHandleSearchEffect(string directoryPathParent)
            {
                var directoryPathChildList = await _commonUtilityService.FileSystemProvider.Directory.GetDirectoriesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                var filePathChildList = await _commonUtilityService.FileSystemProvider.Directory.GetFilesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                foreach (var filePathChild in filePathChildList)
                {
                	var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(filePathChild, false);
                
                    if (absolutePath.NameWithExtension.Contains(codeSearchState.Query))
                        AddResult(filePathChild);
                }

                foreach (var directoryPathChild in directoryPathChildList)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

					if (IFileSystemProvider.IsDirectoryIgnored(directoryPathChild))
						continue;

                    await RecursiveHandleSearchEffect(directoryPathChild).ConfigureAwait(false);
                }
            }
        });
        
        return Task.CompletedTask;
    }
    
    private void CodeSearch_ConstructTreeView(CodeSearchState codeSearchState)
	{
	    var treeViewList = codeSearchState.ResultList.Select(
	    	x => (TreeViewNoType)new TreeViewCodeSearchTextSpan(
		        new TextEditorTextSpan(
		        	0,
			        0,
			        (byte)GenericDecorationKind.None),
			    new AbsolutePath(x, false, _commonUtilityService.EnvironmentProvider),
				_commonUtilityService.EnvironmentProvider,
				_commonUtilityService.FileSystemProvider,
				false,
				false))
			.ToArray();
	
	    var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewList);
	    var firstNode = treeViewList.FirstOrDefault();
	
	    IReadOnlyList<TreeViewNoType> activeNodes = firstNode is null
	        ? Array.Empty<TreeViewNoType>()
	        : new List<TreeViewNoType> { firstNode };
	
	    if (!_commonUtilityService.TryGetTreeViewContainer(CodeSearchState.TreeViewCodeSearchContainerKey, out _))
	    {
	        _commonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
	            CodeSearchState.TreeViewCodeSearchContainerKey,
	            adhocRoot,
	            activeNodes));
	    }
	    else
	    {
	        _commonUtilityService.TreeView_WithRootNodeAction(CodeSearchState.TreeViewCodeSearchContainerKey, adhocRoot);
	
	        _commonUtilityService.TreeView_SetActiveNodeAction(
	            CodeSearchState.TreeViewCodeSearchContainerKey,
	            firstNode,
	            true,
	            false);
	    }
	}
	
	public async Task CodeSearch_UpdateContent(ResourceUri providedResourceUri)
	{
		TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			Console.WriteLine(nameof(UpdateContent));
		
			if (!_commonUtilityService.TryGetTreeViewContainer(
					CodeSearchState.TreeViewCodeSearchContainerKey,
					out var treeViewContainer))
			{
				Console.WriteLine("TryGetTreeViewContainer");
				return;
			}
			
			if (treeViewContainer.SelectedNodeList.Count > 1)
			{
				Console.WriteLine("treeViewContainer.SelectedNodeList.Count > 1");
				return;
			}
				
			var activeNode = treeViewContainer.ActiveNode;
			
			if (activeNode is not TreeViewCodeSearchTextSpan treeViewCodeSearchTextSpan)
			{
				Console.WriteLine("activeNode is not TreeViewCodeSearchTextSpan treeViewCodeSearchTextSpan");
				return;
			}
		
			var inPreviewViewModelKey = GetCodeSearchState().PreviewViewModelKey;
			var outPreviewViewModelKey = Key<TextEditorViewModel>.NewKey();
	
			var filePath = treeViewCodeSearchTextSpan.AbsolutePath.Value;
			var resourceUri = new ResourceUri(treeViewCodeSearchTextSpan.AbsolutePath.Value);
	
	        if (TextEditorService.TextEditorConfig.RegisterModelFunc is null)
	            return;
	
	        await TextEditorService.TextEditorConfig.RegisterModelFunc.Invoke(
	                new RegisterModelArgs(editContext, resourceUri, _commonUtilityService, this))
	            .ConfigureAwait(false);
	
	        if (TextEditorService.TextEditorConfig.TryRegisterViewModelFunc is not null)
	        {
	            var viewModelKey = await TextEditorService.TextEditorConfig.TryRegisterViewModelFunc.Invoke(new TryRegisterViewModelArgs(
	            		editContext,
	                    outPreviewViewModelKey,
	                    resourceUri,
	                    new Category(nameof(CodeSearchService)),
	                    false,
	                    _commonUtilityService,
	                    this))
	                .ConfigureAwait(false);
	
	            if (viewModelKey != Key<TextEditorViewModel>.Empty &&
	                TextEditorService.TextEditorConfig.TryShowViewModelFunc is not null)
	            {
	                With(inState => inState with
	                {
	                    PreviewFilePath = filePath,
	                    PreviewViewModelKey = viewModelKey,
	                });
	
	                if (inPreviewViewModelKey != Key<TextEditorViewModel>.Empty &&
	                    inPreviewViewModelKey != viewModelKey)
					{
						TextEditorService.ViewModel_Dispose(editContext, inPreviewViewModelKey);
					}
	            }
	        }
		});
    }
    /* End ICodeSearchService */
    
    /* Start ICommandFactory */
    private WidgetModel? _contextSwitchWidget;
    private WidgetModel? _commandBarWidget;
    
	public IDialog? CodeSearchDialog { get; set; }

	public void CommandFactory_Initialize()
    {
    	((TextEditorKeymapDefault)TextEditorKeymapFacts.DefaultKeymap).AltF12Func = PeekCodeSearchDialog;
    	
    	// FindAllReferences
    	// ((TextEditorKeymapDefault)TextEditorKeymapFacts.DefaultKeymap).ShiftF12Func = ShowAllReferences;
    
        // ActiveContextsContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "a",
                    Code = "KeyA",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.ActiveContextsContext, "Focus: ActiveContexts", "focus-active-contexts", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // BackgroundServicesContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "b",
                    Code = "KeyB",
                    LayerKey = Key<KeymapLayer>.Empty,

                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.BackgroundServicesContext, "Focus: BackgroundServices", "focus-background-services", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // CompilerServiceExplorerContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "C",
                    Code = "KeyC",
                    LayerKey = Key<KeymapLayer>.Empty,

                    ShiftKey = true,
                	CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.CompilerServiceExplorerContext, "Focus: CompilerServiceExplorer", "focus-compiler-service-explorer", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // CompilerServiceEditorContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "c",
                    Code = "KeyC",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.CompilerServiceEditorContext, "Focus: CompilerServiceEditor", "focus-compiler-service-editor", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // DialogDisplayContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "d",
                    Code = "KeyD",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.DialogDisplayContext, "Focus: DialogDisplay", "focus-dialog-display", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // EditorContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "E",
                    Code = "KeyE",
                    LayerKey = Key<KeymapLayer>.Empty,
                    ShiftKey = true,
                	CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.EditorContext, "Focus: Editor", "focus-editor", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // FolderExplorerContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "f",
                    Code = "KeyF",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.FolderExplorerContext, "Focus: FolderExplorer", "focus-folder-explorer", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // GitContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "g",
                    Code = "KeyG",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.GitContext, "Focus: Git", "focus-git", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // GlobalContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "g",
                    Code = "KeyG",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.GlobalContext, "Focus: Global", "focus-global", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // MainLayoutFooterContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "f",
                    Code = "KeyF",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.MainLayoutFooterContext, "Focus: Footer", "focus-footer", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // MainLayoutHeaderContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "h",
                    Code = "KeyH",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.MainLayoutHeaderContext, "Focus: Header", "focus-header", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // ErrorListContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "e",
                    Code = "KeyE",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.ErrorListContext, "Focus: Error List", "error-list", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // OutputContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "o",
                    Code = "KeyO",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.OutputContext, "Focus: Output", "focus-output", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
		// TerminalContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "t",
                    Code = "KeyT",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.TerminalContext, "Focus: Terminal", "focus-terminal", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // TestExplorerContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "T",
                    Code = "KeyT",
                    LayerKey = Key<KeymapLayer>.Empty,
                    ShiftKey = true,
                	CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.TestExplorerContext, "Focus: Test Explorer", "focus-test-explorer", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }
        // TextEditorContext
        {
            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "t",
                    Code = "KeyT",
                    LayerKey = Key<KeymapLayer>.Empty,
                    CtrlKey = true,
                	AltKey = true,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    ContextFacts.TextEditorContext, "Focus: TextEditor", "focus-text-editor", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
        }

        // Focus the text editor itself (as to allow for typing into the editor)
        {
            var focusTextEditorCommand = new CommonCommand(
                "Focus: Text Editor", "focus-text-editor", false,
                async commandArgs =>
                {
                    var group = TextEditorService.Group_GetOrDefault(EditorTextEditorGroupKey);
                    if (group is null)
                        return;

                    var activeViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);
                    if (activeViewModel is null)
                        return;

					var componentData = activeViewModel.PersistentState.ComponentData;
					if (componentData is not null)
					{
						await _commonUtilityService.JsRuntimeCommonApi
	                        .FocusHtmlElementById(componentData.PrimaryCursorContentId)
	                        .ConfigureAwait(false);
					}
                });

            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
                    new KeymapArgs()
                    {
                        Key = "Escape",
                        Code = "Escape",
                        LayerKey = Key<KeymapLayer>.Empty
                    },
                    focusTextEditorCommand);
        }

		// Add command to bring up a FindAll dialog. Example: { Ctrl + Shift + f }
		{
			var openFindDialogCommand = new CommonCommand(
	            "Open: Find", "open-find", false,
	            commandArgs => 
				{
					TextEditorService.Options_ShowFindAllDialog();
		            return ValueTask.CompletedTask;
				});

            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
	                new KeymapArgs()
	                {
                        Key = "F",
                        Code = "KeyF",
                        LayerKey = Key<KeymapLayer>.Empty,
                        ShiftKey = true,
	                	CtrlKey = true,
	                },
	                openFindDialogCommand);
        }

		// Add command to bring up a CodeSearch dialog. Example: { Ctrl + , }
		{
		    // TODO: determine the actively focused element at time of invocation,
            //       then restore focus to that element when this dialog is closed.
			var openCodeSearchDialogCommand = new CommonCommand(
	            "Open: Code Search", "open-code-search", false,
	            commandArgs => 
				{
                    return OpenCodeSearchDialog();
				});

            _ = ContextFacts.GlobalContext.Keymap.TryRegister(
	                new KeymapArgs()
	                {
                        Key = ",",
                        Code = "Comma",
                        LayerKey = Key<KeymapLayer>.Empty,
                        CtrlKey = true,
	                },
	                openCodeSearchDialogCommand);
        }

		// Add command to bring up a Context Switch dialog. Example: { Ctrl + Tab }
		{
			// TODO: determine the actively focused element at time of invocation,
            //       then restore focus to that element when this dialog is closed.
			var openContextSwitchDialogCommand = new CommonCommand(
	            "Open: Context Switch", "open-context-switch", false,
	            async commandArgs =>
				{
					var elementDimensions = await _commonUtilityService.JsRuntimeCommonApi
						.MeasureElementById("di_ide_header-button-file")
						.ConfigureAwait(false);
						
					var contextState = _commonUtilityService.GetContextState();
					
					var menuOptionList = new List<MenuOptionRecord>();
					
					foreach (var context in contextState.AllContextsList)
			        {
			        	menuOptionList.Add(new MenuOptionRecord(
			        		context.DisplayNameFriendly,
			        		MenuOptionKind.Other));
			        }
					
					MenuRecord menu;
					
					if (menuOptionList.Count == 0)
						menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
					else
						menu = new MenuRecord(menuOptionList);
						
					var dropdownRecord = new DropdownRecord(
						Key<DropdownRecord>.NewKey(),
						elementDimensions.LeftInPixels,
						elementDimensions.TopInPixels + elementDimensions.HeightInPixels,
						typeof(MenuDisplay),
						new Dictionary<string, object?>
						{
							{
								nameof(MenuDisplay.MenuRecord),
								menu
							}
						},
						() => Task.CompletedTask);
			
			        // _dispatcher.Dispatch(new DropdownState.RegisterAction(dropdownRecord));
			        
			        if (_commonUtilityService.GetContextState().FocusedContextKey == ContextFacts.TextEditorContext.ContextKey)
			        {
			        	_commonUtilityService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkTextEditorInitializer.ContextSwitchGroupKey;
			        }
			        else
			        {
			        	_commonUtilityService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkCommonInitializer.ContextSwitchGroupKey;
			        }
				
                    _contextSwitchWidget ??= new WidgetModel(
                        typeof(ContextSwitchDisplay),
                        componentParameterMap: null,
                        cssClass: null,
                        cssStyle: null);

                    _commonUtilityService.SetWidget(_contextSwitchWidget);
				});

			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
					new KeymapArgs()
					{
                        Key = "Tab",
                        Code = "Tab",
                        LayerKey = Key<KeymapLayer>.Empty,
                        CtrlKey = true,
					},
					openContextSwitchDialogCommand);
					
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
					new KeymapArgs()
					{
                        Key = "/",
                        Code = "Slash",
                        LayerKey = Key<KeymapLayer>.Empty,
                        CtrlKey = true,
						AltKey = true,
					},
					openContextSwitchDialogCommand);
		}
		// Command bar
		{
			var openCommandBarCommand = new CommonCommand(
	            "Open: Command Bar", "open-command-bar", false,
	            commandArgs =>
				{
                    _commandBarWidget ??= new WidgetModel(
                        typeof(Walk.Ide.RazorLib.CommandBars.Displays.CommandBarDisplay),
                        componentParameterMap: null,
                        cssClass: null,
                        cssStyle: "width: 80vw; height: 5em; left: 10vw; top: 0;");

                    _commonUtilityService.SetWidget(_commandBarWidget);
                    return ValueTask.CompletedTask;
				});
		
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
					new KeymapArgs()
					{
                        Key = "p",
                        Code = "KeyP",
                        LayerKey = Key<KeymapLayer>.Empty,
                        CtrlKey = true,
					},
					openCommandBarCommand);
		}
    }
    
    public ValueTask CommandFactory_OpenCodeSearchDialog()
    {
    	// Duplicated Code: 'PeekCodeSearchDialog(...)'
    	CodeSearchDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
			"Code Search",
            typeof(CodeSearchDisplay),
            null,
            null,
			true,
			null);

        _commonUtilityService.Dialog_ReduceRegisterAction(CodeSearchDialog);
        
        TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
        	var group = TextEditorService.Group_GetOrDefault(IdeBackgroundTaskApi.EditorTextEditorGroupKey);
            if (group is null)
                return;

            var activeViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);
            if (activeViewModel is null)
                return;
        
            var viewModelModifier = editContext.GetViewModelModifier(activeViewModel.PersistentState.ViewModelKey);
            if (viewModelModifier is null)
                return;

			// If the user has an active text selection,
			// then populate the code search with their selection.
			
			var modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri);

            if (modelModifier is null)
                return;

            var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModelModifier, modelModifier);
			if (selectedText is null)
				return;
			
			_codeSearchService.With(inState => inState with
			{
				Query = selectedText,
			});

			_codeSearchService.HandleSearchEffect();
 	
	 	   // I tried without the Yield and it works fine without it.
	 	   // I'm gonna keep it though so I can sleep at night.
	 	   //
	 	   await Task.Yield();
			await Task.Delay(200).ConfigureAwait(false);
			
			_commonUtilityService.TreeView_MoveHomeAction(
				CodeSearchState.TreeViewCodeSearchContainerKey,
				false,
				false);
        });
        
        return ValueTask.CompletedTask;
    }
    
    public async ValueTask CommandFactory_PeekCodeSearchDialog(TextEditorEditContext editContext, string? resourceUriValue, int? indexInclusiveStart)
    {
    	var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(resourceUriValue, isDirectory: false);
    
    	// Duplicated Code: 'OpenCodeSearchDialog(...)'
    	CodeSearchDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
			"Code Search",
            typeof(CodeSearchDisplay),
            null,
            null,
			true,
			null);

        _commonUtilityService.Dialog_ReduceRegisterAction(CodeSearchDialog);
        
        _codeSearchService.With(inState => inState with
		{
			Query = absolutePath.NameWithExtension,
		});

		await _codeSearchService.HandleSearchEffect().ConfigureAwait(false);
 	
 	   // I tried without the Yield and it works fine without it.
 	   // I'm gonna keep it though so I can sleep at night.
 	   //
 	   await Task.Yield();
		await Task.Delay(200).ConfigureAwait(false);
		
		_commonUtilityService.TreeView_MoveHomeAction(
			CodeSearchState.TreeViewCodeSearchContainerKey,
			false,
			false);
    }
    /* End ICommandFactory */
    
    /* Start ITerminalGroupService */
    private TerminalGroupState _terminalGroupState = new();
	
	public event Action? TerminalGroupStateChanged;
	
	public TerminalGroupState GetTerminalGroupState() => _terminalGroupState;

    public void TerminalGroup_SetActiveTerminal(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            _terminalGroupState = _terminalGroupState with
            {
                ActiveTerminalKey = terminalKey
            };
        }

        TerminalGroupStateChanged?.Invoke();
    }
    
    public void TerminalGroup_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
            {
                if (_terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }

                if (_terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
            }
        }

        TerminalGroupStateChanged?.Invoke();
    }
    /* End ITerminalGroupService */
    
    /* Start IIdeService */
    private IdeState _ideState = new();
    private StartupControlState _startupControlState = new();
    
    public event Action? Ide_IdeStateChanged;
    public event Action? Ide_StartupControlStateChanged;
    
    public IdeState GetIdeState() => _ideState;
    public StartupControlState GetIdeStartupControlState() => _startupControlState;
    
    public void Ide_RegisterFooterBadge(IBadgeModel badgeModel)
	{
		lock (_stateModificationLock)
		{
			var existingComponent = _ideState.FooterBadgeList.FirstOrDefault(x =>
				x.Key == badgeModel.Key);

			if (existingComponent is null)
            {
    			var outFooterBadgeList = new List<IBadgeModel>(_ideState.FooterBadgeList);
    			outFooterBadgeList.Add(badgeModel);
    
    			_ideState = _ideState with
    			{
    				FooterBadgeList = outFooterBadgeList
    			};
    	    }
		}

        IdeStateChanged?.Invoke();
    }
    
    public void Ide_SetMenuFile(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menu
			};
		}
    }
	
	public void Ide_SetMenuTools(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menu
			};
        }
    }
	
	public void Ide_SetMenuView(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menu
			};
        }
    }
	
	public void Ide_SetMenuRun(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menu
			};
        }
    }
	
	public void Ide_ModifyMenuFile(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menuFunc.Invoke(_ideState.MenuFile)
			};
        }
    }
	
	public void Ide_ModifyMenuTools(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menuFunc.Invoke(_ideState.MenuTools)
			};
        }
    }

	public void Ide_ModifyMenuView(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menuFunc.Invoke(_ideState.MenuView)
			};
        }
    }
	
	public void Ide_ModifyMenuRun(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menuFunc.Invoke(_ideState.MenuRun)
			};
        }
    }

	public void Ide_RegisterStartupControl(IStartupControlModel startupControl)
	{
		lock (_stateModificationLock)
		{
			var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
				x => x.Key == startupControl.Key);

			if (indexOfStartupControl == -1)
			{
    			var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
    			outStartupControlList.Add(startupControl);
    
    			_startupControlState = _startupControlState with
    			{
    				StartupControlList = outStartupControlList
    			};
    	    }
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void Ide_DisposeStartupControl(Key<IStartupControlModel> startupControlKey)
	{
		lock (_stateModificationLock)
		{
			var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
				x => x.Key == startupControlKey);

			if (indexOfStartupControl != -1)
            {
                var outActiveStartupControlKey = _startupControlState.ActiveStartupControlKey;
    			if (_startupControlState.ActiveStartupControlKey == startupControlKey)
    				outActiveStartupControlKey = Key<IStartupControlModel>.Empty;
    
    			var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
    			outStartupControlList.RemoveAt(indexOfStartupControl);
    
    			_startupControlState = _startupControlState with
    			{
    				StartupControlList = outStartupControlList,
    				ActiveStartupControlKey = outActiveStartupControlKey
    			};
            }
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void Ide_SetActiveStartupControlKey(Key<IStartupControlModel> startupControlKey)
	{
		lock (_stateModificationLock)
		{
			var startupControl = _startupControlState.StartupControlList.FirstOrDefault(
				x => x.Key == startupControlKey);

			if (startupControlKey == Key<IStartupControlModel>.Empty ||
				startupControl is null)
			{
				_startupControlState = _startupControlState with
				{
					ActiveStartupControlKey = Key<IStartupControlModel>.Empty
				};
            }
            else
            {
    			_startupControlState = _startupControlState with
    			{
    				ActiveStartupControlKey = startupControl.Key
    			};
			}
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void Ide_TriggerStartupControlStateChanged()
	{
		StartupControlStateChanged?.Invoke();
	}
    /* End IIdeService */
    
    /* Start MenuOptionsFactory */
    private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;

    private readonly Queue<MenuOptionsFactoryWorkKind> _workKindQueue = new();

    public MenuOptionRecord NewEmptyFile(AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Empty File", MenuOptionKind.Create,
            widgetRendererType: _ideComponentRenderers.FileFormRendererType,
            widgetParameterMap: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.CheckForTemplates), false },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitFunc),
                    new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
						{
                            Enqueue_PerformNewFile(
                                fileName,
                                exactMatchFileTemplate,
                                relatedMatchFileTemplates,
                                new NamespacePath(string.Empty, parentDirectory),
                                onAfterCompletion);

							return Task.CompletedTask;
						})
                },
            });
    }

    public MenuOptionRecord NewTemplatedFile(NamespacePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Templated File", MenuOptionKind.Create,
            widgetRendererType: _ideComponentRenderers.FileFormRendererType,
            widgetParameterMap: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.CheckForTemplates), true },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitFunc),
                    new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
                        (fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
						{
                            Enqueue_PerformNewFile(
                                fileName,
                                exactMatchFileTemplate,
                                relatedMatchFileTemplates,
                                parentDirectory,
                                onAfterCompletion);

							return Task.CompletedTask;
						})
                },
            });
    }

    public MenuOptionRecord NewDirectory(AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("New Directory", MenuOptionKind.Create,
            widgetRendererType: _ideComponentRenderers.FileFormRendererType,
            widgetParameterMap: new Dictionary<string, object?>
            {
                { nameof(IFileFormRendererType.FileName), string.Empty },
                { nameof(IFileFormRendererType.IsDirectory), true },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitFunc),
                    new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
                        (directoryName, _, _) =>
						{
                            Enqueue_PerformNewDirectory(directoryName, parentDirectory, onAfterCompletion);
							return Task.CompletedTask;
						})
                },
            });
    }

    public MenuOptionRecord DeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Delete", MenuOptionKind.Delete,
            widgetRendererType: _ideComponentRenderers.DeleteFileFormRendererType,
            widgetParameterMap: new Dictionary<string, object?>
            {
                { nameof(IDeleteFileFormRendererType.AbsolutePath), absolutePath },
                { nameof(IDeleteFileFormRendererType.IsDirectory), true },
                {
                    nameof(IDeleteFileFormRendererType.OnAfterSubmitFunc),
                    new Func<AbsolutePath, Task>(
						x => 
						{
							Enqueue_PerformDeleteFile(x, onAfterCompletion);
							return Task.CompletedTask;
						})
                },
            });
    }

    public MenuOptionRecord RenameFile(AbsolutePath sourceAbsolutePath, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Rename", MenuOptionKind.Update,
            widgetRendererType: _ideComponentRenderers.FileFormRendererType,
            widgetParameterMap: new Dictionary<string, object?>
            {
                {
                    nameof(IFileFormRendererType.FileName),
                    sourceAbsolutePath.IsDirectory
                        ? sourceAbsolutePath.NameNoExtension
                        : sourceAbsolutePath.NameWithExtension
                },
                { nameof(IFileFormRendererType.IsDirectory), sourceAbsolutePath.IsDirectory },
                {
                    nameof(IFileFormRendererType.OnAfterSubmitFunc),
                    new Func<string, IFileTemplate?, List<IFileTemplate>, Task>((nextName, _, _) =>
                    {
                        PerformRename(sourceAbsolutePath, nextName, commonUtilityService, onAfterCompletion);
                        return Task.CompletedTask;
                    })
                },
            });
    }

    public MenuOptionRecord CopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Copy", MenuOptionKind.Update,
            onClickFunc: () =>
			{
                Enqueue_PerformCopyFile(absolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
    }

    public MenuOptionRecord CutFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Cut", MenuOptionKind.Update,
            onClickFunc: () =>
			{
                Enqueue_PerformCutFile(absolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
    }

    public MenuOptionRecord PasteClipboard(AbsolutePath directoryAbsolutePath, Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Paste", MenuOptionKind.Update,
            onClickFunc: () =>
			{
                Enqueue_PerformPasteFile(directoryAbsolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
    }

    private readonly
        Queue<(string fileName, IFileTemplate? exactMatchFileTemplate, List<IFileTemplate> relatedMatchFileTemplatesList, NamespacePath namespacePath, Func<Task> onAfterCompletion)>
        _queue_PerformNewFile = new();

    private void Enqueue_PerformNewFile(
        string fileName,
        IFileTemplate? exactMatchFileTemplate,
        List<IFileTemplate> relatedMatchFileTemplatesList,
        NamespacePath namespacePath,
        Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformNewFile);

            _queue_PerformNewFile.Enqueue((
                fileName,
                exactMatchFileTemplate,
                relatedMatchFileTemplatesList,
                namespacePath,
                onAfterCompletion));

            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    private async ValueTask Do_PerformNewFile(
        string fileName,
        IFileTemplate? exactMatchFileTemplate,
        List<IFileTemplate> relatedMatchFileTemplatesList,
        NamespacePath namespacePath,
        Func<Task> onAfterCompletion)
    {
        if (exactMatchFileTemplate is null)
        {
            var emptyFileAbsolutePathString = namespacePath.AbsolutePath.Value + fileName;

            var emptyFileAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                emptyFileAbsolutePathString,
                false);

            await _commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                    emptyFileAbsolutePath.Value,
                    string.Empty,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
        else
        {
            var allTemplates = new[] { exactMatchFileTemplate }
                .Union(relatedMatchFileTemplatesList)
                .ToArray();

            foreach (var fileTemplate in allTemplates)
            {
                var templateResult = fileTemplate.ConstructFileContents.Invoke(
                    new FileTemplateParameter(fileName, namespacePath, _commonUtilityService.EnvironmentProvider));

                await _commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                        templateResult.FileNamespacePath.AbsolutePath.Value,
                        templateResult.Contents,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        await onAfterCompletion.Invoke().ConfigureAwait(false);
    }

    private readonly
        Queue<(string directoryName, AbsolutePath parentDirectory, Func<Task> onAfterCompletion)>
        _queue_PerformNewDirectory = new();

    private void Enqueue_PerformNewDirectory(string directoryName, AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformNewDirectory);
            _queue_PerformNewDirectory.Enqueue((directoryName, parentDirectory, onAfterCompletion));
            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    private async ValueTask Do_PerformNewDirectory(string directoryName, AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
    {
        var directoryAbsolutePathString = parentDirectory.Value + directoryName;
        var directoryAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(directoryAbsolutePathString, true);

        await _commonUtilityService.FileSystemProvider.Directory.CreateDirectoryAsync(
                directoryAbsolutePath.Value,
                CancellationToken.None)
            .ConfigureAwait(false);

        await onAfterCompletion.Invoke().ConfigureAwait(false);
    }

    private readonly
        Queue<(AbsolutePath absolutePath, Func<Task> onAfterCompletion)>
        _queue_general_AbsolutePath_FuncTask = new();

    private void Enqueue_PerformDeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformDeleteFile);
            _queue_general_AbsolutePath_FuncTask.Enqueue((absolutePath, onAfterCompletion));
            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    private async ValueTask Do_PerformDeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        if (absolutePath.IsDirectory)
        {
            await _commonUtilityService.FileSystemProvider.Directory
                .DeleteAsync(absolutePath.Value, true, CancellationToken.None)
                .ConfigureAwait(false);
        }
        else
        {
            await _commonUtilityService.FileSystemProvider.File
                .DeleteAsync(absolutePath.Value)
                .ConfigureAwait(false);
        }

        await onAfterCompletion.Invoke().ConfigureAwait(false);
    }

    private void Enqueue_PerformCopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformCopyFile);
            _queue_general_AbsolutePath_FuncTask.Enqueue((absolutePath, onAfterCompletion));
            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }

    private async ValueTask Do_PerformCopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
    {
        await _commonUtilityService.SetClipboard(ClipboardFacts.FormatPhrase(
                ClipboardFacts.CopyCommand,
                ClipboardFacts.AbsolutePathDataType,
                absolutePath.Value))
            .ConfigureAwait(false);

        await onAfterCompletion.Invoke().ConfigureAwait(false);
    }

    private void Enqueue_PerformCutFile(
        AbsolutePath absolutePath,
        Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformCutFile);
            _queue_general_AbsolutePath_FuncTask.Enqueue((absolutePath, onAfterCompletion));
            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    private async ValueTask Do_PerformCutFile(
        AbsolutePath absolutePath,
        Func<Task> onAfterCompletion)
    {
        await _commonUtilityService.SetClipboard(ClipboardFacts.FormatPhrase(
                ClipboardFacts.CutCommand,
                ClipboardFacts.AbsolutePathDataType,
                absolutePath.Value))
            .ConfigureAwait(false);

        await onAfterCompletion.Invoke().ConfigureAwait(false);
    }

    private void Enqueue_PerformPasteFile(AbsolutePath receivingDirectory, Func<Task> onAfterCompletion)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(MenuOptionsFactoryWorkKind.PerformPasteFile);
            _queue_general_AbsolutePath_FuncTask.Enqueue((receivingDirectory, onAfterCompletion));
            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    private async ValueTask Do_PerformPasteFile(AbsolutePath receivingDirectory, Func<Task> onAfterCompletion)
    {
        var clipboardContents = await _commonUtilityService.ReadClipboard().ConfigureAwait(false);

        if (ClipboardFacts.TryParseString(clipboardContents, out var clipboardPhrase))
        {
            if (clipboardPhrase is not null &&
                clipboardPhrase.DataType == ClipboardFacts.AbsolutePathDataType)
            {
                if (clipboardPhrase.Command == ClipboardFacts.CopyCommand ||
                    clipboardPhrase.Command == ClipboardFacts.CutCommand)
                {
                    AbsolutePath clipboardAbsolutePath = default;

                    // Should the if and else if be kept as inline awaits?
                    // If kept as inline awaits then the else if won't execute if the first one succeeds.
                    if (await _commonUtilityService.FileSystemProvider.Directory.ExistsAsync(clipboardPhrase.Value).ConfigureAwait(false))
                    {
                        clipboardAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                            clipboardPhrase.Value,
                            true);
                    }
                    else if (await _commonUtilityService.FileSystemProvider.File.ExistsAsync(clipboardPhrase.Value).ConfigureAwait(false))
                    {
                        clipboardAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                            clipboardPhrase.Value,
                            false);
                    }

                    if (clipboardAbsolutePath.ExactInput is not null)
                    {
                        var successfullyPasted = true;

                        try
                        {
                            if (clipboardAbsolutePath.IsDirectory)
                            {
                                var clipboardDirectoryInfo = new DirectoryInfo(clipboardAbsolutePath.Value);
                                var receivingDirectoryInfo = new DirectoryInfo(receivingDirectory.Value);

                                CopyFilesRecursively(clipboardDirectoryInfo, receivingDirectoryInfo);
                            }
                            else
                            {
                                var destinationAbsolutePathString = receivingDirectory.Value +
                                    clipboardAbsolutePath.NameWithExtension;

                                var sourceAbsolutePathString = clipboardAbsolutePath.Value;

                                await _commonUtilityService.FileSystemProvider.File.CopyAsync(
                                        sourceAbsolutePathString,
                                        destinationAbsolutePathString)
                                    .ConfigureAwait(false);
                            }
                        }
                        catch (Exception)
                        {
                            successfullyPasted = false;
                        }

                        if (successfullyPasted && clipboardPhrase.Command == ClipboardFacts.CutCommand)
                        {
                            // TODO: Rerender the parent of the deleted due to cut file
                            Enqueue_PerformDeleteFile(clipboardAbsolutePath, onAfterCompletion);
                        }
                        else
                        {
                            await onAfterCompletion.Invoke().ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    private AbsolutePath PerformRename(AbsolutePath sourceAbsolutePath, string nextName, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)
    {
        // Check if the current and next name match when compared with case insensitivity
        if (0 == string.Compare(sourceAbsolutePath.NameWithExtension, nextName, StringComparison.OrdinalIgnoreCase))
        {
            var temporaryNextName = _commonUtilityService.EnvironmentProvider.GetRandomFileName();

            var temporaryRenameResult = PerformRename(
                sourceAbsolutePath,
                temporaryNextName,
                commonUtilityService,
                () => Task.CompletedTask);

            if (temporaryRenameResult.ExactInput is null)
            {
                onAfterCompletion.Invoke();
                return default;
            }
            else
            {
                sourceAbsolutePath = temporaryRenameResult;
            }
        }

        var sourceAbsolutePathString = sourceAbsolutePath.Value;
        var parentOfSource = sourceAbsolutePath.ParentDirectory;
        var destinationAbsolutePathString = parentOfSource + nextName;

        try
        {
            if (sourceAbsolutePath.IsDirectory)
                _commonUtilityService.FileSystemProvider.Directory.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
            else
                _commonUtilityService.FileSystemProvider.File.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
        }
        catch (Exception e)
        {
            NotificationHelper.DispatchError("Rename Action", e.Message, commonUtilityService, TimeSpan.FromSeconds(14));
            onAfterCompletion.Invoke();
            return default;
        }

        onAfterCompletion.Invoke();

        return _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(destinationAbsolutePathString, sourceAbsolutePath.IsDirectory);
    }

    /// <summary>
    /// Looking into copying and pasting a directory
    /// https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    /// </summary>
    public static DirectoryInfo CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        var newDirectoryInfo = target.CreateSubdirectory(source.Name);
        foreach (var fileInfo in source.GetFiles())
            fileInfo.CopyTo(Path.Combine(newDirectoryInfo.FullName, fileInfo.Name));

        foreach (var childDirectoryInfo in source.GetDirectories())
            CopyFilesRecursively(childDirectoryInfo, newDirectoryInfo);

        return newDirectoryInfo;
    }

    public ValueTask HandleEvent()
    {
        MenuOptionsFactoryWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case MenuOptionsFactoryWorkKind.PerformNewFile:
            {
                var args = _queue_PerformNewFile.Dequeue();
                return Do_PerformNewFile(
                    args.fileName, args.exactMatchFileTemplate, args.relatedMatchFileTemplatesList, args.namespacePath, args.onAfterCompletion);
            }
            case MenuOptionsFactoryWorkKind.PerformNewDirectory:
            {
                var args = _queue_PerformNewDirectory.Dequeue();
                return Do_PerformNewDirectory(
                    args.directoryName, args.parentDirectory, args.onAfterCompletion);
            }
            case MenuOptionsFactoryWorkKind.PerformDeleteFile:
            {
                var args = _queue_general_AbsolutePath_FuncTask.Dequeue();
                return Do_PerformDeleteFile(
                    args.absolutePath, args.onAfterCompletion);
            }
            case MenuOptionsFactoryWorkKind.PerformCopyFile:
            {
                var args = _queue_general_AbsolutePath_FuncTask.Dequeue();
                return Do_PerformCopyFile(
                    args.absolutePath, args.onAfterCompletion);
            }
            case MenuOptionsFactoryWorkKind.PerformCutFile:
            {
                var args = _queue_general_AbsolutePath_FuncTask.Dequeue();
                return Do_PerformCutFile(
                    args.absolutePath, args.onAfterCompletion);
            }
            case MenuOptionsFactoryWorkKind.PerformPasteFile:
            {
                var args = _queue_general_AbsolutePath_FuncTask.Dequeue();
                return Do_PerformPasteFile(
                    args.absolutePath, args.onAfterCompletion);
            }
            default:
            {
                Console.WriteLine($"{nameof(MenuOptionsFactory)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
            }
        }
    }
    /* End MenuOptionsFactory */
    
    /* Start CommandBar */
    namespace Walk.Ide.RazorLib.CommandBars.Models;

	private CommandBarState _commandBarState = new();

	public event Action? CommandBarStateChanged;
	
	public CommandBarState GetCommandBarState() => _commandBarState;
    /* End CommandBar */
}
