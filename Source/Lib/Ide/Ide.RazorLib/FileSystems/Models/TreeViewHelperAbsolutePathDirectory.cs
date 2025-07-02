using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Ide.RazorLib.FileSystems.Models;

public class TreeViewHelperAbsolutePathDirectory
{
    public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewAbsolutePath directoryTreeView)
    {
        var directoryAbsolutePathString = directoryTreeView.Item.Value;

        var directoryPathStringsList = await directoryTreeView.CommonUtilityService.FileSystemProvider.Directory
            .GetDirectoriesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var childDirectoryTreeViewModels = directoryPathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                return (TreeViewNoType)new TreeViewAbsolutePath(
                    directoryTreeView.CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(x, true),
                    directoryTreeView.IdeComponentRenderers,
                    directoryTreeView.CommonUtilityService,
                    true,
                    false)
                {
                    TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
                };
            });

        var filePathStringsList = await directoryTreeView.CommonUtilityService.FileSystemProvider.Directory
            .GetFilesAsync(directoryAbsolutePathString)
            .ConfigureAwait(false);

        var childFileTreeViewModels = filePathStringsList
            .OrderBy(pathString => pathString)
            .Select(x =>
            {
                return (TreeViewNoType)new TreeViewAbsolutePath(
                    directoryTreeView.CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(x, false),
                    directoryTreeView.IdeComponentRenderers,
                    directoryTreeView.CommonUtilityService,
                    false,
                    false)
                {
                    TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
                };
            });

        return childDirectoryTreeViewModels.Union(childFileTreeViewModels).ToList();
    }
}