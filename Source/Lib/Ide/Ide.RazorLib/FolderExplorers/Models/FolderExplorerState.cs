using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Models;

public record struct FolderExplorerState(
    AbsolutePath? AbsolutePath,
    bool IsLoadingFolderExplorer)
{
    public static readonly Key<TreeViewContainer> TreeViewContentStateKey = Key<TreeViewContainer>.NewKey();

    public FolderExplorerState() : this(
        default,
        false)
    {

    }
}