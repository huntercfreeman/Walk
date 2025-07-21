using Microsoft.AspNetCore.Components;

namespace Walk.Common.RazorLib.WatchWindows.Displays;

public partial class TreeViewMissingRendererFallbackDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public string DisplayText { get; set; } = string.Empty;
}
