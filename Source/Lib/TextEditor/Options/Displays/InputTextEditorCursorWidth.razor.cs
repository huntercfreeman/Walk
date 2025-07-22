using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorCursorWidth : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    private double TextEditorCursorWidth
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.CursorWidthInPixels;
        set
        {
            TextEditorService.Options_SetCursorWidth(value);
        }
    }
    
    protected override void OnInitialized()
    {
        TextEditorService.SecondaryChanged += OnOptionStaticStateChanged;
    }
    
    private async void OnOptionStaticStateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.StaticStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        TextEditorService.SecondaryChanged -= OnOptionStaticStateChanged;
    }
}
