namespace Walk.Common.RazorLib.TreeViews.Models.Utils;

public class TreeViewSpinner : TreeViewWithType<Guid>
{
    public TreeViewSpinner(
            Guid guid,
            bool isExpandable,
            bool isExpanded)
        : base(guid, isExpandable, isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewSpinner treeViewSpinner)
            return false;

        return treeViewSpinner.Item == Item;
    }

    public override int GetHashCode() => Item.GetHashCode();

    public override string GetDisplayText() => string.Empty;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
        
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.TreeViews.Models.Utils;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;
        
        public partial class TreeViewSpinnerDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewSpinner TreeViewSpinner { get; set; } = null!;
        }
    
    
    
        
        @{
            var appOptionsState = AppOptionsService.GetAppOptionsState();
        
            var iconDriver = new IconDriver(
                appOptionsState.Options.IconSizeInPixels,
                appOptionsState.Options.IconSizeInPixels);
        }
        
        @IconLoadingFragment.Render(iconDriver)

        
        
        
        
        
        return new TreeViewRenderer(
            typeof(TreeViewSpinnerDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewSpinnerDisplay.TreeViewSpinner),
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
