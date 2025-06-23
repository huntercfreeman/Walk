using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TreeViewProjectTestModel : TreeViewWithType<ProjectTestModel>
{
	public TreeViewProjectTestModel(
			ProjectTestModel projectTestModel,
			ICommonComponentRenderers commonComponentRenderers,
			bool isExpandable,
			bool isExpanded)
		: base(projectTestModel, isExpandable, isExpanded)
	{
		CommonComponentRenderers = commonComponentRenderers;
	}

	public ICommonComponentRenderers CommonComponentRenderers { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewProjectTestModel treeViewProjectTestModel)
			return false;

		return treeViewProjectTestModel.Item.ProjectIdGuid == Item.ProjectIdGuid;
	}

	public override int GetHashCode() => Item.ProjectIdGuid.GetHashCode();

	/*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Extensions.DotNet.TestExplorers.Models;
        
        namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
        
        public partial class TreeViewProjectTestModelDisplay : ComponentBase
        {
        	[Parameter, EditorRequired]
        	public TreeViewProjectTestModel TreeViewProjectTestModel { get; set; } = null!;
        }
	
	
	    @TreeViewProjectTestModel.Item.AbsolutePath.NameWithExtension
        &nbsp;
        (@(TreeViewProjectTestModel.Item.TestNameFullyQualifiedList?.Count.ToString() ?? "?"))

	
		return new TreeViewRenderer(
			typeof(TreeViewProjectTestModelDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewProjectTestModelDisplay.TreeViewProjectTestModel),
					this
				},
			});
	}*/

	public override Task LoadChildListAsync()
	{
		if (Item.TestNameFullyQualifiedList is not null)
			return Task.CompletedTask;

		var previousChildren = new List<TreeViewNoType>(ChildList);

		ChildList = new[]
		{
			(TreeViewNoType)new TreeViewSpinner(
				Item.ProjectIdGuid,
				CommonComponentRenderers,
				false,
				false)
		}.ToList();

		LinkChildren(previousChildren, ChildList);

		TreeViewChangedKey = Key<TreeViewChanged>.NewKey();

		return Item.EnqueueDiscoverTestsFunc(async rootStringFragmentMap =>
		{
			try
			{
				previousChildren = new List<TreeViewNoType>(ChildList);

				if (rootStringFragmentMap.Values.Any())
				{
					var rootStringFragment = new StringFragment(string.Empty);
					rootStringFragment.Map = rootStringFragmentMap;

					var newChildList = rootStringFragment.Map.Select(kvp =>
						(TreeViewNoType)new TreeViewStringFragment(
							kvp.Value,
							CommonComponentRenderers,
							true,
							true))
						.ToArray();

					for (var i = 0; i < newChildList.Length; i++)
					{
						var node = (TreeViewStringFragment)newChildList[i];
						await node.LoadChildListAsync().ConfigureAwait(false);
					}

					ChildList = newChildList.ToList();
				}
				else
				{
					ChildList = new List<TreeViewNoType>
					{
						new TreeViewException(new Exception("No results"), false, false, CommonComponentRenderers)
						{
							Parent = this,
							IndexAmongSiblings = 0,
						}
					};
				}

				LinkChildren(previousChildren, ChildList);
			}
			catch (Exception exception)
			{
				ChildList = new List<TreeViewNoType>
				{
					new TreeViewException(exception, false, false, CommonComponentRenderers)
					{
						Parent = this,
						IndexAmongSiblings = 0,
					}
				};
			}

			TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
			Item.ReRenderNodeAction.Invoke(this);
		});
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}