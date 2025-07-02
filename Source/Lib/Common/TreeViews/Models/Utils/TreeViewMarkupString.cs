using Microsoft.AspNetCore.Components;
namespace Walk.Common.RazorLib.TreeViews.Models.Utils;

public class TreeViewMarkupString : TreeViewWithType<MarkupString>
{
    public TreeViewMarkupString(
            MarkupString markupString,
            bool isExpandable,
            bool isExpanded)
        : base(
            markupString,
            isExpandable,
            isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        // TODO: Equals
        return false;
    }

    public override int GetHashCode()
    {
        // TODO: GetHashCode
        return Path.GetRandomFileName().GetHashCode();
    }

    public override string GetDisplayText() => Item.Value;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
        using Microsoft.AspNetCore.Components;

        namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;
        
        public partial class TreeViewMarkupStringDisplay : ComponentBase
        {
            [Parameter, EditorRequired]
            public MarkupString MarkupString { get; set; }
        }
        
        
        
        
        <div>
            @MarkupString
        </div>
        
        
        
        
        return new TreeViewRenderer(
            typeof(TreeViewMarkupStringDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewMarkupStringDisplay.MarkupString),
                    Item
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