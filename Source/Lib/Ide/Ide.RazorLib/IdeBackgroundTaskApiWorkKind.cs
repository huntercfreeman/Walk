namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public enum IdeWorkKind
{
	None,
    WalkIdeInitializerOnInit,
    IdeHeaderOnInit,
    FileContentsWereModifiedOnDisk,
    SaveFile,
    SetFolderExplorerState,
    SetFolderExplorerTreeView,
    RequestInputFileStateForm,
    
    // MenuOptionsFactoryWorkKind
    PerformNewFile,
    PerformNewDirectory,
    PerformDeleteFile,
    PerformCopyFile,
    PerformCutFile,
    PerformPasteFile
}
