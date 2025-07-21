using Walk.Common.RazorLib;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.CompilerServices.DotNetSolution.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

public class TreeViewSolution : TreeViewWithType<DotNetSolutionModel>
{
	public TreeViewSolution(
			DotNetSolutionModel dotNetSolutionModel,
			CommonService commonService,
			bool isExpandable,
			bool isExpanded)
		: base(dotNetSolutionModel, isExpandable, isExpanded)
	{
		CommonService = commonService;
	}

	public CommonService CommonService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewSolution treeViewSolution)
			return false;

		return treeViewSolution.Item.AbsolutePath.Value ==
			   Item.AbsolutePath.Value;
	}

	public override int GetHashCode() => Item.AbsolutePath.Value.GetHashCode();

	public override string GetDisplayText() => Item.AbsolutePath.NameWithExtension;
	
	public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconDotNetSolutionFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	    
	
		return new TreeViewRenderer(
			IdeComponentRenderers.IdeTreeViews.TreeViewNamespacePathRendererType,
			new Dictionary<string, object?>
			{
				{
					nameof(ITreeViewNamespacePathRendererType.NamespacePath),
					Item.NamespacePath
				},
			});
	}*/

	public override async Task LoadChildListAsync()
	{
		try
		{
			var previousChildren = new List<TreeViewNoType>(ChildList);

			var newChildList = await TreeViewHelperDotNetSolution.LoadChildrenAsync(this).ConfigureAwait(false);

			ChildList = newChildList;
			LinkChildren(previousChildren, ChildList);
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

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}
