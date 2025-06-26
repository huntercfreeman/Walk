using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppFontSize : ComponentBase, IDisposable
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public int FontSizeInPixels
    {
        get => AppOptionsService.GetAppOptionsState().Options.FontSizeInPixels;
        set
        {
            if (value < AppOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS)
                value = AppOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS;

            AppOptionsService.SetFontSize(value);
        }
    }

    protected override void OnInitialized()
    {
        AppOptionsService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        AppOptionsService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
    }
}