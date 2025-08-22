using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;

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
        var parentDirectoryOfRazorMarkup = razorMarkupTreeView.Item.CreateSubstringParentDirectory();
        if (parentDirectoryOfRazorMarkup is null)
            return razorMarkupTreeView.ChildList;
        
        var ancestorDirectory = parentDirectoryOfRazorMarkup;

        var filePathStringsList = await razorMarkupTreeView.CommonService.FileSystemProvider.Directory
            .GetFilesAsync(ancestorDirectory)
            .ConfigureAwait(false);

        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
        
        var childFileTreeViewModels = filePathStringsList
            .Select(x =>
            {
                var absolutePath = razorMarkupTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameWithExtension);

                return (TreeViewNoType)new TreeViewNamespacePath(
                    absolutePath,
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
            razorMarkupTreeView.Item.Name + '.' + CommonFacts.C_SHARP_CLASS,
            razorMarkupTreeView.Item.Name + '.' + CommonFacts.CSS
        };

        var relatedFiles = siblingsAndSelfTreeViews
            .Where(x =>
                x.UntypedItem is AbsolutePath absolutePath &&
                matches.Contains(absolutePath.Name))
            .OrderBy(x =>
            {
                if (x.UntypedItem is AbsolutePath absolutePath)
                    return absolutePath.Name ?? string.Empty;
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
