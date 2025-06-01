using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.FindAlls.Displays;
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
	
	public override TreeViewRenderer GetTreeViewRenderer()
	{
		return new TreeViewRenderer(
			typeof(TreeViewFindAllTextSpanDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewFindAllTextSpanDisplay.TreeViewFindAllTextSpan),
					this
				}
			});
	}
	
	public override Task LoadChildListAsync()
	{
		return Task.CompletedTask;
	}
}
