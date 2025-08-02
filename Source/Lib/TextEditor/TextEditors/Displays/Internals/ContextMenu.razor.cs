using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class ContextMenu : ComponentBase, ITextEditorDependentComponent
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

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
    
    private TextEditorVirtualizationResult GetVirtualizationResult()
    {
        return GetComponentData()?.Virtualization ?? TextEditorVirtualizationResult.Empty;
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
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return;
        
        if (CommonFacts.ESCAPE == keyboardEventArgs.Key)
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
                {
                    TextEditorCommandDefaultFunctions.RemoveDropdown(
                        editContext,
                        viewModelModifier,
                        TextEditorService.CommonService);
                }

                return ValueTask.CompletedTask;
            });
        }
    }

    private Task ReturnFocusToThisAsync()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
            
        try
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
                {
                    TextEditorCommandDefaultFunctions.RemoveDropdown(
                        editContext,
                        viewModelModifier,
                        TextEditorService.CommonService);
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
        MenuRecord menu;
    
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
        {
            menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
        }
        else
        {
            menu = virtualizationResult.Model.PersistentState.CompilerService.GetContextMenu(virtualizationResult, this);
            
            var componentData = virtualizationResult.ViewModel.PersistentState.ComponentData;
            if (componentData is not null)
                menu.ElementIdToRestoreFocusToOnClose = componentData.PrimaryCursorContentId;
        }
        
        return menu;
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
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
            
        _ = Task.Run(async () =>
        {
            try
            {
                TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                {
                    var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                    if (viewModelModifier.PersistentState.MenuKind != MenuKind.None)
                    {
                        TextEditorCommandDefaultFunctions.RemoveDropdown(
                            editContext,
                            viewModelModifier,
                            TextEditorService.CommonService);
                    }

                    return ValueTask.CompletedTask;
                });

                await menuOptionAction.Invoke().ConfigureAwait(false);
                TextEditorService.ViewModel_StopCursorBlinking();
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
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
            
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
            
            return TextEditorCommandDefaultFunctions.CutAsync(
                editContext,
                modelModifier,
                viewModelModifier);
        });
        return Task.CompletedTask;
    }

    public Task CopyMenuOption()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
            
            return TextEditorCommandDefaultFunctions.CopyAsync(
                editContext,
                modelModifier,
                viewModelModifier);
        });
        return Task.CompletedTask;
    }

    public Task PasteMenuOption()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.PasteAsync(
                editContext,
                modelModifier,
                viewModelModifier);
        });
        return Task.CompletedTask;
    }

    public Task GoToDefinitionOption()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
            
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            if (viewModelModifier is null)
                return ValueTask.CompletedTask;
            
            viewModelModifier.PersistentState.ShouldRevealCursor = true;
            
            TextEditorCommandDefaultFunctions.GoToDefinition(
                editContext,
                modelModifier,
                viewModelModifier,
                new Category("main"),
                modelModifier.GetPositionIndex(viewModelModifier));
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }
    
    public Task PeekDefinitionOption()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
            
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            if (viewModelModifier is null)
                return ValueTask.CompletedTask;
            
            viewModelModifier.PersistentState.ShouldRevealCursor = true;
            
            TextEditorCommandDefaultFunctions.GoToDefinition(
                editContext,
                modelModifier,
                viewModelModifier,
                new Category("CodeSearchService"),
                modelModifier.GetPositionIndex(viewModelModifier));
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }
    
    public Task QuickActionsSlashRefactors()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.QuickActionsSlashRefactor(
                editContext,
                modelModifier,
                viewModelModifier,
                TextEditorService.CommonService.JsRuntimeCommonApi,
                TextEditorService,
                TextEditorService.CommonService);
        }));
        return Task.CompletedTask;
    }
    
    public Task FindInTextEditor()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.ShowFindOverlay(
                editContext,
                modelModifier,
                viewModelModifier,
                TextEditorService.CommonService.JsRuntimeCommonApi);
        }));
        return Task.CompletedTask;
    }
    
    public Task FindAllReferences()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            return ((TextEditorKeymapDefault)TextEditorFacts.Keymap_DefaultKeymap).ShiftF12Func.Invoke(
                editContext,
                modelModifier,
                viewModelModifier);
        });
        return Task.CompletedTask;
    }
    
    public Task RelatedFilesQuickPick()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            return TextEditorCommandDefaultFunctions.RelatedFilesQuickPick(
                editContext,
                modelModifier,
                viewModelModifier,
                TextEditorService.CommonService.JsRuntimeCommonApi,
                TextEditorService.CommonService.EnvironmentProvider,
                TextEditorService.CommonService.FileSystemProvider,
                TextEditorService,
                TextEditorService.CommonService);
        }));
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // This component isn't subscribing to the text editor render batch changing event.
        return;
    }
}
