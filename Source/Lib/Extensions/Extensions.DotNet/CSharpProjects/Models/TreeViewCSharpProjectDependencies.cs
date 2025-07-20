using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectDependencies : TreeViewWithType<CSharpProjectDependencies>
{
	public TreeViewCSharpProjectDependencies(
			CSharpProjectDependencies cSharpProjectDependencies,
			IDotNetComponentRenderers dotNetComponentRenderers,
			CommonService commonService,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectDependencies, isExpandable, isExpanded)
	{
		DotNetComponentRenderers = dotNetComponentRenderers;
		CommonService = commonService;
	}

	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public CommonService CommonService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCSharpProjectDependencies otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => Item.CSharpProjectNamespacePath.AbsolutePath.Value.GetHashCode();

	public override string GetDisplayText() => "Dependencies";
	
	public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconProjectDependenciesFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectDependenciesDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
        }
	    
	
	
        <div>

        	@{
        		var appOptionsState = AppOptionsService.GetAppOptionsState();
        	
        		var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
        	}
        
            @IconProjectDependenciesFragment.Render(iconDriver)
            Dependencies
        </div>
	
	
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectDependenciesRendererType,
			null);
	}*/

	public override Task LoadChildListAsync()
	{
		var previousChildren = new List<TreeViewNoType>(ChildList);

		var treeViewCSharpProjectNugetPackageReferences = new TreeViewCSharpProjectNugetPackageReferences(
			new CSharpProjectNugetPackageReferences(Item.CSharpProjectNamespacePath),
			DotNetComponentRenderers,
			CommonService,
			true,
			false)
		{
			TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
		};

		var treeViewCSharpProjectToProjectReferences = new TreeViewCSharpProjectToProjectReferences(
			new CSharpProjectToProjectReferences(Item.CSharpProjectNamespacePath),
			DotNetComponentRenderers,
			CommonService,
			true,
			false)
		{
			TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
		};

		var newChildList = new List<TreeViewNoType>
		{
			treeViewCSharpProjectNugetPackageReferences,
			treeViewCSharpProjectToProjectReferences
		};

		ChildList = newChildList;
		LinkChildren(previousChildren, ChildList);

		TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
		return Task.CompletedTask;
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}