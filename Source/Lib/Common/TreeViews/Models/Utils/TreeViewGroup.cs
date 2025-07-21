namespace Walk.Common.RazorLib.TreeViews.Models.Utils;

public class TreeViewGroup : TreeViewWithType<string>
{
    public TreeViewGroup(
            string displayText,
            bool isExpandable,
            bool isExpanded)
        : base(displayText, isExpandable, isExpanded)
    {
    }
    
    public string TitleText { get; init; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewGroup treeViewGroup)
            return false;

        return treeViewGroup.Item == Item;
    }

    public override int GetHashCode() => Item.GetHashCode();

    public override string GetDisplayText() => Item;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.TreeViews.Models.Utils;
        
        namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;
        
        public partial class TreeViewGroupDisplay : ComponentBase
        {
            [Parameter, EditorRequired]
            public TreeViewGroup TreeViewGroup { get; set; } = null!;
        }
        
    
        <div title="@TreeViewGroup.TitleText">
            @TreeViewGroup.Item (@(TreeViewGroup.ChildList?.Count.ToString() ?? "0"))
        </div>
    
    
        return new TreeViewRenderer(
            typeof(TreeViewGroupDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewGroupDisplay.TreeViewGroup),
                    this
                },
            });
    }*/

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        return;
    }
}
