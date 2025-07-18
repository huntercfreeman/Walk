using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorTabWidth : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public int TabWidth
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.TabWidth;
        set => TextEditorService.Options_SetTabWidth(value);
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
