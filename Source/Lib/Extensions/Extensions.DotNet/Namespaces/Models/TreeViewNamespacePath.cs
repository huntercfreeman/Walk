using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.FileSystems.Displays;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.Namespaces.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewNamespacePath : TreeViewWithType<NamespacePath>
{
    public TreeViewNamespacePath(
            NamespacePath namespacePath,
            IDotNetComponentRenderers dotNetComponentRenderers,
            IIdeComponentRenderers ideComponentRenderers,
            CommonService commonService,
            bool isExpandable,
            bool isExpanded)
        : base(namespacePath, isExpandable, isExpanded)
    {
        DotNetComponentRenderers = dotNetComponentRenderers;
        IdeComponentRenderers = ideComponentRenderers;
        CommonService = commonService;
    }

    public IDotNetComponentRenderers DotNetComponentRenderers { get; }
    public IIdeComponentRenderers IdeComponentRenderers { get; }
    public CommonService CommonService { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewNamespacePath treeViewSolutionExplorer)
            return false;

        return treeViewSolutionExplorer.Item.AbsolutePath.Value ==
               Item.AbsolutePath.Value;
    }

    public override int GetHashCode() => Item.AbsolutePath.Value.GetHashCode();
    
    public override string GetDisplayText() => Item.AbsolutePath.NameWithExtension;
    
    public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => 
        iconDriver => FileIconStaticRenderFragments.GetRenderFragment((iconDriver, Item.AbsolutePath));

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
        
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Namespaces.Models;
        using Walk.Common.RazorLib.Options.Models;
        using Walk.Ide.RazorLib.ComponentRenderers.Models;
        
        namespace Walk.Ide.RazorLib.Namespaces.Displays;
        
        public partial class TreeViewNamespacePathDisplay : ComponentBase, ITreeViewNamespacePathRendererType
        {
        	[Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
        
            [CascadingParameter(Name="WalkCommonIconWidthOverride")]
            public int? WalkCommonIconWidthOverride { get; set; }
            [CascadingParameter(Name="WalkCommonIconHeightOverride")]
            public int? WalkCommonIconHeightOverride { get; set; }
        
        	[Parameter, EditorRequired]
            public NamespacePath NamespacePath { get; set; }
            [Parameter]
            public string CssStyleString { get; set; } = string.Empty;
            
            public int WidthInPixels => WalkCommonIconWidthOverride ??
                AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;
        
            public int HeightInPixels => WalkCommonIconHeightOverride ??
                AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;
        }
        
        
        @using Walk.Common.RazorLib.Icons.Models
        @using Walk.Ide.RazorLib.FileSystems.Displays
        
        <div title="@NamespacePath.AbsolutePath.Value">
        	
        	@{ var iconDriver = new IconDriver(WidthInPixels, HeightInPixels); }
        	
        	@FileIconStaticRenderFragments.GetRenderFragment((iconDriver, NamespacePath.AbsolutePath))
            @NamespacePath.AbsolutePath.NameWithExtension
        </div>
        
        
        return new TreeViewRenderer(
            IdeComponentRenderers.IdeTreeViews.TreeViewNamespacePathRendererType,
            new Dictionary<string, object?>
            {
                {
                    nameof(ITreeViewNamespacePathRendererType.NamespacePath),
                    Item
                },
            });
    }*/

    public override async Task LoadChildListAsync()
    {
    	// Codebehinds: Children need to
    	// - new instance
		// - try map to old instance
		// - opportunity for child to do something (like take siblings as their children)
    
        try
        {
            var previousChildren = new List<TreeViewNoType>(ChildList);
            var newChildList = new List<TreeViewNoType>();

			// new instance
            if (Item.AbsolutePath.IsDirectory)
            {
                newChildList = await TreeViewHelperNamespacePathDirectory.LoadChildrenAsync(this).ConfigureAwait(false);
            }
            else
            {
                switch (Item.AbsolutePath.ExtensionNoPeriod)
                {
                    case ExtensionNoPeriodFacts.DOT_NET_SOLUTION:
                        return;
                    case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                        newChildList = await TreeViewHelperCSharpProject.LoadChildrenAsync(this).ConfigureAwait(false);
                        break;
                    case ExtensionNoPeriodFacts.RAZOR_MARKUP:
                        newChildList = await TreeViewHelperRazorMarkup.LoadChildrenAsync(this).ConfigureAwait(false);
                        break;
                }
            }

			// try map to old instance
            ChildList = newChildList;
            LinkChildren(previousChildren, ChildList);
            
            // opportunity for child to do something (like take siblings as their children)
            {
	            var shouldPermitChildToTakeSiblingsAsChildren = false;
	            
	            if (Item.AbsolutePath.IsDirectory)
	            {
	                shouldPermitChildToTakeSiblingsAsChildren = true;
	            }
	            else
	            {
	                switch (Item.AbsolutePath.ExtensionNoPeriod)
	                {
	                    case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
	                        shouldPermitChildToTakeSiblingsAsChildren = true;
	                        break;
	                }
	            }
	            
	            if (shouldPermitChildToTakeSiblingsAsChildren)
	            {
	            	// Codebehind logic
					var copyOfChildrenToFindRelatedFiles = new List<TreeViewNoType>(newChildList);
			
					// Note that this loops over the original, and passes the copy
			        foreach (var child in newChildList)
			        {
			            child.RemoveRelatedFilesFromParent(copyOfChildrenToFindRelatedFiles);
			        }
			
			        // The parent directory gets what is left over after the
			        // children take their respective 'code behinds'
			        newChildList = copyOfChildrenToFindRelatedFiles;
			        
			        // This time, 'LinkChildren(...)' is invoked
			        // in order to wire up the 'TreeViewNoType.IndexAmongSiblings'.
			        // 
			        // This index is used by the keyboard events to move
			        // throughout the tree view.
		            ChildList = newChildList;
		            LinkChildren(ChildList, ChildList);
	            }
	        }
        }
        catch (Exception exception)
        {
            ChildList = new List<TreeViewNoType>
            {
                new TreeViewException(exception, false, false, CommonService.CommonComponentRenderers)
                {
                    Parent = this,
                    IndexAmongSiblings = 0,
                }
            };
        }

        TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
    }

    /// <summary>
    /// This method is called on each child when loading children for a parent node.
    /// This method allows for code-behinds
    /// </summary>
    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        if (Item.AbsolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.RAZOR_MARKUP))
            TreeViewHelperRazorMarkup.FindRelatedFiles(this, siblingsAndSelfTreeViews);
    }
}