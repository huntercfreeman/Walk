namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public enum IdeWorkKind
{
    None,
    IdeHeaderOnInit,
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
