using Walk.Ide.RazorLib.FolderExplorers.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private FolderExplorerState _folderExplorerState = new();

    public FolderExplorerState GetFolderExplorerState() => _folderExplorerState;

    public void FolderExplorer_With(Func<FolderExplorerState, FolderExplorerState> withFunc)
    {
        lock (_stateModificationLock)
        {
            _folderExplorerState = withFunc.Invoke(_folderExplorerState);
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.FolderExplorerStateChanged);
    }
}
