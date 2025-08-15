using System;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Ide.RazorLib.FileSystems.Displays;
using Walk.Ide.RazorLib.Namespaces.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewNamespacePath : TreeViewWithType<AbsolutePath>
{
    public TreeViewNamespacePath(
            AbsolutePath namespacePath,
            CommonService commonService,
            bool isExpandable,
            bool isExpanded)
        : base(namespacePath, isExpandable, isExpanded)
    {
        CommonService = commonService;
    }

    public CommonService CommonService { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewNamespacePath treeViewSolutionExplorer)
            return false;

        return treeViewSolutionExplorer.Item.Value == Item.Value;
    }

    public override int GetHashCode() => Item.Value.GetHashCode();
    
    public override string GetDisplayText() => Item.Name;
    
    public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => 
        iconDriver => FileIconStaticRenderFragments.GetRenderFragment((iconDriver, Item));

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
            if (Item.IsDirectory)
            {
                newChildList = await TreeViewHelperNamespacePathDirectory.LoadChildrenAsync(this).ConfigureAwait(false);
            }
            else
            {
                if (Item.Name.EndsWith(ExtensionNoPeriodFacts.DOT_NET_SOLUTION))                {                    return;                }
                else if (Item.Name.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))                {
                    newChildList = await TreeViewHelperCSharpProject.LoadChildrenAsync(this).ConfigureAwait(false);
                }
                else if (Item.Name.EndsWith(ExtensionNoPeriodFacts.RAZOR_MARKUP))                {
                    newChildList = await TreeViewHelperRazorMarkup.LoadChildrenAsync(this).ConfigureAwait(false);
                }
            }

            // try map to old instance
            ChildList = newChildList;
            LinkChildren(previousChildren, ChildList);
            
            // opportunity for child to do something (like take siblings as their children)
            {
                var shouldPermitChildToTakeSiblingsAsChildren = false;
                
                if (Item.IsDirectory)
                {
                    shouldPermitChildToTakeSiblingsAsChildren = true;
                }
                else
                {
                    if (Item.Name.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))                    {                        shouldPermitChildToTakeSiblingsAsChildren = true;                    }
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
                new TreeViewException(exception, false, false)
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
        if (Item.Name.EndsWith(ExtensionNoPeriodFacts.RAZOR_MARKUP))
            TreeViewHelperRazorMarkup.FindRelatedFiles(this, siblingsAndSelfTreeViews);
    }
}
