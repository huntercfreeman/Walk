using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectNugetPackageReference : TreeViewWithType<CSharpProjectNugetPackageReference>
{
	public TreeViewCSharpProjectNugetPackageReference(
			CSharpProjectNugetPackageReference cSharpProjectNugetPackageReference,
			IDotNetComponentRenderers dotNetComponentRenderers,
			IIdeComponentRenderers ideComponentRenderers,
			ICommonUtilityService commonUtilityService,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectNugetPackageReference, isExpandable, isExpanded)
	{
		DotNetComponentRenderers = dotNetComponentRenderers;
		IdeComponentRenderers = ideComponentRenderers;
		CommonUtilityService = commonUtilityService;
	}

	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public IIdeComponentRenderers IdeComponentRenderers { get; }
	public ICommonUtilityService CommonUtilityService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCSharpProjectNugetPackageReference otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode()
	{
		var uniqueString = Item.CSharpProjectAbsolutePathString + Item.LightWeightNugetPackageRecord.Id;
		return uniqueString.GetHashCode();
	}

	public override string GetDisplayText() => $"{Item.LightWeightNugetPackageRecord.Title}/{Item.LightWeightNugetPackageRecord.Version}";
	
	public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconPackageFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        using Walk.Extensions.DotNet.Nugets.Models;
        using Walk.Extensions.DotNet.ComponentRenderers.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectNugetPackageReferenceDisplay : ComponentBase, ITreeViewCSharpProjectNugetPackageReferenceRendererType
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
        	[Parameter, EditorRequired]
        	public CSharpProjectNugetPackageReference CSharpProjectNugetPackageReference { get; set; } = null!;
        }
	
	
	    <div>
        
        	@{
        		var appOptionsState = AppOptionsService.GetAppOptionsState();
        	
        		var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
        	}
        
            @IconPackageFragment.Render(iconDriver)
            @CSharpProjectNugetPackageReference.LightWeightNugetPackageRecord.Title<!--
            -->/<!--
            -->@CSharpProjectNugetPackageReference.LightWeightNugetPackageRecord.Version
        </div>
	
	
	
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectNugetPackageReferenceRendererType,
			new Dictionary<string, object?>
			{
				{
					nameof(ITreeViewCSharpProjectNugetPackageReferenceRendererType.CSharpProjectNugetPackageReference),
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