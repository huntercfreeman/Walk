using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class TreeViewFindAllTextSpan : TreeViewWithType<(string SourceText, TextEditorTextSpan TextSpan)>
{
	public TreeViewFindAllTextSpan(
			(string SourceText, TextEditorTextSpan TextSpan) tuple,
			AbsolutePath absolutePath,
			bool isExpandable,
			bool isExpanded)
		: base(tuple, isExpandable, isExpanded)
	{
		AbsolutePath = absolutePath;
	}
	
	public AbsolutePath AbsolutePath { get; }
	public string? PreviewEarlierNearbyText { get; set; }
	public string? PreviewLaterNearbyText { get; set; }
	
	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewFindAllTextSpan otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => Item.TextSpan.ResourceUri.Value.GetHashCode();
	
	public override string GetDisplayText() => Item.TextSpan.Text;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	    
	    using Microsoft.AspNetCore.Components;
        using Walk.TextEditor.RazorLib.FindAlls.Models;
        using Walk.TextEditor.RazorLib.Lexers.Models;
        
        namespace Walk.TextEditor.RazorLib.FindAlls.Displays;
        
        public partial class TreeViewFindAllTextSpanDisplay : ComponentBase
        {
        	[Parameter, EditorRequired]
        	public TreeViewFindAllTextSpan TreeViewFindAllTextSpan { get; set; } = null!;
        	
        	protected override void OnInitialized()
        	{
        		CalculatePreviewNearbyText();
        		base.OnInitialized();
        	}
        	
        	private void CalculatePreviewNearbyText()
        	{
        		var localTreeView = TreeViewFindAllTextSpan;
        		
        		if (localTreeView.PreviewEarlierNearbyText is not null &&
        			localTreeView.PreviewLaterNearbyText is not null)
        		{
        			return;
        		}
        		
        		var distanceOffset = 15;
        		
        		var startingIndexInclusive = localTreeView.Item.TextSpan.StartInclusiveIndex - distanceOffset;
        		startingIndexInclusive = Math.Max(0, startingIndexInclusive);
        		
            	var endingIndexExclusive = localTreeView.Item.TextSpan.EndExclusiveIndex + distanceOffset;
          	  endingIndexExclusive = Math.Min(localTreeView.Item.SourceText.Length, endingIndexExclusive);
            	
            	var earlierTextSpan = new TextEditorTextSpan(
        		    startingIndexInclusive,
        		    startingIndexInclusive + localTreeView.Item.TextSpan.StartInclusiveIndex - startingIndexInclusive,
        		    0,
        		    localTreeView.Item.TextSpan.ResourceUri,
        		    localTreeView.Item.SourceText);
        		localTreeView.PreviewEarlierNearbyText = earlierTextSpan.Text;
        		
        		var laterTextSpan = new TextEditorTextSpan(
        		    localTreeView.Item.TextSpan.EndExclusiveIndex,
        		    localTreeView.Item.TextSpan.EndExclusiveIndex + endingIndexExclusive - localTreeView.Item.TextSpan.EndExclusiveIndex,
        		    0,
        		    localTreeView.Item.TextSpan.ResourceUri,
        		    localTreeView.Item.SourceText);
        		localTreeView.PreviewLaterNearbyText = laterTextSpan.Text;
        	}
        }
	
	
	
	    <div title="start position index inclusive: @TreeViewFindAllTextSpan.Item.TextSpan.StartInclusiveIndex">
        	<span>@TreeViewFindAllTextSpan.PreviewEarlierNearbyText</span><!--
        	--><span class="di_te_keyword-control">@(TreeViewFindAllTextSpan.Item.TextSpan.Text)</span><!--
        	--><span>@TreeViewFindAllTextSpan.PreviewLaterNearbyText</span>
        </div>

	
	
	
	
		return new TreeViewRenderer(
			typeof(TreeViewFindAllTextSpanDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewFindAllTextSpanDisplay.TreeViewFindAllTextSpan),
					this
				}
			});
	}*/
	
	public override Task LoadChildListAsync()
	{
		return Task.CompletedTask;
	}
}
