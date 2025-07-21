using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorFontFamily : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public string FontFamily
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.CommonOptions.FontFamily ?? "unset";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                TextEditorService.Options_SetFontFamily(null);

            TextEditorService.Options_SetFontFamily(value.Trim());
        }
    }

    protected override void OnInitialized()
    {
        TextEditorService.Options_StaticStateChanged += OptionsWrapOnStateChanged;
    }

    private async void OptionsWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TextEditorService.Options_StaticStateChanged -= OptionsWrapOnStateChanged;
    }
}
