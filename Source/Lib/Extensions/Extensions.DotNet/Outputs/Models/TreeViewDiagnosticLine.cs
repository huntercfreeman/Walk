using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.Outputs.Displays.Internals;

namespace Walk.Extensions.DotNet.Outputs.Models;

public class TreeViewDiagnosticLine : TreeViewWithType<DiagnosticLine>
{
	public TreeViewDiagnosticLine(
			DiagnosticLine diagnosticLine,
			bool isExpandable,
			bool isExpanded)
		: base(diagnosticLine, isExpandable, isExpanded)
	{
	}

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewDiagnosticLine otherTreeView)
			return false;

		return otherTreeView.Item == Item;
	}

	public override int GetHashCode() => Item.GetHashCode();

	public override TreeViewRenderer GetTreeViewRenderer()
	{
		return new TreeViewRenderer(
			typeof(TreeViewDiagnosticLineDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewDiagnosticLineDisplay.TreeViewDiagnosticLine),
					this
				},
			});
	}

	public override Task LoadChildListAsync()
	{
		return Task.CompletedTask;
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}
