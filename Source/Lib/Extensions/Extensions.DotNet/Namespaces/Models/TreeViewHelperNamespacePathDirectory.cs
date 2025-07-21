using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Ide.RazorLib.Namespaces.Models;

public class TreeViewHelperNamespacePathDirectory
{
    /// <summary>Used with <see cref="TreeViewNamespacePath"/></summary>
    public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath directoryTreeView)
    {
        var directoryAbsolutePathString = directoryTreeView.Item.AbsolutePath.Value;

        var directoryPathStringsList = await directoryTreeView.CommonService.FileSystemProvider.Directory
            .GetDirectoriesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var childDirectoryTreeViewModels = directoryPathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                var absolutePath = directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, true);

                var namespaceString = directoryTreeView.Item.Namespace +
                    TreeViewNamespaceHelper.NAMESPACE_DELIMITER +
                    absolutePath.NameNoExtension;

                return (TreeViewNoType)new TreeViewNamespacePath(
                    new NamespacePath(namespaceString, absolutePath),
                    directoryTreeView.CommonService,
                    true,
                    false);
            });

        var filePathStringsList = await directoryTreeView.CommonService.FileSystemProvider.Directory
            .GetFilesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var childFileTreeViewModels = filePathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                var absolutePath = directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false);
                var namespaceString = directoryTreeView.Item.Namespace;

                return (TreeViewNoType)new TreeViewNamespacePath(
                    new NamespacePath(namespaceString, absolutePath),
                    directoryTreeView.CommonService,
                    false,
                    false);
            }).ToList();

        var result = new List<TreeViewNoType>();
        result.AddRange(childDirectoryTreeViewModels);
        result.AddRange(childFileTreeViewModels);
        
        return result;
    }
}
