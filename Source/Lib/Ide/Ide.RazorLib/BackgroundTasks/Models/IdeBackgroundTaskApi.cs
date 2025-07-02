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
using Walk.Common.RazorLib.Options.Models;
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
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Ide.RazorLib.Commands;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Terminals.Displays;
using Walk.Ide.RazorLib.Shareds.Displays;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;

namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public class IdeBackgroundTaskApi : IBackgroundTaskGroup
{
	public static readonly Key<TextEditorGroup> EditorTextEditorGroupKey = Key<TextEditorGroup>.NewKey();

    private readonly IIdeComponentRenderers _ideComponentRenderers;
    private readonly TextEditorService _textEditorService;
    private readonly ICompilerServiceRegistry _compilerServiceRegistry;
    private readonly ITerminalService _terminalService;
	private readonly IDecorationMapperRegistry _decorationMapperRegistry;
	private readonly IInputFileService _inputFileService;
	private readonly IFolderExplorerService _folderExplorerService;
	private readonly ICodeSearchService _codeSearchService;
	private readonly WalkTextEditorConfig _textEditorConfig;
	private readonly CommonUtilityService _commonUtilityService;
	private readonly ICommandFactory _commandFactory;
	private readonly ITerminalGroupService _terminalGroupService;
	private readonly IIdeService _ideService;
	private readonly IServiceProvider _serviceProvider;

    public IdeBackgroundTaskApi(
        ICompilerServiceRegistry compilerServiceRegistry,
        IIdeComponentRenderers ideComponentRenderers,
        TextEditorService textEditorService,
        ITerminalService terminalService,
        IDecorationMapperRegistry decorationMapperRegistry,
        IInputFileService inputFileService,
        IFolderExplorerService folderExplorerService,
        ICodeSearchService codeSearchService,
        WalkTextEditorConfig textEditorConfig,
        CommonUtilityService commonUtilityService,
        ICommandFactory commandFactory,
        ITerminalGroupService terminalGroupService,
        IIdeService ideService,
        IServiceProvider serviceProvider)
    {
        _ideComponentRenderers = ideComponentRenderers;
        _textEditorService = textEditorService;
        _compilerServiceRegistry = compilerServiceRegistry;
        _terminalService = terminalService;
		_decorationMapperRegistry = decorationMapperRegistry;
		_inputFileService = inputFileService;
		_folderExplorerService = folderExplorerService;
        _codeSearchService = codeSearchService;
        _textEditorConfig = textEditorConfig;
        _commonUtilityService = commonUtilityService;
        _commandFactory = commandFactory;
        _terminalGroupService = terminalGroupService;
        _ideService = ideService;
        _serviceProvider = serviceProvider;
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly ConcurrentQueue<IdeBackgroundTaskApiWorkArgs> _workQueue = new();

    private static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();

    public void Enqueue(IdeBackgroundTaskApiWorkArgs workArgs)
    {
        _workQueue.Enqueue(workArgs);
        _commonUtilityService.Continuous_EnqueueGroup(this);
    }

    public ValueTask Do_WalkIdeInitializerOnInit()
    {
        AddExecutionTerminal();
        AddGeneralTerminal();

        _codeSearchService.InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => _commonUtilityService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW));

        InitializePanelResizeHandleDimensionUnit();
        InitializePanelTabs();
        _commandFactory.Initialize();

        return ValueTask.CompletedTask;
    }

    private void InitializePanelResizeHandleDimensionUnit()
    {
        // Left
        {
            var leftPanel = PanelFacts.GetTopLeftPanelGroup(_commonUtilityService.GetPanelState());
            leftPanel.CommonUtilityService = _commonUtilityService;

            _commonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                leftPanel.Key,
                new DimensionUnit(
                    () => _commonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                    DimensionUnitKind.Pixels,
                    DimensionOperatorKind.Subtract,
                    DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));
        }

        // Right
        {
            var rightPanel = PanelFacts.GetTopRightPanelGroup(_commonUtilityService.GetPanelState());
            rightPanel.CommonUtilityService = _commonUtilityService;

            _commonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                rightPanel.Key,
                new DimensionUnit(
                    () => _commonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                    DimensionUnitKind.Pixels,
                    DimensionOperatorKind.Subtract,
                    DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));
        }

        // Bottom
        {
            var bottomPanel = PanelFacts.GetBottomPanelGroup(_commonUtilityService.GetPanelState());
            bottomPanel.CommonUtilityService = _commonUtilityService;

            _commonUtilityService.Panel_InitializeResizeHandleDimensionUnit(
                bottomPanel.Key,
                new DimensionUnit(
                    () => _commonUtilityService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
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
        var leftPanel = PanelFacts.GetTopLeftPanelGroup(_commonUtilityService.GetPanelState());
        leftPanel.CommonUtilityService = _commonUtilityService;

        // folderExplorerPanel
        var folderExplorerPanel = new Panel(
            "Folder Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.FolderExplorerContext.ContextKey,
            typeof(FolderExplorerDisplay),
            null,
            _commonUtilityService);
        _commonUtilityService.RegisterPanel(folderExplorerPanel);
        _commonUtilityService.RegisterPanelTab(leftPanel.Key, folderExplorerPanel, false);

        // SetActivePanelTabAction
        _commonUtilityService.SetActivePanelTab(leftPanel.Key, folderExplorerPanel.Key);
    }

    private void InitializeRightPanelTabs()
    {
        var rightPanel = PanelFacts.GetTopRightPanelGroup(_commonUtilityService.GetPanelState());
        rightPanel.CommonUtilityService = _commonUtilityService;
    }

    private void InitializeBottomPanelTabs()
    {
        var bottomPanel = PanelFacts.GetBottomPanelGroup(_commonUtilityService.GetPanelState());
        bottomPanel.CommonUtilityService = _commonUtilityService;

        // terminalGroupPanel
        var terminalGroupPanel = new Panel(
            "Terminal",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.TerminalContext.ContextKey,
            typeof(TerminalGroupDisplay),
            null,
            _commonUtilityService);
        _commonUtilityService.RegisterPanel(terminalGroupPanel);
        _commonUtilityService.RegisterPanelTab(bottomPanel.Key, terminalGroupPanel, false);
        // This UI has resizable parts that need to be initialized.
        _terminalGroupService.InitializeResizeHandleDimensionUnit(
            new DimensionUnit(
                () => _commonUtilityService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));

        // SetActivePanelTabAction
        //_panelService.SetActivePanelTab(bottomPanel.Key, terminalGroupPanel.Key);
    }

    private void AddGeneralTerminal()
    {
        if (_commonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            _commonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
        {
            _terminalService.Register(
                new TerminalWebsite(
                    "General",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            _textEditorService,
                            _compilerServiceRegistry,
                            _commonUtilityService)),
                    _commonUtilityService)
                {
                    Key = TerminalFacts.GENERAL_KEY
                });
        }
        else
        {
            _terminalService.Register(
                new Terminal(
                    "General",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            _textEditorService,
                            _compilerServiceRegistry,
                            _commonUtilityService)),
                    _commonUtilityService,
                    _terminalService)
                {
                    Key = TerminalFacts.GENERAL_KEY
                });
        }
    }

    private void AddExecutionTerminal()
    {
        if (_commonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Wasm ||
            _commonUtilityService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.ServerSide)
        {
            _terminalService.Register(
                new TerminalWebsite(
                    "Execution",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            _textEditorService,
                            _compilerServiceRegistry,
                            _commonUtilityService)),
                    _commonUtilityService)
                {
                    Key = TerminalFacts.EXECUTION_KEY
                });
        }
        else
        {
            _terminalService.Register(
                new Terminal(
                    "Execution",
                    terminal => new TerminalInteractive(terminal),
                    terminal => new TerminalInputStringBuilder(terminal),
                    terminal => new TerminalOutput(
                        terminal,
                        new TerminalOutputFormatterExpand(
                            terminal,
                            _textEditorService,
                            _compilerServiceRegistry,
                            _commonUtilityService)),
                    _commonUtilityService,
                    _terminalService)
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

        _ideService.SetMenuFile(new MenuRecord(menuOptionsList));
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
                    _textEditorService.OptionsApi.ShowFindAllDialog();
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
                    _commandFactory.CodeSearchDialog ??= new DialogViewModel(
                        Key<IDynamicViewModel>.NewKey(),
                        "Code Search",
                        typeof(CodeSearchDisplay),
                        null,
                        null,
                        true,
                        null);

                    _commonUtilityService.Dialog_ReduceRegisterAction(_commandFactory.CodeSearchDialog);
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

        _ideService.SetMenuTools(new MenuRecord(menuOptionsList));
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

        _commonUtilityService.Dialog_ReduceRegisterAction(dialogRecord);
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
				_commonUtilityService.EnvironmentProvider.DeletionPermittedRegister(new(absolutePath.Value, isDirectory: true));
            
            	_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
				{
					await _textEditorService.OpenInEditorAsync(
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
			
		var uniqueTextEditorWork = new UniqueTextEditorWork(_textEditorService, editContext =>
			compilerService.FastParseAsync(editContext, fastParseArgs.ResourceUri, _fileSystemProvider));
		
		_textEditorService.WorkerArbitrary.EnqueueUniqueTextEditorWork(uniqueTextEditorWork);*/
    }
    
    public async Task Editor_RegisterModelFunc(RegisterModelArgs registerModelArgs)
    {
        var model = _textEditorService.ModelApi.GetOrDefault(registerModelArgs.ResourceUri);
        
        if (model is not null)
        {
        	await Editor_CheckIfContentsWereModifiedAsync(
	                registerModelArgs.ResourceUri.Value,
	                model)
	            .ConfigureAwait(false);
	        return;
        }
			
    	var resourceUri = registerModelArgs.ResourceUri;

        var fileLastWriteTime = await _commonUtilityService.FileSystemProvider.File
            .GetLastWriteTimeAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var content = await _commonUtilityService.FileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(resourceUri.Value, false);
        var decorationMapper = _decorationMapperRegistry.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
        var compilerService = _compilerServiceRegistry.GetCompilerService(absolutePath.ExtensionNoPeriod);

        model = new TextEditorModel(
            resourceUri,
            fileLastWriteTime,
            absolutePath.ExtensionNoPeriod,
            content,
            decorationMapper,
            compilerService,
            _textEditorService);
            
        var modelModifier = new TextEditorModel(model);
        modelModifier.PerformRegisterPresentationModelAction(CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(FindOverlayPresentationFacts.EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(DiffPresentationFacts.EmptyInPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(DiffPresentationFacts.EmptyOutPresentationModel);
        
        model = modelModifier;

        _textEditorService.ModelApi.RegisterCustom(registerModelArgs.EditContext, model);
        
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
    	
		var model = _textEditorService.ModelApi.GetOrDefault(registerViewModelArgs.ResourceUri);

        if (model is null)
        {
        	NotificationHelper.DispatchDebugMessage(nameof(Editor_TryRegisterViewModelFunc), () => "model is null: " + registerViewModelArgs.ResourceUri.Value, _commonUtilityService, TimeSpan.FromSeconds(4));
            return Key<TextEditorViewModel>.Empty;
        }

        var viewModel = _textEditorService.ModelApi
            .GetViewModelsOrEmpty(registerViewModelArgs.ResourceUri)
            .FirstOrDefault(x => x.PersistentState.Category == registerViewModelArgs.Category);

        if (viewModel is not null)
		    return viewModel.PersistentState.ViewModelKey;

        viewModel = new TextEditorViewModel(
            viewModelKey,
            registerViewModelArgs.ResourceUri,
            _textEditorService,
            _commonUtilityService,
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

        var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
            registerViewModelArgs.ResourceUri.Value,
            false);

        viewModel.PersistentState.OnSaveRequested = Editor_HandleOnSaveRequested;
        viewModel.PersistentState.GetTabDisplayNameFunc = _ => absolutePath.NameWithExtension;
        viewModel.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;
        
        _textEditorService.ViewModelApi.Register(registerViewModelArgs.EditContext, viewModel);
        return viewModelKey;
    }
    
    private void Editor_HandleOnSaveRequested(TextEditorModel innerTextEditor)
    {
        var innerContent = innerTextEditor.GetAllText_WithOriginalLineEndings();
        
        var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
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
                    _textEditorService.WorkerArbitrary.PostUnique(editContext =>
                    {
                    	var modelModifier = editContext.GetModelModifier(innerTextEditor.PersistentState.ResourceUri);
                    	if (modelModifier is null)
                    		return ValueTask.CompletedTask;
                    
                    	_textEditorService.ModelApi.SetResourceData(
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
        _textEditorService.GroupApi.Register(EditorTextEditorGroupKey);

        var viewModel = _textEditorService.ViewModelApi.GetOrDefault(showViewModelArgs.ViewModelKey);

        if (viewModel is null)
            return false;

        if (viewModel.PersistentState.Category == new Category("main") &&
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            showViewModelArgs = new TryShowViewModelArgs(
                showViewModelArgs.ViewModelKey,
                EditorTextEditorGroupKey,
                showViewModelArgs.ShouldSetFocusToEditor,
                showViewModelArgs.ServiceProvider);
        }

        if (showViewModelArgs.ViewModelKey == Key<TextEditorViewModel>.Empty ||
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            return false;
        }

        _textEditorService.GroupApi.AddViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);

        _textEditorService.GroupApi.SetActiveViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);
            
        if (showViewModelArgs.ShouldSetFocusToEditor)
        {
        	_textEditorService.WorkerArbitrary.PostUnique(editContext =>
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
        var fileLastWriteTime = await _commonUtilityService.FileSystemProvider.File
            .GetLastWriteTimeAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        if (fileLastWriteTime > textEditorModel.ResourceLastWriteTime &&
            _ideComponentRenderers.BooleanPromptOrCancelRendererType is not null)
        {
            var notificationInformativeKey = Key<IDynamicViewModel>.NewKey();

            var notificationInformative = new NotificationViewModel(
                notificationInformativeKey,
                "File contents were modified on disk",
                _ideComponentRenderers.BooleanPromptOrCancelRendererType,
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
                                _commonUtilityService.Notification_ReduceDisposeAction(notificationInformativeKey);
                                return Task.CompletedTask;
                            })
                        },
                },
                TimeSpan.FromSeconds(20),
                true,
                null);

            _commonUtilityService.Notification_ReduceRegisterAction(notificationInformative);
        }
    }

    private async ValueTask Editor_Do_FileContentsWereModifiedOnDisk(string inputFileAbsolutePathString, TextEditorModel textEditorModel, DateTime fileLastWriteTime, Key<IDynamicViewModel> notificationInformativeKey)
    {
        _commonUtilityService.Notification_ReduceDisposeAction(notificationInformativeKey);

        var content = await _commonUtilityService.FileSystemProvider.File
            .ReadAllTextAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        _textEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(textEditorModel.PersistentState.ResourceUri);
            if (modelModifier is null)
                return ValueTask.CompletedTask;

            _textEditorService.ModelApi.Reload(
                editContext,
                modelModifier,
                content,
                fileLastWriteTime);

            editContext.TextEditorService.ModelApi.ApplySyntaxHighlighting(
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
            await _commonUtilityService.FileSystemProvider.File.ExistsAsync(absolutePathString).ConfigureAwait(false))
        {
            await _commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(absolutePathString, content).ConfigureAwait(false);
        }
        else
        {
            // TODO: Save As to make new file
            NotificationHelper.DispatchInformative("Save Action", "File not found. TODO: Save As", _commonUtilityService, TimeSpan.FromSeconds(7));
        }

        DateTime? fileLastWriteTime = null;

        if (absolutePathString is not null)
        {
            fileLastWriteTime = await _commonUtilityService.FileSystemProvider.File.GetLastWriteTimeAsync(
                    absolutePathString,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        if (onAfterSaveCompletedWrittenDateTimeFunc is not null)
            await onAfterSaveCompletedWrittenDateTimeFunc.Invoke(fileLastWriteTime);
    }
    
    private ValueTask Do_SetFolderExplorerState(AbsolutePath folderAbsolutePath)
    {
        _folderExplorerService.With(
            inFolderExplorerState => inFolderExplorerState with
            {
                AbsolutePath = folderAbsolutePath
            });

        return Do_SetFolderExplorerTreeView(folderAbsolutePath);
    }

    private async ValueTask Do_SetFolderExplorerTreeView(AbsolutePath folderAbsolutePath)
    {
        _folderExplorerService.With(inFolderExplorerState => inFolderExplorerState with
        {
            IsLoadingFolderExplorer = true
        });
        
		_commonUtilityService.EnvironmentProvider.DeletionPermittedRegister(new(folderAbsolutePath.Value, true));

        var rootNode = new TreeViewAbsolutePath(
            folderAbsolutePath,
            _ideComponentRenderers,
            _commonUtilityService,
            true,
            true);

        await rootNode.LoadChildListAsync().ConfigureAwait(false);

        if (!_commonUtilityService.TryGetTreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                out var treeViewState))
        {
            _commonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                new List<TreeViewNoType>() { rootNode }));
        }
        else
        {
            _commonUtilityService.TreeView_WithRootNodeAction(FolderExplorerState.TreeViewContentStateKey, rootNode);

            _commonUtilityService.TreeView_SetActiveNodeAction(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                true,
                false);
        }

        _folderExplorerService.With(inFolderExplorerState => inFolderExplorerState with
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
        _inputFileService.StartInputFileStateForm(
            message,
            onAfterSubmitFunc,
            selectionIsValidFunc,
            inputFilePatternsList);

        var inputFileDialog = new DialogViewModel(
            DialogFacts.InputFileDialogKey,
            "Input File",
            _ideComponentRenderers.InputFileRendererType,
            null,
            Walk.Ide.RazorLib.Htmls.Models.HtmlFacts.Classes.DIALOG_PADDING_0,
            true,
            null);

        _commonUtilityService.Dialog_ReduceRegisterAction(inputFileDialog);

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
}
