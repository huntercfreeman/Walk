using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorFontSize : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;
    
    private static readonly TimeSpan ThrottleDelay = TimeSpan.FromMilliseconds(300); 
    private readonly Throttle _throttle = new Throttle(ThrottleDelay);
    
    private int _fontSizeInPixels;
    private bool _hasFocus;

    public int FontSizeInPixels
    {
        get => _fontSizeInPixels;
        set
        {
            // This has to be done here as well as the Service API due to the throttling using it.
            if (value < TextEditorOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS)
                value = TextEditorOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS;
        
            _fontSizeInPixels = value;

            _throttle.Run(_ =>
            {
                TextEditorService.Options_SetFontSize(_fontSizeInPixels);
                return Task.CompletedTask;
            });
        }
    }

    protected override void OnInitialized()
    {
        TextEditorService.SecondaryChanged += OptionsWrapOnStateChanged;
        ReadActualFontSizeInPixels();
    }
    
    private void ReadActualFontSizeInPixels()
    {
        var temporaryFontSizeInPixels = TextEditorService.Options_GetTextEditorOptionsState().Options.CommonOptions?.FontSizeInPixels;
        
        if (temporaryFontSizeInPixels is null)
        {
            temporaryFontSizeInPixels = TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS;

            _throttle.Run(_ =>
            {
                TextEditorService.Options_SetFontSize(temporaryFontSizeInPixels.Value);
                return Task.CompletedTask;
            });
        }
        
        _fontSizeInPixels = temporaryFontSizeInPixels.Value;
    }

    private async void OptionsWrapOnStateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.StaticStateChanged && !_hasFocus)
        {
            ReadActualFontSizeInPixels();
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private void HandleOnFocus()
    {
        _hasFocus = true;
    }
    
    private void HandleOnBlur()
    {
        _hasFocus = false;
    }

    public void Dispose()
    {
        TextEditorService.SecondaryChanged -= OptionsWrapOnStateChanged;
    }
}
