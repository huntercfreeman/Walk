using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
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
			IFileSystemProvider fileSystemProvider,
			IEnvironmentProvider environmentProvider,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectToProjectReferences, isExpandable, isExpanded)
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
		if (obj is not TreeViewCSharpProjectToProjectReferences otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode() => Item.CSharpProjectNamespacePath.AbsolutePath.Value.GetHashCode();

	public override TreeViewRenderer GetTreeViewRenderer()
	{
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectToProjectReferencesRendererType,
			null);
	}

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

		var projectReferences = cSharpProjectSyntaxWalker.TagNodes
			.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.Text ?? string.Empty) == "ProjectReference")
			.ToList();

		List<CSharpProjectToProjectReference> cSharpProjectToProjectReferences = new();

		foreach (var projectReference in projectReferences)
		{
			var attributeNameValueTuples = projectReference
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

			var referenceProjectAbsolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
				Item.CSharpProjectNamespacePath.AbsolutePath,
				includeAttribute.Item2,
				EnvironmentProvider);

			var referenceProjectAbsolutePath = EnvironmentProvider.AbsolutePathFactory(
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