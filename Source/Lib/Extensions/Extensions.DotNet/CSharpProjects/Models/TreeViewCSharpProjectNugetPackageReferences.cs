using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.Xml.Html.SyntaxActors;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectNugetPackageReferences : TreeViewWithType<CSharpProjectNugetPackageReferences>
{
	public TreeViewCSharpProjectNugetPackageReferences(
			CSharpProjectNugetPackageReferences cSharpProjectNugetPackageReferences,
			IDotNetComponentRenderers dotNetComponentRenderers,
			IIdeComponentRenderers ideComponentRenderers,
			IFileSystemProvider fileSystemProvider,
			IEnvironmentProvider environmentProvider,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectNugetPackageReferences, isExpandable, isExpanded)
	{
		DotNetComponentRenderers = dotNetComponentRenderers;
		IdeComponentRenderers = ideComponentRenderers;
		FileSystemProvider = fileSystemProvider;
		EnvironmentProvider = environmentProvider;
	}

	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public IIdeComponentRenderers IdeComponentRenderers { get; }
	public IFileSystemProvider FileSystemProvider { get; }
	public IEnvironmentProvider EnvironmentProvider { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCSharpProjectNugetPackageReferences otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => Item.CSharpProjectNamespacePath.AbsolutePath.Value.GetHashCode();

	public override string GetDisplayText() => "NuGet Packages";

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectNugetPackageReferencesDisplay : ComponentBase
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
        
            @IconNuGetPackagesFragment.Render(iconDriver)
            NuGet Packages
        </div>
		
		
		
		
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectNugetPackageReferencesRendererType,
			null);
	}*/

	public override async Task LoadChildListAsync()
	{
		var previousChildren = new List<TreeViewNoType>(ChildList);

		var content = await FileSystemProvider.File.ReadAllTextAsync(
				Item.CSharpProjectNamespacePath.AbsolutePath.Value)
			.ConfigureAwait(false);

		var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
			textEditorService: null,
			new StringWalker(),
			new(Item.CSharpProjectNamespacePath.AbsolutePath.Value),
			content);

		var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

		var cSharpProjectSyntaxWalker = new CSharpProjectSyntaxWalker();

		cSharpProjectSyntaxWalker.Visit(syntaxNodeRoot);

		var packageReferences = cSharpProjectSyntaxWalker.TagNodes
			.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.Text ?? string.Empty) == "PackageReference")
			.ToList();

		List<LightWeightNugetPackageRecord> lightWeightNugetPackageRecords = new();

		foreach (var packageReference in packageReferences)
		{
			var attributeNameValueTuples = packageReference
				.AttributeNodes
				.Select(x => (
					x.AttributeNameSyntax.TextEditorTextSpan
						.Text
						.Trim(),
					x.AttributeValueSyntax.TextEditorTextSpan
						.Text
						.Replace("\"", string.Empty)
						.Replace("=", string.Empty)
						.Trim()))
				.ToArray();

			var includeAttribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Include");
			var versionAttribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Version");

			var lightWeightNugetPackageRecord = new LightWeightNugetPackageRecord(
				includeAttribute.Item2,
				includeAttribute.Item2,
				versionAttribute.Item2);

			lightWeightNugetPackageRecords.Add(lightWeightNugetPackageRecord);
		}

		var cSharpProjectAbsolutePathString = Item.CSharpProjectNamespacePath.AbsolutePath.Value;

		var newChildList = lightWeightNugetPackageRecords.Select(
			npr => (TreeViewNoType)new TreeViewCSharpProjectNugetPackageReference(
				new(cSharpProjectAbsolutePathString, npr),
				DotNetComponentRenderers,
				IdeComponentRenderers,
				FileSystemProvider,
				EnvironmentProvider,
				false,
				false)
			{
				TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
			})
			.ToList();


		ChildList = newChildList;
		LinkChildren(previousChildren, ChildList);
		TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}