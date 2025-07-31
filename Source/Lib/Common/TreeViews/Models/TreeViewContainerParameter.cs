using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Commands.Models;

namespace Walk.Common.RazorLib.TreeViews.Models;

public struct TreeViewContainerParameter
{
    public TreeViewContainerParameter(
        Key<TreeViewContainer> treeViewContainerKey,
        TreeViewKeyboardEventHandler treeViewKeyboardEventHandler,
        TreeViewMouseEventHandler treeViewMouseEventHandler,
        Func<TreeViewCommandArgs, Task>? onContextMenuFunc)
    {
        TreeViewContainerKey = treeViewContainerKey;
        TreeViewKeyboardEventHandler = treeViewKeyboardEventHandler;
        TreeViewMouseEventHandler = treeViewMouseEventHandler;
        OnContextMenuFunc = onContextMenuFunc;
    }
    
    public Key<TreeViewContainer> TreeViewContainerKey { get; set; } = Key<TreeViewContainer>.Empty;
    public TreeViewKeyboardEventHandler TreeViewKeyboardEventHandler { get; set; } = null!;
    public TreeViewMouseEventHandler TreeViewMouseEventHandler { get; set; } = null!;
    public Func<TreeViewCommandArgs, Task>? OnContextMenuFunc { get; set; }
}
