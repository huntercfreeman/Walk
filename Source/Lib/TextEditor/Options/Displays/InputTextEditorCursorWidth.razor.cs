using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorCursorWidth : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    private const double MINIMUM_CURSOR_SIZE_IN_PIXELS = 1;

    private double TextEditorCursorWidth
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.CursorWidthInPixels;
        set
        {
            if (value < MINIMUM_CURSOR_SIZE_IN_PIXELS)
                value = MINIMUM_CURSOR_SIZE_IN_PIXELS;

            TextEditorService.Options_SetCursorWidth(value);
        }
    }
    
    protected override void OnInitialized()
    {
    	TextEditorService.Options_StaticStateChanged += OnOptionStaticStateChanged;
    }
    
    private async void OnOptionStaticStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.Options_StaticStateChanged -= OnOptionStaticStateChanged;
    }
}