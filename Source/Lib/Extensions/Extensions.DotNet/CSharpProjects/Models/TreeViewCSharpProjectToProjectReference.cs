using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectToProjectReference : TreeViewWithType<CSharpProjectToProjectReference>
{
	public TreeViewCSharpProjectToProjectReference(
			CSharpProjectToProjectReference cSharpProjectToProjectReference,
			IDotNetComponentRenderers dotNetComponentRenderers,
			IIdeComponentRenderers ideComponentRenderers,
			IFileSystemProvider fileSystemProvider,
			IEnvironmentProvider environmentProvider,
			bool isExpandable,
			bool isExpanded)
		: base(cSharpProjectToProjectReference, isExpandable, isExpanded)
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

	public override TreeViewRenderer GetTreeViewRenderer()
	{
		return new TreeViewRenderer(
			DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectToProjectReferenceRendererType,
			new Dictionary<string, object?>
			{
				{
					nameof(ITreeViewCSharpProjectToProjectReferenceRendererType.CSharpProjectToProjectReference),
					Item
				},
			});
	}

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