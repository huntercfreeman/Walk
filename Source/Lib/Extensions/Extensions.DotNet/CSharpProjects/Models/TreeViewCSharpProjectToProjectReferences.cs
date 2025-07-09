using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.Xml.Html.SyntaxActors;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectToProjectReferences : TreeViewWithType<CSharpProjectToProjectReferences>
{
	public TreeViewCSharpProjectToProjectReferences(
			CSharpProjectToProjectReferences cSharpProjectToProjectReferences,
			IDotNetComponentRenderers dotNetComponentRenderers,
			IIdeComponentRenderers ideComponentRenderers,
			CommonUtilityService commonUtilityService,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectToProjectReferences, isExpandable, isExpanded)
	{
		DotNetComponentRenderers = dotNetComponentRenderers;
		IdeComponentRenderers = ideComponentRenderers;
		CommonUtilityService = commonUtilityService;
	}

	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public IIdeComponentRenderers IdeComponentRenderers { get; }
	public CommonUtilityService CommonUtilityService { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCSharpProjectToProjectReferences otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => Item.CSharpProjectNamespacePath.AbsolutePath.Value.GetHashCode();

	public override string GetDisplayText() => "Project References";
	
	public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconReferencesFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
	    using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectToProjectReferencesDisplay : ComponentBase
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
        
            @IconReferencesFragment.Render(iconDriver)
            Project References
        </div>
		
		
		
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectToProjectReferencesRendererType,
			null);
	}*/

	public override async Task LoadChildListAsync()
	{
		var previousChildren = new List<TreeViewNoType>(ChildList);

		var content = await CommonUtilityService.FileSystemProvider.File.ReadAllTextAsync(
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

		var projectReferences = cSharpProjectSyntaxWalker.TagNodes
			.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, textEditorService: null) ?? string.Empty) == "ProjectReference")
			.ToList();

		List<CSharpProjectToProjectReference> cSharpProjectToProjectReferences = new();

		foreach (var projectReference in projectReferences)
		{
			var attributeNameValueTuples = projectReference
				.AttributeNodes
				.Select(x => (
					x.AttributeNameSyntax.TextEditorTextSpan
						.GetText(content, textEditorService: null)
						.Trim(),
					x.AttributeValueSyntax.TextEditorTextSpan
						.GetText(content, textEditorService: null)
						.Replace("\"", string.Empty)
						.Replace("=", string.Empty)
						.Trim()))
				.ToArray();

			var includeAttribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Include");

			var referenceProjectAbsolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
				Item.CSharpProjectNamespacePath.AbsolutePath,
				includeAttribute.Item2,
				(IEnvironmentProvider)CommonUtilityService.EnvironmentProvider);

			var referenceProjectAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
				referenceProjectAbsolutePathString,
				false);

			var cSharpProjectToProjectReference = new CSharpProjectToProjectReference(
				Item.CSharpProjectNamespacePath,
				referenceProjectAbsolutePath);

			cSharpProjectToProjectReferences.Add(cSharpProjectToProjectReference);
		}

		var newChildList = cSharpProjectToProjectReferences
			.Select(x => (TreeViewNoType)new TreeViewCSharpProjectToProjectReference(
				x,
				DotNetComponentRenderers,
				IdeComponentRenderers,
				CommonUtilityService,
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