using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppFontFamily : ComponentBase, IDisposable
{
    [Inject]
    private ICommonUtilityService CommonUtilityService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public string FontFamily
    {
        get => CommonUtilityService.GetAppOptionsState().Options.FontFamily ?? "unset";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                CommonUtilityService.Options_SetFontFamily(null);

            CommonUtilityService.Options_SetFontFamily(value.Trim());
        }
    }

    protected override void OnInitialized()
    {
        CommonUtilityService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        CommonUtilityService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
    }
}