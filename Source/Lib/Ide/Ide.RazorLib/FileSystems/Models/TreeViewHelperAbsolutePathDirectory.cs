using System.Text;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Ide.RazorLib.FileSystems.Models;

public class TreeViewHelperAbsolutePathDirectory
{
    public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewAbsolutePath directoryTreeView)
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
                return (TreeViewNoType)new TreeViewAbsolutePath(
                    directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, true, tokenBuilder, formattedBuilder),
                    directoryTreeView.CommonService,
                    true,
                    false)
                {
                    TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
                };
            });

        var filePathStringsList = await directoryTreeView.CommonService.FileSystemProvider.Directory
            .GetFilesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var childFileTreeViewModels = filePathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                return (TreeViewNoType)new TreeViewAbsolutePath(
                    directoryTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false, tokenBuilder, formattedBuilder),
                    directoryTreeView.CommonService,
                    false,
                    false)
                {
                    TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
                };
            });

        return childDirectoryTreeViewModels.Union(childFileTreeViewModels).ToList();
    }
}
