using Walk.Common.RazorLib.ComponentRenderers.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.Common.RazorLib.WatchWindows.Displays;

public partial class TreeViewMissingRendererFallbackDisplay : ComponentBase,
    ITreeViewMissingRendererFallbackType
{
    [Parameter, EditorRequired]
    public string DisplayText { get; set; } = string.Empty;
}