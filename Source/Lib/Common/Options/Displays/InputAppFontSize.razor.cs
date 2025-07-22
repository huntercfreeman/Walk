using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Options.Displays;

public partial class InputAppFontSize : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter]
    public InputViewModel InputViewModel { get; set; } = InputViewModel.Empty;

    public int FontSizeInPixels
    {
        get => CommonService.GetAppOptionsState().Options.FontSizeInPixels;
        set => CommonService.Options_SetFontSize(value);
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
