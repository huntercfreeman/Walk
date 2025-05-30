using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewText : TreeViewWithType<string>
{
    private readonly ICommonComponentRenderers _commonComponentRenderers;

    public TreeViewText(
            string text,
            bool isExpandable,
            bool isExpanded,
            ICommonComponentRenderers commonComponentRenderers)
        : base(text, isExpandable, isExpanded)
    {
        _commonComponentRenderers = commonComponentRenderers;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewText treeViewText)
            return false;

        return treeViewText.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewTextRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewText),
                    this
                },
            });
    }

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }
}