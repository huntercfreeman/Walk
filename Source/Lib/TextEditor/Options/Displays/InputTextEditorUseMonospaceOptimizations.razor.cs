using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorUseMonospaceOptimizations : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public bool UseMonospaceOptimizations
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.UseMonospaceOptimizations;
        set => TextEditorService.Options_SetUseMonospaceOptimizations(value);
    }
    
    protected override void OnInitialized()
    {
    	TextEditorService.Options_StaticStateChanged += TextEditorOptionsStateWrapOnStateChanged;
    }
    
    private async void TextEditorOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.Options_StaticStateChanged -= TextEditorOptionsStateWrapOnStateChanged;
    }
}