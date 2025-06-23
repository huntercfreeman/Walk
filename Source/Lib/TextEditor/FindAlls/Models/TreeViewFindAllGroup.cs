using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.FindAlls.Displays;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class TreeViewFindAllGroup : TreeViewWithType<List<TreeViewFindAllTextSpan>>
{
	public TreeViewFindAllGroup(
			List<TreeViewFindAllTextSpan> treeViewFindAllTextSpanList,
			AbsolutePath absolutePath,
			bool isExpandable,
			bool isExpanded)
		: base(treeViewFindAllTextSpanList, isExpandable, isExpanded)
	{
		AbsolutePath = absolutePath;
	}
	
	public AbsolutePath AbsolutePath { get; }
	
	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewFindAllGroup otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => AbsolutePath.Value.GetHashCode();
	
	public override string GetDisplayText() => AbsolutePath.NameWithExtension;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.TextEditor.RazorLib.FindAlls.Models;
        
        namespace Walk.TextEditor.RazorLib.FindAlls.Displays;
        
        public partial class TreeViewFindAllGroupDisplay : ComponentBase
        {
        	[Parameter, EditorRequired]
        	public TreeViewFindAllGroup TreeViewFindAllGroup { get; set; } = null!;
        }
	
	
	
	
	    <div title="@TreeViewFindAllGroup.AbsolutePath.Value">
        	<span class="di_te_keyword">
        		@(TreeViewFindAllGroup.AbsolutePath.NameWithExtension)
        	</span>
        	
        	<span title="matches">
        		@($"{TreeViewFindAllGroup.Item.Count:N0}")
        	</span>
        </div>

	
	
	
		return new TreeViewRenderer(
			typeof(TreeViewFindAllGroupDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewFindAllGroupDisplay.TreeViewFindAllGroup),
					this
				}
			});
	}*/
	
	public override Task LoadChildListAsync()
	{
		if (ChildList.Count != 0)
			return Task.CompletedTask;
		
		var previousChildList = ChildList;
		ChildList = Item.Select(x => (TreeViewNoType)x).ToList();
		LinkChildren(previousChildList, ChildList);
		
		return Task.CompletedTask;
	}
}
