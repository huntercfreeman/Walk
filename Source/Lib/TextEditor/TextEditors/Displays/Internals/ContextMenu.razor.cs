using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class ContextMenu : ComponentBase, ITextEditorDependentComponent
{
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private WalkTextEditorConfig TextEditorConfig { get; set; } = null!;
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    private IFileSystemProvider FileSystemProvider { get; set; } = null!;

    [Parameter, EditorRequired]
	public Key<TextEditorComponentData> ComponentDataKey { get; set; }

    private ElementReference? _textEditorContextMenuElementReference;
    
    private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (_textEditorContextMenuElementReference is not null)
            {
                try
                {
                    await _textEditorContextMenuElementReference.Value
                        .FocusAsync(preventScroll: true)
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
                    //             This bug is seemingly happening randomly. I have a suspicion
                    //             that there are race-condition exceptions occurring with "FocusAsync"
                    //             on an ElementReference.
                }
            }
        }
    }
    
    private TextEditorRenderBatch GetRenderBatch()
    {
    	return GetComponentData()?.RenderBatch ?? default;
    }
    
    private TextEditorComponentData? GetComponentData()
    {
    	if (_componentDataKeyPrevious != ComponentDataKey)
    	{
    		if (!TextEditorService.TextEditorState._componentDataMap.TryGetValue(ComponentDataKey, out var componentData) ||
    		    componentData is null)
    		{
    			_componentData = null;
    		}
    		else
    		{
    			_componentData = componentData;
				_componentDataKeyPrevious = ComponentDataKey;
    		}
    	}
    	
		return _componentData;
    }

    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return;
    	
        if (KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key)
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
			{
				var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);

				if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
				{
					TextEditorCommandDefaultFunctions.RemoveDropdown(
				        editContext,
				        viewModelModifier,
				        DropdownService);
				}

				return ValueTask.CompletedTask;
			});
        }
    }

    private Task ReturnFocusToThisAsync()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    		
        try
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
			{
				var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);

				if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
				{
					TextEditorCommandDefaultFunctions.RemoveDropdown(
				        editContext,
				        viewModelModifier,
				        DropdownService);
				}

				return ValueTask.CompletedTask;
			});
			return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private MenuRecord GetMenuRecord()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return new MenuRecord(MenuRecord.NoMenuOptionsExistList);
    		
    	return renderBatch.Model.PersistentState.CompilerService.GetContextMenu(renderBatch, this);
    }
    
    public MenuRecord GetDefaultMenuRecord()
    {
    	List<MenuOptionRecord> menuOptionRecordsList = new();
    	
    	var cut = new MenuOptionRecord("Cut (Ctrl x)", MenuOptionKind.Other, () => SelectMenuOption(CutMenuOption));
        menuOptionRecordsList.Add(cut);

        var copy = new MenuOptionRecord("Copy (Ctrl c)", MenuOptionKind.Other, () => SelectMenuOption(CopyMenuOption));
        menuOptionRecordsList.Add(copy);

        var paste = new MenuOptionRecord("Paste (Ctrl v)", MenuOptionKind.Other, () => SelectMenuOption(PasteMenuOption));
        menuOptionRecordsList.Add(paste);
        
        var toggleCollapse = new MenuOptionRecord("Toggle Collapse (Ctrl m)", MenuOptionKind.Other, () => SelectMenuOption(ToggleCollapseOption));
        menuOptionRecordsList.Add(toggleCollapse);
        
        var findInTextEditor = new MenuOptionRecord("Find (Ctrl f)", MenuOptionKind.Other, () => SelectMenuOption(FindInTextEditor));
        menuOptionRecordsList.Add(findInTextEditor);
        
        /*
        // FindAllReferences
        var findAllReferences = new MenuOptionRecord("Find All References (Shift F12)", MenuOptionKind.Other, () => SelectMenuOption(FindAllReferences));
        menuOptionRecordsList.Add(findAllReferences);
        */
        
        var relatedFilesQuickPick = new MenuOptionRecord("Related Files (F7)", MenuOptionKind.Other, () => SelectMenuOption(RelatedFilesQuickPick));
        menuOptionRecordsList.Add(relatedFilesQuickPick);
        
        var peekDefinition = new MenuOptionRecord("Peek definition (Alt F12)", MenuOptionKind.Other, () => SelectMenuOption(PeekDefinitionOption));
        menuOptionRecordsList.Add(peekDefinition);
        
        var goToDefinition = new MenuOptionRecord("Go to definition (F12)", MenuOptionKind.Other, () => SelectMenuOption(GoToDefinitionOption));
        menuOptionRecordsList.Add(goToDefinition);
        
        var quickActionsSlashRefactors = new MenuOptionRecord("QuickActions/Refactors (Ctrl .)", MenuOptionKind.Other, () => SelectMenuOption(QuickActionsSlashRefactors));
        menuOptionRecordsList.Add(quickActionsSlashRefactors);

        if (!menuOptionRecordsList.Any())
            menuOptionRecordsList.Add(new MenuOptionRecord("No Context Menu Options for this item", MenuOptionKind.Other));

        return new MenuRecord(menuOptionRecordsList);
    }

    public Task SelectMenuOption(Func<Task> menuOptionAction)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    		
        _ = Task.Run(async () =>
        {
            try
            {
				TextEditorService.WorkerArbitrary.PostUnique(editContext =>
				{
					var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);

					if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
					{
						TextEditorCommandDefaultFunctions.RemoveDropdown(
					        editContext,
					        viewModelModifier,
					        DropdownService);
					}

					return ValueTask.CompletedTask;
				});

                await menuOptionAction.Invoke().ConfigureAwait(false);
                TextEditorService.ViewModelApi.StopCursorBlinking();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }
    
    public Task CutMenuOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    		
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
		    var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
			
            return TextEditorCommandDefaultFunctions.CutAsync(
            	editContext,
		        modelModifier,
		        viewModelModifier,
		        ClipboardService);
        });
        return Task.CompletedTask;
    }

    public Task CopyMenuOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
	        var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
		    var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        	
            return TextEditorCommandDefaultFunctions.CopyAsync(
        		editContext,
		        modelModifier,
		        viewModelModifier,
		        ClipboardService);
        });
        return Task.CompletedTask;
    }

    public Task PasteMenuOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.PasteAsync(
            	editContext,
		        modelModifier,
		        viewModelModifier,
		        ClipboardService);
        });
        return Task.CompletedTask;
    }

	public Task ToggleCollapseOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
    		
    		CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

			foreach (var collapsePoint in viewModelModifier.PersistentState.AllCollapsePointList)
			{
				for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex; lineOffset++)
				{
					if (viewModelModifier.LineIndex == collapsePoint.AppendToLineIndex + lineOffset)
						encompassingCollapsePoint = collapsePoint;
				}
			}
			
        	if (encompassingCollapsePoint.AppendToLineIndex != -1)
        	{
        		_ = TextEditorCommandDefaultFunctions.ToggleCollapsePoint(
            		encompassingCollapsePoint.AppendToLineIndex,
        			modelModifier,
        			viewModelModifier);
        	}
        
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    public Task GoToDefinitionOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    		
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);

    		if (viewModelModifier is null)
    			return ValueTask.CompletedTask;
            
            viewModelModifier.PersistentState.ShouldRevealCursor = true;
            
            TextEditorCommandDefaultFunctions.GoToDefinition(
            	editContext,
            	modelModifier,
            	viewModelModifier,
            	new Category("main"));
        	return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }
    
    public Task PeekDefinitionOption()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    		
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);

    		if (viewModelModifier is null)
    			return ValueTask.CompletedTask;
            
            viewModelModifier.PersistentState.ShouldRevealCursor = true;
            
            TextEditorCommandDefaultFunctions.GoToDefinition(
            	editContext,
            	modelModifier,
            	viewModelModifier,
            	new Category("CodeSearchService"));
        	return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }
    
    public Task QuickActionsSlashRefactors()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
        	var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.QuickActionsSlashRefactor(
            	editContext,
            	modelModifier,
            	viewModelModifier,
            	CommonBackgroundTaskApi.JsRuntimeCommonApi,
            	TextEditorService,
            	DropdownService);
        });
        return Task.CompletedTask;
    }
    
    public Task FindInTextEditor()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
        	var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.ShowFindOverlay(
		        editContext,
            	modelModifier,
            	viewModelModifier,
		        CommonBackgroundTaskApi.JsRuntimeCommonApi);
        });
        return Task.CompletedTask;
    }
    
    public Task FindAllReferences()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
        	var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        
            return ((TextEditorKeymapDefault)TextEditorKeymapFacts.DefaultKeymap).ShiftF12Func.Invoke(
            	editContext,
    			modelModifier,
    			viewModelModifier);
        });
        return Task.CompletedTask;
    }
    
    public Task RelatedFilesQuickPick()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(renderBatch.Model.PersistentState.ResourceUri);
        	var viewModelModifier = editContext.GetViewModelModifier(renderBatch.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.RelatedFilesQuickPick(
		        editContext,
            	modelModifier,
            	viewModelModifier,
		        CommonBackgroundTaskApi.JsRuntimeCommonApi,
		        EnvironmentProvider,
		        FileSystemProvider,
		        TextEditorService,
		        DropdownService);
        });
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    	// This component isn't subscribing to the text editor render batch changing event.
    	return;
    }
}