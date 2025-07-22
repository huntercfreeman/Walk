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
        set => CommonService.Options_SetIconSize(value);
    }

    protected override Task OnInitializedAsync()
    {
        CommonService.CommonUiStateChanged += AppOptionsStateWrapOnStateChanged;
        return Task.CompletedTask;
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
