using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Models;

public record struct InputFileState(
    int IndexInHistory,
	IReadOnlyList<TreeViewAbsolutePath> OpenedTreeViewModelHistoryList,
    TreeViewAbsolutePath? SelectedTreeViewModel,
    Func<AbsolutePath, Task> OnAfterSubmitFunc,
    Func<AbsolutePath, Task<bool>> SelectionIsValidFunc,
    IReadOnlyList<InputFilePattern> InputFilePatternsList,
    InputFilePattern? SelectedInputFilePattern,
    string SearchQuery,
    string Message)
{
    public InputFileState() : this(
        -1,
		Array.Empty<TreeViewAbsolutePath>(),
        null,
        _ => Task.CompletedTask,
        _ => Task.FromResult(false),
		Array.Empty<InputFilePattern>(),
        null,
        string.Empty,
        string.Empty)
    {
    }
    
    public bool CanMoveBackwardsInHistory => IndexInHistory > 0;
    public bool CanMoveForwardsInHistory => IndexInHistory < OpenedTreeViewModelHistoryList.Count - 1;

    public TreeViewAbsolutePath? GetOpenedTreeView()
    {
        if (IndexInHistory == -1 || IndexInHistory >= OpenedTreeViewModelHistoryList.Count)
            return null;

        return OpenedTreeViewModelHistoryList[IndexInHistory];
    }

    public static InputFileState NewOpenedTreeViewModelHistory(
        InputFileState inInputFileState,
        TreeViewAbsolutePath selectedTreeViewModel,
        IIdeComponentRenderers ideComponentRenderers,
        ICommonUtilityService commonUtilityService)
    {
        var selectionClone = new TreeViewAbsolutePath(
            selectedTreeViewModel.Item,
            ideComponentRenderers,
            commonUtilityService,
            false,
            true);

        selectionClone.IsExpanded = true;
        selectionClone.ChildList = selectedTreeViewModel.ChildList;
        
        var nextHistory = new List<TreeViewAbsolutePath>(inInputFileState.OpenedTreeViewModelHistoryList);

        // If not at end of history the more recent history is
        // replaced by the to be selected TreeViewModel
        if (inInputFileState.IndexInHistory != inInputFileState.OpenedTreeViewModelHistoryList.Count - 1)
        {
            var historyCount = inInputFileState.OpenedTreeViewModelHistoryList.Count;
            var startingIndexToRemove = inInputFileState.IndexInHistory + 1;
            var countToRemove = historyCount - startingIndexToRemove;

			nextHistory.RemoveRange(
                startingIndexToRemove,
                countToRemove);
        }

        nextHistory.Add(selectionClone);

        return inInputFileState with
        {
            IndexInHistory = inInputFileState.IndexInHistory + 1,
            OpenedTreeViewModelHistoryList = nextHistory,
        };
    }
}