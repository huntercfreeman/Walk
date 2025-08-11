namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public enum IdeWorkKind
{
    None,
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
    PerformPasteFile,
    
    // InputFileServiceWorkKind
    OpenParentDirectoryAction,
    RefreshCurrentSelectionAction,
}
