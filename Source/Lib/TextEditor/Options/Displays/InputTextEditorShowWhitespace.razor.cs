using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorShowWhitespace : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public bool GlobalShowWhitespace
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.ShowWhitespace;
        set => TextEditorService.Options_SetShowWhitespace(value);
    }
    
    protected override void OnInitialized()
    {
        TextEditorService.OptionsChanged += TextEditorOptionsStateWrapOnStateChanged;
    }
    
    private async void TextEditorOptionsStateWrapOnStateChanged(OptionsChangedKind optionsChangedKind)
    {
        if (optionsChangedKind == OptionsChangedKind.StaticStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        TextEditorService.OptionsChanged -= TextEditorOptionsStateWrapOnStateChanged;
    }
}
