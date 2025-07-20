using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectToProjectReference : TreeViewWithType<CSharpProjectToProjectReference>
{
	public TreeViewCSharpProjectToProjectReference(
			CSharpProjectToProjectReference cSharpProjectToProjectReference,
			CommonService commonService,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectToProjectReference, isExpandable, isExpanded)
	{
		CommonService = commonService;
	}

	public CommonService CommonService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCSharpProjectToProjectReference otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode()
	{
		var modifyProjectAbsolutePathString = Item.ModifyProjectNamespacePath.AbsolutePath.Value;
		var referenceProjectAbsolutePathString = Item.ReferenceProjectAbsolutePath.Value;

		var uniqueAbsolutePathString = modifyProjectAbsolutePathString + referenceProjectAbsolutePathString;
		return uniqueAbsolutePathString.GetHashCode();
	}

	public override string GetDisplayText() => Item.ReferenceProjectAbsolutePath.NameWithExtension;
	
	public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconGoToFileFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
		
		
		<div>

        	@{
        		var appOptionsState = AppOptionsService.GetAppOptionsState();
        	
        		var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
        	}
        
            @IconGoToFileFragment.Render(iconDriver)
            @CSharpProjectToProjectReference.ReferenceProjectAbsolutePath.NameWithExtension
        </div>
		
		using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        using Walk.CompilerServices.DotNetSolution.Models.Project;
        using Walk.Extensions.DotNet.ComponentRenderers.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectToProjectReferenceDisplay : ComponentBase, ITreeViewCSharpProjectToProjectReferenceRendererType
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
        	[Parameter, EditorRequired]
        	public CSharpProjectToProjectReference CSharpProjectToProjectReference { get; set; } = null!;
        }
		
		
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectToProjectReferenceRendererType,
			new Dictionary<string, object?>
			{
				{
					nameof(ITreeViewCSharpProjectToProjectReferenceRendererType.CSharpProjectToProjectReference),
					Item
				},
			});
	}*/

	public override Task LoadChildListAsync()
	{
		TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
		return Task.CompletedTask;
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}