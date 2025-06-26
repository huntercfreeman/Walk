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
        get => TextEditorService.OptionsApi.GetTextEditorOptionsState().Options.CommonOptions.FontFamily ?? "unset";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                TextEditorService.OptionsApi.SetFontFamily(null);

            TextEditorService.OptionsApi.SetFontFamily(value.Trim());
        }
    }

    protected override void OnInitialized()
    {
        TextEditorService.OptionsApi.StaticStateChanged += OptionsWrapOnStateChanged;
    }

    private async void OptionsWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TextEditorService.OptionsApi.StaticStateChanged -= OptionsWrapOnStateChanged;
    }
}