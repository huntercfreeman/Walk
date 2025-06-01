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