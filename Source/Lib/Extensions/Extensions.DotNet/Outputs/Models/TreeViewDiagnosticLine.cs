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

	public override string GetDisplayText() => Item.TextShort;
	
	public override string GetDisplayTextCssClass()
	{
	    if (Item.DiagnosticLineKind == DiagnosticLineKind.Warning)
        	return "di_tree-view-title-content di_tree-view-warning";
        else if (Item.DiagnosticLineKind == DiagnosticLineKind.Error)
        	return "di_tree-view-title-content di_tree-view-exception";
        else
        	return "di_tree-view-title-content";
     }

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Extensions.DotNet.Outputs.Models;
        
        namespace Walk.Extensions.DotNet.Outputs.Displays.Internals;
        
        public partial class TreeViewDiagnosticLineDisplay : ComponentBase
        {
        	[Parameter, EditorRequired]
        	public TreeViewDiagnosticLine TreeViewDiagnosticLine { get; set; } = null!;
        }
	
	
	    @using Walk.Extensions.DotNet.CommandLines.Models;

        @{ var cssClass = string.Empty; }
        
        @if (TreeViewDiagnosticLine.Item.DiagnosticLineKind == DiagnosticLineKind.Warning)
        {
        	cssClass = "di_tree-view-warning";
        }
        else if (TreeViewDiagnosticLine.Item.DiagnosticLineKind == DiagnosticLineKind.Error)
        {
        	cssClass = "di_tree-view-exception";
        }
        else if (TreeViewDiagnosticLine.Item.DiagnosticLineKind == DiagnosticLineKind.Other)
        {
        	cssClass = string.Empty;
        }
        
        <span class="@cssClass di_set-selectable">
        	@TreeViewDiagnosticLine.Item.TextShort
        </span>
	    
	
		return new TreeViewRenderer(
			typeof(TreeViewDiagnosticLineDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewDiagnosticLineDisplay.TreeViewDiagnosticLine),
					this
				},
			});
	}*/

	public override Task LoadChildListAsync()
	{
		return Task.CompletedTask;
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}
