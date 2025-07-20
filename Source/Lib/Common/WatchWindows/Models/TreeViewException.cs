using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewException : TreeViewWithType<Exception>
{
    public TreeViewException(
            Exception exception,
            bool isExpandable,
            bool isExpanded)
        : base(exception, isExpandable, isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewException treeViewException)
            return false;

        return treeViewException.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override string GetDisplayText() => Item.Message;

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }
}
