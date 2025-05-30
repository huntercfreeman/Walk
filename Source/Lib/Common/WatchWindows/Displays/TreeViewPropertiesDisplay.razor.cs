using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.WatchWindows.Displays;

public partial class TreeViewPropertiesDisplay : ComponentBase
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TreeViewProperties TreeViewProperties { get; set; } = null!;
}