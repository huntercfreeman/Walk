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
        get => TextEditorService.OptionsApi.GetTextEditorOptionsState().Options.CursorWidthInPixels;
        set
        {
            if (value < MINIMUM_CURSOR_SIZE_IN_PIXELS)
                value = MINIMUM_CURSOR_SIZE_IN_PIXELS;

            TextEditorService.OptionsApi.SetCursorWidth(value);
        }
    }
    
    protected override void OnInitialized()
    {
    	TextEditorService.OptionsApi.StaticStateChanged += OnOptionStaticStateChanged;
    }
    
    private async void OnOptionStaticStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.OptionsApi.StaticStateChanged -= OnOptionStaticStateChanged;
    }
}