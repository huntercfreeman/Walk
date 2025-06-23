using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewException : TreeViewWithType<Exception>
{
    private readonly ICommonComponentRenderers _commonComponentRenderers;

    public TreeViewException(
            Exception exception,
            bool isExpandable,
            bool isExpanded,
            ICommonComponentRenderers commonComponentRenderers)
        : base(exception, isExpandable, isExpanded)
    {
        _commonComponentRenderers = commonComponentRenderers;
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

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
      
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
      
      
      <div class="di_tree-view-exception"
             style="display: flex; align-items: center;">
             
            @{
            	var appOptionsState = AppOptionsService.GetAppOptionsState();
            
            	var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
            }
        
            @IconErrorFragment.Render(iconDriver)
        
            <span style="margin-left: 0.5ch;">
                @TreeViewException.Item.Message
            </span>
        </div>
        
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewExceptionRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewException),
                    this
                },
            });
    }*/

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }
}