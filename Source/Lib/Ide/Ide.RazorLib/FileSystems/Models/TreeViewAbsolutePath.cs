using Walk.Common.RazorLib;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;

namespace Walk.Ide.RazorLib.FileSystems.Models;

public class TreeViewAbsolutePath : TreeViewWithType<AbsolutePath>
{
    public TreeViewAbsolutePath(
            AbsolutePath absolutePath,
            IIdeComponentRenderers ideComponentRenderers,
            CommonService commonService,
            bool isExpandable,
            bool isExpanded)
        : base(absolutePath, isExpandable, isExpanded)
    {
        IdeComponentRenderers = ideComponentRenderers;
        CommonService = commonService;
    }

    public IIdeComponentRenderers IdeComponentRenderers { get; }
    public CommonService CommonService { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewAbsolutePath treeViewAbsolutePath)
            return false;

        return treeViewAbsolutePath.Item.Value == Item.Value;
    }

    public override int GetHashCode() => Item.Value.GetHashCode();
    
    public override string GetDisplayText() => Item.NameWithExtension;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
        
        using Walk.Common.RazorLib.Keys.Models;
        using Walk.Common.RazorLib.TreeViews.Models;
        using Walk.Ide.RazorLib.ComponentRenderers.Models;
        using Walk.Ide.RazorLib.FileSystems.Models;
        using Microsoft.AspNetCore.Components;
        
        namespace Walk.Ide.RazorLib.FileSystems.Displays;
        
        public partial class TreeViewAbsolutePathDisplay : ComponentBase, ITreeViewAbsolutePathRendererType
        {
            [CascadingParameter]
            public TreeViewContainer TreeViewState { get; set; } = null!;
            [CascadingParameter(Name = "SearchQuery")]
            public string SearchQuery { get; set; } = string.Empty;
            [CascadingParameter(Name = "SearchMatchTuples")]
            public List<(Key<TreeViewContainer> treeViewStateKey, TreeViewAbsolutePath treeViewAbsolutePath)>? SearchMatchTuples { get; set; }
        
            [Parameter, EditorRequired]
            public TreeViewAbsolutePath TreeViewAbsolutePath { get; set; } = null!;
        }
        
        
        
        <div>
    
            @{ var displayText = TreeViewAbsolutePath.Item.NameWithExtension; }
            
            @if (!String.IsNullOrWhiteSpace(SearchQuery))
            {
                var indexOfSearchQuery = displayText.IndexOf(
                    SearchQuery,
                    StringComparison.InvariantCultureIgnoreCase);
        
                if (indexOfSearchQuery == -1)
                {
                    @: @displayText
                }
                else
                {
                    var localSearchMatchTuples = SearchMatchTuples;
        
                    if (localSearchMatchTuples is not null)
                    {
                        localSearchMatchTuples.Add((TreeViewState.Key, TreeViewAbsolutePath));
                    }
                    
                    string splitStart = string.Empty;
                    string splitMiddle = string.Empty;
                    string splitEnd = string.Empty;
        
                    if (indexOfSearchQuery > 0)
                    {
                        splitStart = displayText.Substring(0, indexOfSearchQuery);
                    }
                    
                    splitMiddle = displayText.Substring(indexOfSearchQuery, SearchQuery.Length);
        
                    var remainingIndices = displayText.Length - indexOfSearchQuery - SearchQuery.Length;
                    
                    if (remainingIndices > 0)
                    {
                        splitEnd = displayText.Substring(indexOfSearchQuery + SearchQuery.Length);
                    }
        
                    <text>
                        @splitStart<span class="di_ide_search-match">@splitMiddle</span>@splitEnd
                    </text>
                }
            }
            else
            {
                @: @displayText
            }
        </div>
        
        
        
        
        
        return new TreeViewRenderer(
            IdeComponentRenderers.IdeTreeViews.TreeViewAbsolutePathRendererType,
            new Dictionary<string, object?>
            {
                { nameof(ITreeViewAbsolutePathRendererType.TreeViewAbsolutePath), this },
            });
    }*/

    public override async Task LoadChildListAsync()
    {
        try
        {
            var previousChildren = new List<TreeViewNoType>(ChildList);

            var newChildList = new List<TreeViewNoType>();

            if (Item.IsDirectory)
                newChildList = await TreeViewHelperAbsolutePathDirectory.LoadChildrenAsync(this).ConfigureAwait(false);

            ChildList = newChildList;
            LinkChildren(previousChildren, ChildList);
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

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        // This method is meant to do nothing in this case.
    }
}