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
        set => CommonService.Options_SetResizeHandleWidth(value);
    }

    protected override void OnInitialized()
    {
        CommonService.CommonUiStateChanged += AppOptionsStateWrapOnStateChanged;
    }

    private async void AppOptionsStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.AppOptionsStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= AppOptionsStateWrapOnStateChanged;
    }
}
