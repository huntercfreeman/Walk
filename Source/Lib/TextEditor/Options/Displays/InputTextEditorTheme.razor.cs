using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorTheme : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;
    
    protected override void OnInitialized()
    {
        TextEditorService.OptionsChanged += TextEditorOptionsStateWrapOnStateChanged;
    }
    
    private void SelectedThemeChanged(ChangeEventArgs changeEventArgs)
    {
        var themeList = CommonService.GetThemeState().ThemeList;

        var chosenThemeKeyIntString = changeEventArgs.Value?.ToString() ?? string.Empty;

        if (int.TryParse(chosenThemeKeyIntString, out var chosenThemeKeyInt))
        {
            var foundTheme = themeList.FirstOrDefault(x => x.Key == chosenThemeKeyInt);

            if (foundTheme is not null)
                TextEditorService.Options_SetTheme(foundTheme);
        }
        else
        {
            TextEditorService.Options_SetTheme(CommonFacts.VisualStudioDarkThemeClone);
        }
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
