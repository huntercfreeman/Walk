using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppIconSize : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    private int IconSizeInPixels
    {
        get => CommonService.GetAppOptionsState().Options.IconSizeInPixels;
        set
        {
            if (value < AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS)
                value = AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS;

            CommonService.Options_SetIconSize(value);
        }
    }

    protected override Task OnInitializedAsync()
    {
        CommonService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
        return Task.CompletedTask;
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        CommonService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
    }
}