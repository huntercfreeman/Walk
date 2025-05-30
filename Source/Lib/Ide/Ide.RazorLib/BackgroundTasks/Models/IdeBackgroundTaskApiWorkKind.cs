namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public enum IdeBackgroundTaskApiWorkKind
{
	None,
    WalkIdeInitializerOnInit,
    IdeHeaderOnInit,
    FileContentsWereModifiedOnDisk,
    SaveFile,
    SetFolderExplorerState,
    SetFolderExplorerTreeView,
    RequestInputFileStateForm,
}
