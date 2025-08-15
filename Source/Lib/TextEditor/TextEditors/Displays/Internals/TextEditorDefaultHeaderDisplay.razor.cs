using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib.TextEditors.Models;

// HeaderDriver.cs
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class TextEditorDefaultHeaderDisplay : ComponentBase, ITextEditorDependentComponent
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorComponentData> ComponentDataKey { get; set; }

    public string _reloadButtonHtmlElementId = "di_te_text-editor-header-reload-button";
    
    private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;

    protected override void OnInitialized()
    {
        TextEditorService.SecondaryChanged += OnTextEditorWrapperCssStateChanged;
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
    
    private async void OnTextEditorWrapperCssStateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.TextEditorWrapperCssStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
        else if (secondaryChangedKind == SecondaryChangedKind.ViewModel_CursorShouldBlinkChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public Task DoSaveOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            TextEditorCommandDefaultFunctions.TriggerSave(
                editContext,
                modelModifier,
                viewModelModifier,
                TextEditorService.CommonService);
            return ValueTask.CompletedTask;
        }));
        return Task.CompletedTask;
    }

    public Task DoCopyOnClick(MouseEventArgs arg)
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

    public Task DoCutOnClick(MouseEventArgs arg)
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

    public Task DoPasteOnClick(MouseEventArgs arg)
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

    public Task DoRedoOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            TextEditorCommandDefaultFunctions.Redo(
                editContext,
                modelModifier,
                viewModelModifier);
            
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    public Task DoUndoOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            TextEditorCommandDefaultFunctions.Undo(
                editContext,
                modelModifier,
                viewModelModifier);
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    public Task DoSelectAllOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            TextEditorCommandDefaultFunctions.SelectAll(
                editContext,
                modelModifier,
                viewModelModifier);
                
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    public Task DoRemeasureOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            TextEditorCommandDefaultFunctions.TriggerRemeasure(
                editContext,
                viewModelModifier);
                
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    public async Task DoReloadOnClick(MouseEventArgs arg)
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return;
        
        var dropdownKey = Key<DropdownRecord>.NewKey();
        
        var buttonDimensions = await TextEditorService.JsRuntimeCommonApi
            .MeasureElementById(_reloadButtonHtmlElementId)
            .ConfigureAwait(false);
            
        var menuOptionList = new List<MenuOptionRecord>();
        
        var absolutePath = TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(virtualizationResult.Model.PersistentState.ResourceUri.Value, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder(), shouldNameContainsExtension: true);

        menuOptionList.Add(new MenuOptionRecord(
            "Cancel",
            MenuOptionKind.Read,
            onClickFunc: () =>
            {
                TextEditorService.CommonService.Dropdown_ReduceDisposeAction(dropdownKey);
                return Task.CompletedTask;
            }));
            
        menuOptionList.Add(new MenuOptionRecord(
            $"Reset: '{absolutePath.Name}'",
            MenuOptionKind.Delete,
            onClickFunc: () =>
            {
                TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                {
                    editContext.TextEditorService.ViewModel_Dispose(editContext, virtualizationResult.ViewModel.PersistentState.ViewModelKey);
                    TextEditorService.RemoveDirtyResourceUri(virtualizationResult.Model.PersistentState.ResourceUri);
                    editContext.TextEditorService.Model_Dispose(editContext, virtualizationResult.Model.PersistentState.ResourceUri);
                    return ValueTask.CompletedTask;
                });
                return Task.CompletedTask;
            }));
            
        var menu = new MenuRecord(menuOptionList);

        var dropdownRecord = new DropdownRecord(
            dropdownKey,
            buttonDimensions.LeftInPixels,
            buttonDimensions.TopInPixels + buttonDimensions.HeightInPixels,
            typeof(MenuDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(MenuDisplay.Menu),
                    menu
                }
            },
            async () => await TextEditorService.JsRuntimeCommonApi.FocusHtmlElementById(_reloadButtonHtmlElementId));

        TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
    }

    public Task DoRefreshOnClick()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return Task.CompletedTask;
        
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
        
            TextEditorCommandDefaultFunctions.TriggerRemeasure(
                editContext,
                viewModelModifier);
            return ValueTask.CompletedTask;
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// disabled=@GetUndoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetUndoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    public bool GetUndoDisabledAttribute()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return true;
        
        var model = virtualizationResult.Model;
        var viewModel = virtualizationResult.ViewModel;

        if (model is null || viewModel is null)
            return true;

        return !model.CanUndoEdit();
    }

    /// <summary>
    /// disabled=@GetRedoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetRedoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    public bool GetRedoDisabledAttribute()
    {
        var virtualizationResult = GetVirtualizationResult();
        if (!virtualizationResult.IsValid)
            return true;
        
        var model = virtualizationResult.Model;
        var viewModel = virtualizationResult.ViewModel;

        if (model is null || viewModel is null)
            return true;

        return !model.CanRedoEdit();
    }

    public void Dispose()
    {
        TextEditorService.SecondaryChanged -= OnTextEditorWrapperCssStateChanged;
    }
}
