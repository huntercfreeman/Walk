using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Models;

public interface IInputFileService
{
	public event Action? InputFileStateChanged;
	
	public InputFileState GetInputFileState();

    public void StartInputFileStateForm(
        string message,
        Func<AbsolutePath, Task> onAfterSubmitFunc,
        Func<AbsolutePath, Task<bool>> selectionIsValidFunc,
        List<InputFilePattern> inputFilePatterns);

    public void SetSelectedTreeViewModel(TreeViewAbsolutePath? SelectedTreeViewModel);

    public void SetOpenedTreeViewModel(
    	TreeViewAbsolutePath treeViewModel,
        IIdeComponentRenderers ideComponentRenderers,
        ICommonComponentRenderers commonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider);

    public void SetSelectedInputFilePattern(InputFilePattern inputFilePattern);

    public void MoveBackwardsInHistory();

    public void MoveForwardsInHistory();

    public void OpenParentDirectory(
        IIdeComponentRenderers ideComponentRenderers,
        ICommonComponentRenderers commonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        BackgroundTaskService backgroundTaskService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel);

    public void RefreshCurrentSelection(
    	BackgroundTaskService backgroundTaskService,
    	TreeViewAbsolutePath? currentSelection);

    public void SetSearchQuery(string searchQuery);
    
    public void Enqueue_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        ICommonComponentRenderers commonComponentRenderers,
        IFileSystemProvider fileSystemProvider,
        IEnvironmentProvider environmentProvider,
        BackgroundTaskService backgroundTaskService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel);
    
    public void Enqueue_RefreshCurrentSelectionAction(
        BackgroundTaskService backgroundTaskService,
    	TreeViewAbsolutePath? currentSelection);
}
