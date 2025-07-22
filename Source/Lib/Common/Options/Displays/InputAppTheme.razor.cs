using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppTheme : IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    protected override void OnInitialized()
    {
        CommonService.CommonUiStateChanged += OnAppOptionsStateChanged;
    }

    private async void OnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private void OnThemeSelectChanged(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs.Value is null)
            return;

        var themeState = CommonService.GetThemeState();

        var intAsString = (string)changeEventArgs.Value;

        if (int.TryParse(intAsString, out var intValue))
        {
            var themesInScopeList = themeState.ThemeList.Where(x => x.IncludeScopeApp)
                .ToArray();

            var existingThemeRecord = themesInScopeList.FirstOrDefault(btr => btr.Key == intValue);

            if (existingThemeRecord != default)
                CommonService.Options_SetActiveThemeRecordKey(existingThemeRecord.Key);
        }
    }

    private bool CheckIsActiveValid(ThemeRecord[] themeRecordList, int activeThemeKey)
    {
        return themeRecordList.Any(btr => btr.Key == activeThemeKey);
    }

    private bool CheckIsActiveSelection(int themeKey, int activeThemeKey)
    {
        return themeKey == activeThemeKey;
    }

    public async void OnAppOptionsStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.AppOptionsStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnAppOptionsStateChanged;
    }
}
