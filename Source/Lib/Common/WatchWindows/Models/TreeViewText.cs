using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewText : TreeViewWithType<string>
{
    public TreeViewText(
            string text,
            bool isExpandable,
            bool isExpanded)
        : base(text, isExpandable, isExpanded)
    {
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

    public override string GetDisplayText() => Item;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewTextDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewText TreeViewText { get; set; } = null!;
        }
        
    
    
        <div title="@TreeViewText.Key.Guid">
        
            @{
                var appOptionsState = AppOptionsService.GetAppOptionsState();
            
                var iconDriver = new IconDriver(
                    appOptionsState.Options.IconSizeInPixels,
                    appOptionsState.Options.IconSizeInPixels);
            }
        
            @IconSymbolKeyFragment.Render(iconDriver)
            @TreeViewText.Item
        </div>
    
    
    
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewTextRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewText),
                    this
                },
            });
    }*/

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }
}
