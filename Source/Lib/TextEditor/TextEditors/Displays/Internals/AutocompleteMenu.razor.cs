using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Autocompletes.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class AutocompleteMenu : ComponentBase, ITextEditorDependentComponent
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAutocompleteService AutocompleteService { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

	[Parameter, EditorRequired]
	public Key<TextEditorComponentData> ComponentDataKey { get; set; }
	
	public const string HTML_ELEMENT_ID = "di_te_autocomplete-menu-id";
	
	private static readonly MenuRecord NoResultsMenuRecord = new(
		new List<MenuOptionRecord>()
        {
            new("No results", MenuOptionKind.Other)
        });

    private ElementReference? _autocompleteMenuElementReference;
    private MenuDisplay? _autocompleteMenuComponent;
    
    private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;
    
    protected override void OnInitialized()
    {
        TextEditorService.TextEditorStateChanged += OnTextEditorStateChanged;
        OnTextEditorStateChanged();
        
        base.OnInitialized();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
    	var componentData = GetComponentData();
    	
    	if (componentData?.MenuShouldTakeFocus ?? false)
    	{
    		componentData.MenuShouldTakeFocus = false;
        		
        	await _autocompleteMenuComponent.SetFocusToFirstOptionInMenuAsync();
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
    
    private async void OnTextEditorStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }

    private Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return Task.CompletedTask;
    
        if (KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key)
			return ReturnFocusToThisAsync();
			
		return Task.CompletedTask;
    }

    private async Task ReturnFocusToThisAsync()
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return;
    		
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
				
			await renderBatch.ViewModel.FocusAsync();
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
    		return NoResultsMenuRecord;
    
        try
        {
            return renderBatch.Model.PersistentState.CompilerService.GetAutocompleteMenu(renderBatch, this);
        }
		// Catching 'InvalidOperationException' is for the currently occurring case: "Collection was modified; enumeration operation may not execute."
        catch (Exception e) when (e is WalkTextEditorException || e is InvalidOperationException)
        {
            return NoResultsMenuRecord;
        }
    }
    
    public MenuRecord GetDefaultMenuRecord(List<AutocompleteEntry>? otherAutocompleteEntryList = null)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return NoResultsMenuRecord;
    
        try
        {
            if (renderBatch.ViewModel.ColumnIndex > 0)
            {
                var word = renderBatch.Model.ReadPreviousWordOrDefault(
                    renderBatch.ViewModel.LineIndex,
                    renderBatch.ViewModel.ColumnIndex);

                List<MenuOptionRecord> menuOptionRecordsList = new();

                if (word is not null)
                {
                    var autocompleteWordsList = AutocompleteService.GetAutocompleteOptions(word);

                    var autocompleteEntryList = autocompleteWordsList
                        .Select(aw => new AutocompleteEntry(aw, AutocompleteEntryKind.Word, null))
                        .ToList();
                     
                    if (otherAutocompleteEntryList is not null && otherAutocompleteEntryList.Count != 0)   
                    {
                        otherAutocompleteEntryList.AddRange(autocompleteEntryList);
						autocompleteEntryList = otherAutocompleteEntryList;
                    }

                    menuOptionRecordsList = autocompleteEntryList.Select(entry => new MenuOptionRecord(
                        entry.DisplayName,
                        MenuOptionKind.Other,
                        () => SelectMenuOption(() =>
                        {
                        	if (entry.AutocompleteEntryKind != AutocompleteEntryKind.Snippet)
                            	InsertAutocompleteMenuOption(word, entry, renderBatch.ViewModel);
                            	
                            return entry.SideEffectFunc?.Invoke();
                        }),
                        widgetParameterMap: new Dictionary<string, object?>
                        {
                            {
                                nameof(AutocompleteEntry),
                                entry
                            }
                        }))
                    .ToList();
                }

                if (!menuOptionRecordsList.Any())
                    menuOptionRecordsList.Add(new MenuOptionRecord("No results", MenuOptionKind.Other));

                return new MenuRecord(menuOptionRecordsList);
            }

            return NoResultsMenuRecord;
        }
		// Catching 'InvalidOperationException' is for the currently occurring case: "Collection was modified; enumeration operation may not execute."
        catch (Exception e) when (e is WalkTextEditorException || e is InvalidOperationException)
        {
            return NoResultsMenuRecord;
        }
    }

    public async Task SelectMenuOption(Func<Task> menuOptionAction)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return;
    
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

					return renderBatch.ViewModel.FocusAsync();
				});
				
				// (2025-01-21)
				// ====================================================================================
				// System.NullReferenceException: Object reference not set to an instance of an object.
				//    at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.<>c__DisplayClass30_0.<<SelectMenuOption>b__0>d.MoveNext() in C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\TextEditor\TextEditors\Displays\Internals\AutocompleteMenu.razor.cs:line 235
				// System.NullReferenceException: Object reference not set to an instance of an object.
				//    at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.<>c__DisplayClass30_0.<<SelectMenuOption>b__0>d.MoveNext() in C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\TextEditor\TextEditors\Displays\Internals\AutocompleteMenu.razor.cs:line 235
				// System.NullReferenceException: Object reference not set to an instance of an object.
				//    at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.<>c__DisplayClass30_0.<<SelectMenuOption>b__0>d.MoveNext() in C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\TextEditor\TextEditors\Displays\Internals\AutocompleteMenu.razor.cs:line 235
				// System.NullReferenceException: Object reference not set to an instance of an object.
				//    at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.<>c__DisplayClass30_0.<<SelectMenuOption>b__0>d.MoveNext() in C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\TextEditor\TextEditors\Displays\Internals\AutocompleteMenu.razor.cs:line 235
				// PS C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\Ide\Host.Photino\bin\Release\net8.0\publish>
				try
				{
					await menuOptionAction.Invoke().ConfigureAwait(false);
				}
                catch (Exception e)
                {
                	Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }, CancellationToken.None);

        await renderBatch.ViewModel.FocusAsync();
    }

    public async Task InsertAutocompleteMenuOption(
        string word,
        AutocompleteEntry autocompleteEntry,
        TextEditorViewModel viewModel)
    {
    	var renderBatch = GetRenderBatch();
    	if (!renderBatch.IsValid)
    		return;
    
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);

            if (modelModifier is null || viewModelModifier is null)
                return ValueTask.CompletedTask;
        
        	TextEditorService.ModelApi.InsertText(
        		editContext,
		        modelModifier,
		        viewModelModifier,
		        autocompleteEntry.DisplayName.Substring(word.Length));
		        
            return renderBatch.ViewModel.FocusAsync();
        });
		
		await renderBatch.ViewModel.FocusAsync();
    }
    
    public void Dispose()
    {
    	TextEditorService.TextEditorStateChanged -= OnTextEditorStateChanged;
    }
}