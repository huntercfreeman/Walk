using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Options.Displays;

public partial class InputTextEditorTheme : ComponentBase, IDisposable
{
    [Inject]
    private IThemeService ThemeService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;
	
	protected override void OnInitialized()
    {
    	TextEditorService.OptionsApi.StaticStateChanged += TextEditorOptionsStateWrapOnStateChanged;
    }
    
    private void SelectedThemeChanged(ChangeEventArgs changeEventArgs)
    {
        var themeList = ThemeService.GetThemeState().ThemeList;

        var chosenThemeKeyGuidString = changeEventArgs.Value?.ToString() ?? string.Empty;

        if (Guid.TryParse(chosenThemeKeyGuidString,
                out var chosenThemeKeyGuid))
        {
            var chosenThemeKey = new Key<ThemeRecord>(chosenThemeKeyGuid);
            var foundTheme = themeList.FirstOrDefault(x => x.Key == chosenThemeKey);

            if (foundTheme is not null)
                TextEditorService.OptionsApi.SetTheme(foundTheme);
        }
        else
        {
            TextEditorService.OptionsApi.SetTheme(ThemeFacts.VisualStudioDarkThemeClone);
        }
    }
    
    private async void TextEditorOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.OptionsApi.StaticStateChanged -= TextEditorOptionsStateWrapOnStateChanged;
    }
}