using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.TreeViews.Models;

public class TreeViewCascadingValueBatch
{
    public TreeViewContainer TreeViewContainer { get; set; } = null!;
    public Func<MouseEventArgs?, Key<TreeViewContainer>, TreeViewNoType?, Task> HandleTreeViewOnContextMenu { get; set; } = null!;
    public TreeViewMouseEventHandler TreeViewMouseEventHandler { get; set; } = null!;
    public TreeViewKeyboardEventHandler TreeViewKeyboardEventHandler { get; set; } = null!;
    public ICommonUtilityService CommonUtilityService { get; set; } = null!;
    public int OffsetPerDepthInPixels { get; set; }
    public int WalkTreeViewIconWidth { get; set; }
}
