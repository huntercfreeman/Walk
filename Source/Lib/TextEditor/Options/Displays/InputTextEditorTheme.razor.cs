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
    	TextEditorService.Options_StaticStateChanged += TextEditorOptionsStateWrapOnStateChanged;
    }
    
    private void SelectedThemeChanged(ChangeEventArgs changeEventArgs)
    {
        var themeList = CommonService.GetThemeState().ThemeList;

        var chosenThemeKeyGuidString = changeEventArgs.Value?.ToString() ?? string.Empty;

        if (Guid.TryParse(chosenThemeKeyGuidString,
                out var chosenThemeKeyGuid))
        {
            var chosenThemeKey = new Key<ThemeRecord>(chosenThemeKeyGuid);
            var foundTheme = themeList.FirstOrDefault(x => x.Key == chosenThemeKey);

            if (foundTheme is not null)
                TextEditorService.Options_SetTheme(foundTheme);
        }
        else
        {
            TextEditorService.Options_SetTheme(CommonFacts.VisualStudioDarkThemeClone);
        }
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