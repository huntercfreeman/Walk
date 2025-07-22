namespace Walk.Common.RazorLib.TreeViews.Models;

public struct TreeViewNodeParameter
{
    public TreeViewNodeParameter(TreeViewNoType treeViewNoType, int depth, TreeViewCascadingValueBatch renderBatch)
    {
        TreeViewNoType = treeViewNoType;
        Depth = depth;
        RenderBatch = renderBatch;
    }

    public TreeViewNoType TreeViewNoType { get; set; }
    public int Depth { get; set; }
    public TreeViewCascadingValueBatch RenderBatch { get; set; }
}
