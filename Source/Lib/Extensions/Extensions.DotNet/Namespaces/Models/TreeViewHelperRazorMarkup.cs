using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewHelperRazorMarkup
{
	/// <summary>
	/// This method is used in order to allow for collapsing and expanding the node to refresh
	/// the codebehinds.
	///
	/// This method is not the same as <see cref="FindRelatedFiles"/>.
	/// </summary>
	public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath razorMarkupTreeView)
	{
		if (razorMarkupTreeView.Item.Namespace is null)
			return new();

		var parentDirectoryOfRazorMarkup = razorMarkupTreeView.Item.AbsolutePath.ParentDirectory;
		var ancestorDirectory = parentDirectoryOfRazorMarkup;

		var filePathStringsList = await razorMarkupTreeView.CommonService.FileSystemProvider.Directory
			.GetFilesAsync(ancestorDirectory)
			.ConfigureAwait(false);

		var childFileTreeViewModels = filePathStringsList
			.Select(x =>
			{
				var absolutePath = razorMarkupTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false);
				var namespaceString = razorMarkupTreeView.Item.Namespace;

				return (TreeViewNoType)new TreeViewNamespacePath(
					new NamespacePath(namespaceString, absolutePath),
					razorMarkupTreeView.DotNetComponentRenderers,
					razorMarkupTreeView.IdeComponentRenderers,
					razorMarkupTreeView.CommonService,
					false,
					false);
			}).ToList();

		FindRelatedFiles(razorMarkupTreeView, childFileTreeViewModels);
		return razorMarkupTreeView.ChildList;
	}

	/// <summary>
	/// This method is used in order to remove from the parent node, various siblings
	/// which will be moved to be a child node of the '.razor' file.
	///
	/// This is method not the same as <see cref="LoadChildrenAsync"/>.
	/// </summary>
	public static void FindRelatedFiles(
		TreeViewNamespacePath razorMarkupTreeView,
		List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		razorMarkupTreeView.ChildList.Clear();

		// .razor files look to remove .razor.cs and .razor.css files
		var matches = new[]
		{
			razorMarkupTreeView.Item.AbsolutePath.NameWithExtension + '.' + ExtensionNoPeriodFacts.C_SHARP_CLASS,
			razorMarkupTreeView.Item.AbsolutePath.NameWithExtension + '.' + ExtensionNoPeriodFacts.CSS
		};

		var relatedFiles = siblingsAndSelfTreeViews
			.Where(x =>
				x.UntypedItem is NamespacePath namespacePath &&
				matches.Contains(namespacePath.AbsolutePath.NameWithExtension))
			.OrderBy(x =>
			{
				if (x.UntypedItem is NamespacePath namespacePath)
					return namespacePath.AbsolutePath.NameWithExtension ?? string.Empty;
				else
					return string.Empty;
			})
			.ToArray();

		if (!relatedFiles.Any())
		{
			razorMarkupTreeView.IsExpandable = false;
			razorMarkupTreeView.IsExpanded = false;
			razorMarkupTreeView.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
			return;
		}

		// TODO: use 'TreeViewNoType.LinkChildren(List<TreeViewNoType> previousChildList, List<TreeViewNoType> nextChildList)'?
		for (var index = 0; index < relatedFiles.Length; index++)
		{
			var relatedFile = relatedFiles[index];

			siblingsAndSelfTreeViews.Remove(relatedFile);

			relatedFile.Parent = razorMarkupTreeView;
			relatedFile.IndexAmongSiblings = index;
			relatedFile.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();

			razorMarkupTreeView.ChildList.Add(relatedFile);
		}

		razorMarkupTreeView.IsExpandable = true;
		razorMarkupTreeView.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
	}
}