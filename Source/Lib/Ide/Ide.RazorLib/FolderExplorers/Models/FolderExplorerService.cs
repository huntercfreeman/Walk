namespace Walk.Ide.RazorLib.FolderExplorers.Models;

public class FolderExplorerService : IFolderExplorerService
{
    private readonly object _stateModificationLock = new();

    private FolderExplorerState _folderExplorerState = new();
	
	public event Action? FolderExplorerStateChanged;
	
	public FolderExplorerState GetFolderExplorerState() => _folderExplorerState;

    public void With(Func<FolderExplorerState, FolderExplorerState> withFunc)
    {
        lock (_stateModificationLock)
        {
            _folderExplorerState = withFunc.Invoke(_folderExplorerState);
        }

        FolderExplorerStateChanged?.Invoke();
    }
}
