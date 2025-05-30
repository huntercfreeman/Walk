using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Icons.Models;

public class IconBase : ComponentBase
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [CascadingParameter(Name = "WalkCommonIconWidthOverride")]
    public int? WalkCommonIconWidthOverride { get; set; }
    [CascadingParameter(Name = "WalkCommonIconHeightOverride")]
    public int? WalkCommonIconHeightOverride { get; set; }

    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;

    public int WidthInPixels => WalkCommonIconWidthOverride ??
        AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;

    public int HeightInPixels => WalkCommonIconHeightOverride ??
        AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;
}