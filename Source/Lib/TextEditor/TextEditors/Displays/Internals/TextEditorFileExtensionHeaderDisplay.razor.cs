using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class TextEditorFileExtensionHeaderDisplay : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorViewModel> TextEditorViewModelKey { get; set; }
    [Parameter, EditorRequired]
    public Key<TextEditorComponentData> ComponentDataKey { get; set; }
    
    private Dictionary<string, object?> _componentInnerParameters = null!;
    
    private string _fileExtensionCurrent = string.Empty;
    
    private TextEditorViewModelSlimDisplay _previousTextEditorViewModelSlimDisplay;
    
    private string DictionaryKey => nameof(ITextEditorDependentComponent.ComponentDataKey);
    
    protected override void OnInitialized()
    {
        _componentInnerParameters = new()
        {
            {
                DictionaryKey,
                ComponentDataKey
            }
        };

        // ShouldRender does not invoke on the initial render.
        _ = ShouldRender();
        
        TextEditorService.OptionsChanged += OnTextEditorWrapperCssStateChanged;
    }
    
    protected override bool ShouldRender()
    {
        TextEditorViewModel? viewModel;
        TextEditorModel? model;
        
        if (TextEditorService.TextEditorState._viewModelMap.TryGetValue(
                TextEditorViewModelKey,
                out viewModel))
        {
            _ = TextEditorService.TextEditorState._modelMap.TryGetValue(
                    viewModel.PersistentState.ResourceUri,
                    out model);
        }
        else
        {
            model = null;
        }
        
        var fileExtensionLocal = model is null
            ? string.Empty
            : model.PersistentState.FileExtension;
            
        if (_fileExtensionCurrent != fileExtensionLocal)
            _fileExtensionCurrent = fileExtensionLocal;
            
        return true;
    }
    
    private async void OnTextEditorWrapperCssStateChanged(OptionsChangedKind optionsChangedKind)
    {
        if (optionsChangedKind == OptionsChangedKind.TextEditorWrapperCssStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        TextEditorService.OptionsChanged -= OnTextEditorWrapperCssStateChanged;
    }
}
