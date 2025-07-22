using Microsoft.AspNetCore.Components;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorHeight : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter(Name = "InputElementCssClass")]
    public string CascadingInputElementCssClass { get; set; } = string.Empty;

    [Parameter]
    public string TopLevelDivElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string InputElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string LabelElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string CheckboxElementCssClassString { get; set; } = string.Empty;

    private const int MINIMUM_HEIGHT_IN_PIXELS = 200;

    private int TextEditorHeight
    {
        get => TextEditorService.Options_GetTextEditorOptionsState().Options.TextEditorHeightInPixels ?? MINIMUM_HEIGHT_IN_PIXELS;
        set
        {
            if (value < MINIMUM_HEIGHT_IN_PIXELS)
                value = MINIMUM_HEIGHT_IN_PIXELS;

            TextEditorService.Options_SetHeight(value);
        }
    }
    
    protected override void OnInitialized()
    {
        TextEditorService.SecondaryChanged += TextEditorOptionsStateWrapOnStateChanged;
    }

    public string GetIsDisabledCssClassString(bool globalHeightInPixelsValueIsNull)
    {
        return globalHeightInPixelsValueIsNull
            ? "di_te_disabled"
            : "";
    }

    private void ToggleUseGlobalHeightInPixels(bool globalHeightInPixelsValueIsNull)
    {
        if (globalHeightInPixelsValueIsNull)
            TextEditorService.Options_SetHeight(MINIMUM_HEIGHT_IN_PIXELS);
        else
            TextEditorService.Options_SetHeight(null);
    }
    
    private async void TextEditorOptionsStateWrapOnStateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.StaticStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrapOnStateChanged;
    }
}
