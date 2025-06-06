using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppIconSize : ComponentBase, IDisposable
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    private int IconSizeInPixels
    {
        get => AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;
        set
        {
            if (value < AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS)
                value = AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS;

            AppOptionsService.SetIconSize(value);
        }
    }

    protected override Task OnInitializedAsync()
    {
        AppOptionsService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;

        return base.OnInitializedAsync();
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