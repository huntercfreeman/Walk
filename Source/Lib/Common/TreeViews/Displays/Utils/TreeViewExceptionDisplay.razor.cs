using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Options.Models;


namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;

public partial class TreeViewExceptionDisplay : ComponentBase, ITreeViewExceptionRendererType
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TreeViewException TreeViewException { get; set; } = null!;
}