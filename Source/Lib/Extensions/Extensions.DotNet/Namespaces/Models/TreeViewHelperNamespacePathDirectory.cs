using System.Text;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Ide.RazorLib.Namespaces.Models;

public class TreeViewHelperNamespacePathDirectory
{
    /// <summary>Used with <see cref="TreeViewNamespacePath"/></summary>
    public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath directoryTreeView)
    {
        var directoryAbsolutePathString = directoryTreeView.Item.Value;

        var directoryPathStringsList = await directoryTreeView.CommonService.FileSystemProvider.Directory
            .GetDirectoriesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
        
        var childDirectoryTreeViewModels = directoryPathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                var absolutePath = directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, true, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameNoExtension);

                return (TreeViewNoType)new TreeViewNamespacePath(
                    absolutePath,
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
                var absolutePath = directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameWithExtension);

                return (TreeViewNoType)new TreeViewNamespacePath(
                    absolutePath,
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
