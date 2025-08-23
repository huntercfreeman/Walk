using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.Extensions.DotNet;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class EditorDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    
    private Key<TextEditorViewModel> _previousActiveViewModelKey = Key<TextEditorViewModel>.Empty;
    
    private Key<TextEditorComponentData> _componentDataKey;
    private ViewModelDisplayOptions _viewModelDisplayOptions = null!;
    
    private static readonly List<HeaderButtonKind> TextEditorHeaderButtonKindsList =
        Enum.GetValues(typeof(HeaderButtonKind))
            .Cast<HeaderButtonKind>()
            .ToList();
    
    private string? _htmlId = null;
    private string HtmlId => _htmlId ??= $"di_te_group_{Walk.TextEditor.RazorLib.TextEditorService.EditorTextEditorGroupKey.Guid}";
    
    protected override void OnInitialized()
    {
        _viewModelDisplayOptions = new()
        {
            TabIndex = 0,
            HeaderButtonKinds = TextEditorHeaderButtonKindsList,
            HeaderComponentType = typeof(TextEditorFileExtensionHeaderDisplay),
            TextEditorHtmlElementId = Guid.NewGuid(),
        };
        
        _componentDataKey = new Key<TextEditorComponentData>(_viewModelDisplayOptions.TextEditorHtmlElementId);
    
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;
    }
    
    private async void TextEditorOptionsStateWrap_StateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.Group_TextEditorGroupStateChanged)
        {
            var textEditorGroup = DotNetService.TextEditorService.Group_GetTextEditorGroupState().EditorTextEditorGroup;
                
            if (_previousActiveViewModelKey != textEditorGroup.ActiveViewModelKey)
            {
                _previousActiveViewModelKey = textEditorGroup.ActiveViewModelKey;
                DotNetService.TextEditorService.ViewModel_StopCursorBlinking();
            }
        
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        DotNetService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
    }
}