using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorShowNewLines : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public bool GlobalShowNewlines
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.ShowNewlines;
        set => TextEditorService.Options_SetShowNewlines(value);
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
