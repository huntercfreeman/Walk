using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Options.Models;
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
        CommonUtilityService commonUtilityService);

    public void SetSelectedInputFilePattern(InputFilePattern inputFilePattern);

    public void MoveBackwardsInHistory();

    public void MoveForwardsInHistory();

    public void OpenParentDirectory(
        IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel);

    public void RefreshCurrentSelection(TreeViewAbsolutePath? currentSelection);

    public void SetSearchQuery(string searchQuery);
    
    public void Enqueue_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        CommonUtilityService commonUtilityService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel);
    
    public void Enqueue_RefreshCurrentSelectionAction(CommonUtilityService commonUtilityService, TreeViewAbsolutePath? currentSelection);
}
