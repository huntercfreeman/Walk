using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppResizeHandleWidth : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public int ResizeHandleWidthInPixels
    {
        get => CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels;
        set
        {
            if (value < AppOptionsState.MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS)
                value = AppOptionsState.MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS;

            CommonService.Options_SetResizeHandleWidth(value);
        }
    }

    protected override void OnInitialized()
    {
        CommonService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
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
