using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Displays;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
// FindAllReferences
// using Walk.Ide.RazorLib.FindAllReferences.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.CodeSearches.Models;

namespace Walk.Ide.RazorLib.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IContextService _contextService;
    private readonly TextEditorService _textEditorService;
    private readonly ITreeViewService _treeViewService;
    // FindAllReferences
    // private readonly IFindAllReferencesService _findAllReferencesService;
    private readonly ICodeSearchService _codeSearchService;
    private readonly ICommonUtilityService _commonUtilityService;
    private readonly CommonBackgroundTaskApi _commonBackgroundTaskApi;

    public CommandFactory(
    	IContextService contextService,
		TextEditorService textEditorService,
		ITreeViewService treeViewService,
		// FindAllReferences
		// IFindAllReferencesService findAllReferencesService,
		ICodeSearchService codeSearchService,
		ICommonUtilityService commonUtilityService,
		CommonBackgroundTaskApi commonBackgroundTaskApi)
    {
    	_contextService = contextService;
		_textEditorService = textEditorService;
		_treeViewService = treeViewService;
		// FindAllReferences
		// _findAllReferencesService = findAllReferencesService;
		_codeSearchService = codeSearchService;
		_commonUtilityService = commonUtilityService;
		_commonBackgroundTaskApi = commonBackgroundTaskApi;
    }

    private WidgetModel? _contextSwitchWidget;
    private WidgetModel? _commandBarWidget;
    
	public IDialog? CodeSearchDialog { get; set; }

	public void Initialize()
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
                    ContextFacts.ActiveContextsContext, "Focus: ActiveContexts", "focus-active-contexts", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.BackgroundServicesContext, "Focus: BackgroundServices", "focus-background-services", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.CompilerServiceExplorerContext, "Focus: CompilerServiceExplorer", "focus-compiler-service-explorer", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.CompilerServiceEditorContext, "Focus: CompilerServiceEditor", "focus-compiler-service-editor", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.DialogDisplayContext, "Focus: DialogDisplay", "focus-dialog-display", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.EditorContext, "Focus: Editor", "focus-editor", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.FolderExplorerContext, "Focus: FolderExplorer", "focus-folder-explorer", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.GitContext, "Focus: Git", "focus-git", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.GlobalContext, "Focus: Global", "focus-global", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.MainLayoutFooterContext, "Focus: Footer", "focus-footer", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.MainLayoutHeaderContext, "Focus: Header", "focus-header", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.ErrorListContext, "Focus: Error List", "error-list", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.OutputContext, "Focus: Output", "focus-output", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.TerminalContext, "Focus: Terminal", "focus-terminal", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.TestExplorerContext, "Focus: Test Explorer", "focus-test-explorer", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
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
                    ContextFacts.TextEditorContext, "Focus: TextEditor", "focus-text-editor", _commonBackgroundTaskApi.JsRuntimeCommonApi, _commonUtilityService));
        }

        // Focus the text editor itself (as to allow for typing into the editor)
        {
            var focusTextEditorCommand = new CommonCommand(
                "Focus: Text Editor", "focus-text-editor", false,
                async commandArgs =>
                {
                    var group = _textEditorService.GroupApi.GetOrDefault(IdeBackgroundTaskApi.EditorTextEditorGroupKey);
                    if (group is null)
                        return;

                    var activeViewModel = _textEditorService.ViewModelApi.GetOrDefault(group.ActiveViewModelKey);
                    if (activeViewModel is null)
                        return;

					var componentData = activeViewModel.PersistentState.ComponentData;
					if (componentData is not null)
					{
						await _commonBackgroundTaskApi.JsRuntimeCommonApi
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
					_textEditorService.OptionsApi.ShowFindAllDialog();
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
					var elementDimensions = await _commonBackgroundTaskApi.JsRuntimeCommonApi
						.MeasureElementById("di_ide_header-button-file")
						.ConfigureAwait(false);
						
					var contextState = _contextService.GetContextState();
					
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
			        
			        if (_contextService.GetContextState().FocusedContextKey == ContextFacts.TextEditorContext.ContextKey)
			        {
			        	_contextService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkTextEditorInitializer.ContextSwitchGroupKey;
			        }
			        else
			        {
			        	_contextService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkCommonInitializer.ContextSwitchGroupKey;
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
    
    public ValueTask OpenCodeSearchDialog()
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
        
        _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
        	var group = _textEditorService.GroupApi.GetOrDefault(IdeBackgroundTaskApi.EditorTextEditorGroupKey);
            if (group is null)
                return;

            var activeViewModel = _textEditorService.ViewModelApi.GetOrDefault(group.ActiveViewModelKey);
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
			
			_treeViewService.ReduceMoveHomeAction(
				CodeSearchState.TreeViewCodeSearchContainerKey,
				false,
				false);
        });
        
        return ValueTask.CompletedTask;
    }
    
    public async ValueTask PeekCodeSearchDialog(TextEditorEditContext editContext, string? resourceUriValue, int? indexInclusiveStart)
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
		
		_treeViewService.ReduceMoveHomeAction(
			CodeSearchState.TreeViewCodeSearchContainerKey,
			false,
			false);
    }
    
    /*
    // FindAllReferences
    public async ValueTask ShowAllReferences(
    	TextEditorEditContext editContext,
    	TextEditorModel modelModifier,
    	TextEditorViewModel viewModelModifier,
    	CursorModifierBagTextEditor cursorModifierBag)
    {
    	var primaryCursorModifier = cursorModifierBag.CursorModifier;
    	
        var cursorPositionIndex = modelModifier.GetPositionIndex(primaryCursorModifier);
    
        var foundMatch = false;
        
        var resource = modelModifier.CompilerService.GetResource(modelModifier.ResourceUri);
        var compilationUnitLocal = resource.CompilationUnit;
        
        if (compilationUnitLocal is not IExtendedCompilationUnit extendedCompilationUnit)
        	return;
        
        var symbolList = extendedCompilationUnit.SymbolList;
        var foundSymbol = default(Symbol);
        
        foreach (var symbol in symbolList)
        {
            if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex)
            {
                foundMatch = true;
				foundSymbol = symbol;
            }
        }
        
        if (!foundMatch)
        	return;
    
    	var symbolLocal = foundSymbol;
		var targetNode = SymbolDisplay.GetTargetNode(_textEditorService, symbolLocal);
		var definitionNode = SymbolDisplay.GetDefinitionNode(_textEditorService, symbolLocal, targetNode);
		
		if (definitionNode is null || definitionNode.SyntaxKind != SyntaxKind.TypeDefinitionNode)
			return;
			
		// TODO: Do not duplicate this code from SyntaxViewModel.HandleOnClick(...)
		
		string? resourceUriValue = null;
		var indexInclusiveStart = -1;
		
		var typeDefinitionNode = (TypeDefinitionNode)definitionNode;
		resourceUriValue = typeDefinitionNode.TypeIdentifierToken.TextSpan.ResourceUri.Value;
		indexInclusiveStart = typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex;
		
		if (resourceUriValue is null || indexInclusiveStart == -1)
			return;
		
    	_findAllReferencesService.SetFullyQualifiedName(
    		typeDefinitionNode.NamespaceName,
    		typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(),
    		typeDefinitionNode);
    
        var findAllReferencesPanel = new Panel(
            "Find All References",
            Walk.Ide.RazorLib.FindAllReferences.Displays.FindAllReferencesDisplay.FindAllReferencesPanelKey,
            Walk.Ide.RazorLib.FindAllReferences.Displays.FindAllReferencesDisplay.FindAllReferencesDynamicViewModelKey,
            ContextFacts.FindAllReferencesContext.ContextKey,
            typeof(Walk.Ide.RazorLib.FindAllReferences.Displays.FindAllReferencesDisplay),
            null,
            _panelService,
            _dialogService,
            _commonBackgroundTaskApi);
        _panelService.RegisterPanelTab(PanelFacts.BottomPanelGroupKey, findAllReferencesPanel, false);

        _panelService.SetActivePanelTab(PanelFacts.BottomPanelGroupKey, findAllReferencesPanel.Key);
    }
    */
}
