using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppResizeHandleHeight : ComponentBase, IDisposable
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public int ResizeHandleHeightInPixels
    {
        get => CommonUtilityService.GetAppOptionsState().Options.ResizeHandleHeightInPixels;
        set
        {
            if (value < AppOptionsState.MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS)
                value = AppOptionsState.MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS;

            CommonUtilityService.Options_SetResizeHandleHeight(value);
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
