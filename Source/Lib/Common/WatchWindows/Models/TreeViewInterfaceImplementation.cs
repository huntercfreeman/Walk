
namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewInterfaceImplementation : TreeViewReflection
{
    public TreeViewInterfaceImplementation(
            WatchWindowObject watchWindowObject,
            bool isExpandable,
            bool isExpanded)
        : base(watchWindowObject, isExpandable, isExpanded)
    {
    }

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewInterfaceImplementationDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewInterfaceImplementation TreeViewInterfaceImplementation { get; set; } = null!;
        }
    
    
    
        <div title="@TreeViewInterfaceImplementation.Key.Guid">
    
            @{
                var appOptionsService = AppOptionsService.GetAppOptionsState();
            
                var iconDriver = new IconDriver(
                    appOptionsService.Options.IconSizeInPixels,
                    appOptionsService.Options.IconSizeInPixels);
            }
        
            @IconSymbolInterfaceFragment.Render(iconDriver)
            ConcreteType:
            <span class="di_te_type">@TreeViewInterfaceImplementation.Item.ItemType.Name</span>
        </div>
    
    
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewInterfaceImplementationRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewInterfaceImplementation),
                    this
                },
            });
    }*/
}
