using Walk.Common.RazorLib;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

public class TreeViewSolutionFolder : TreeViewWithType<SolutionFolder>
{
	public TreeViewSolutionFolder(
			SolutionFolder dotNetSolutionFolder,
			IDotNetComponentRenderers dotNetComponentRenderers,
			IIdeComponentRenderers ideComponentRenderers,
			CommonService commonService,
			bool isExpandable,
			bool isExpanded)
		: base(dotNetSolutionFolder, isExpandable, isExpanded)
	{
		DotNetComponentRenderers = dotNetComponentRenderers;
		IdeComponentRenderers = ideComponentRenderers;
		CommonService = commonService;
	}

	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public IIdeComponentRenderers IdeComponentRenderers { get; }
	public CommonService CommonService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewSolutionFolder treeViewSolutionFolder)
			return false;
		
		if (treeViewSolutionFolder.Item.IsSlnx != Item.IsSlnx)
			return false;
			
		if (Item.IsSlnx)
			return treeViewSolutionFolder.Item.ActualName == Item.ActualName;
		else
			return treeViewSolutionFolder.Item.ProjectIdGuid == Item.ProjectIdGuid;
	}

	public override int GetHashCode()
	{
		if (Item.IsSlnx)
			return Item.ActualName.GetHashCode();
		else
			return Item.ProjectIdGuid.GetHashCode();
	}

    public override string GetDisplayText() => Item.DisplayName;
    
    public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconDotNetSolutionFolderFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
		
		using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        using Walk.CompilerServices.DotNetSolution.Models.Project;
        using Walk.Extensions.DotNet.ComponentRenderers.Models;
        
        namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;
        
        public partial class TreeViewSolutionFolderDisplay : ComponentBase, ITreeViewSolutionFolderRendererType
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
        	[Parameter, EditorRequired]
        	public SolutionFolder DotNetSolutionFolder { get; set; } = null!;
        }
        
        
        <div>
        	@{
        		var appOptionsState = AppOptionsService.GetAppOptionsState();
        	
        		var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
        	}
        	
            @IconDotNetSolutionFolderFragment.Render(iconDriver)
            @DotNetSolutionFolder.DisplayName
        </div>
		
		
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewSolutionFolderRendererType,
			new Dictionary<string, object?>
			{
				{
					nameof(ITreeViewSolutionFolderRendererType.DotNetSolutionFolder),
					Item
				},
			});
	}*/

	public override Task LoadChildListAsync()
	{
		if (Item is null)
			return Task.CompletedTask;

		try
		{
			LinkChildren(ChildList, ChildList);
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

		return Task.CompletedTask;
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		var currentNode = (TreeViewNoType)this;

		// First, find the TreeViewSolution
		var infiniteLoopDebugLimitCounter = 0;
		
		while (currentNode is not TreeViewSolution && currentNode.Parent is not null)
		{
			if (++infiniteLoopDebugLimitCounter % 100_000 == 0)
			{
				if (currentNode.Parent is TreeViewSolutionFolder)
				{
					Console.WriteLine($"tree_view_solution_folder {((TreeViewSolutionFolder)currentNode.Parent).Item.ActualName} -- {Item.ActualName}");
				}
				else
				{
					Console.WriteLine($"tree_view_solution_folder parent_infinite_loop -- {Item.ActualName}");
				}
			}
			
			currentNode = currentNode.Parent;
		}

		if (currentNode is not TreeViewSolution treeViewSolution)
			return;

		var childTreeViewSolutionFolderList = new List<TreeViewSolutionFolder>();
		var childTreeViewCSharpProjectList = new List<TreeViewNamespacePath>();

		List<TreeViewNoType> childTreeViewList;
		
		if (Item.IsSlnx)
		{
			var selfStringNestedProjectEntryList = treeViewSolution.Item.StringNestedProjectEntryList
				.Where(x => x.SolutionFolderActualName == Item.ActualName)
				.ToList();
				
			var solutionFolderSelfStringNestedProjectEntryList = selfStringNestedProjectEntryList
				.Where(x => x.ChildIsSolutionFolder)
				.Select(x => x.ChildIdentifier)
				.ToList();
			
			var projectSelfStringNestedProjectEntryList = selfStringNestedProjectEntryList
				.Where(x => !x.ChildIsSolutionFolder)
				.Select(x => x.ChildIdentifier)
				.ToList();
			
			foreach (var otherSolutionFolder in treeViewSolution.Item.SolutionFolderList)
			{
				if (solutionFolderSelfStringNestedProjectEntryList.Contains(otherSolutionFolder.ActualName))
					childTreeViewSolutionFolderList.Add(ConstructTreeViewSolutionFolder(otherSolutionFolder));
			}
			
			foreach (var project in treeViewSolution.Item.DotNetProjectList)
			{
				if (projectSelfStringNestedProjectEntryList.Contains(project.RelativePathFromSolutionFileString))
					childTreeViewCSharpProjectList.Add(ConstructTreeViewCSharpProject((CSharpProjectModel)project));
			}
		}
		else
		{
			var nestedProjectEntries = treeViewSolution.Item.GuidNestedProjectEntryList
				.Where(x => x.SolutionFolderIdGuid == Item.ProjectIdGuid)
				.ToArray();
	
			var childProjectIds = nestedProjectEntries.Select(x => x.ChildProjectIdGuid).ToArray();
	
			var childMemberList = treeViewSolution.Item.DotNetProjectList.Select(x => (ISolutionMember)x).Union(treeViewSolution.Item.SolutionFolderList)
				.Where(x => childProjectIds.Contains(x.ProjectIdGuid))
				.ToArray();
	
			foreach (var member in childMemberList)
			{
				if (member.SolutionMemberKind == SolutionMemberKind.SolutionFolder)
					childTreeViewSolutionFolderList.Add(ConstructTreeViewSolutionFolder((SolutionFolder)member));
				else
					childTreeViewCSharpProjectList.Add(ConstructTreeViewCSharpProject((CSharpProjectModel)member));
			}
		}
		
		childTreeViewList =
			childTreeViewSolutionFolderList.OrderBy(x => x.Item.DisplayName).Select(x => (TreeViewNoType)x)
			.Union(childTreeViewCSharpProjectList.OrderBy(x => x.Item.AbsolutePath.NameNoExtension).Select(x => (TreeViewNoType)x))
			.ToList();
			
		for (int siblingsIndex = siblingsAndSelfTreeViews.Count - 1; siblingsIndex >= 0; siblingsIndex--)
		{
			var siblingOrSelf = siblingsAndSelfTreeViews[siblingsIndex];

			for (var childrensIndex = 0; childrensIndex < childTreeViewList.Count; childrensIndex++)
			{
				var childTreeView = childTreeViewList[childrensIndex];

				if (siblingOrSelf.Equals(childTreeView))
				{
					// What i'm doing here is super confusing and needs changed.
					// In lines above I re-created a TreeView node for a second time.
					//
					// Now I have to figure out where that re-created TreeView node
					// existed originally because it will have its
					// "RemoveRelatedFilesFromParent" invoked.
					//
					// Without this logic a:
					//     solution folder -> solution folder -> project
					//
					// Will not render the project.
					//
					// TODO: Revisit this logic.
					var originalTreeView = siblingsAndSelfTreeViews[siblingsIndex];

					originalTreeView.Parent = this;
					originalTreeView.IndexAmongSiblings = childrensIndex;
					originalTreeView.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();

					siblingsAndSelfTreeViews.RemoveAt(siblingsIndex);

					childTreeViewList[childrensIndex] = originalTreeView;
				}
				else
				{
					childTreeView.Parent = this;
					childTreeView.IndexAmongSiblings = childrensIndex;
					childTreeView.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
				}
			}
		}

		ChildList = childTreeViewList;
	}

	private TreeViewSolutionFolder ConstructTreeViewSolutionFolder(SolutionFolder dotNetSolutionFolder)
	{
		return new TreeViewSolutionFolder(
			dotNetSolutionFolder,
			DotNetComponentRenderers,
			IdeComponentRenderers,
			CommonService,
			true,
			false)
		{
			TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
		};
	}

	private TreeViewNamespacePath ConstructTreeViewCSharpProject(CSharpProjectModel cSharpProject)
	{
		var namespacePath = new NamespacePath(
			cSharpProject.AbsolutePath.NameNoExtension,
			cSharpProject.AbsolutePath);

		return new TreeViewNamespacePath(
			namespacePath,
			DotNetComponentRenderers,
			IdeComponentRenderers,
			CommonService,
			true,
			false)
		{
			TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
		};
	}
}