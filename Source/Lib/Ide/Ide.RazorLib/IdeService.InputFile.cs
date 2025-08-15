using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private InputFileState _inputFileState = new();

    public InputFileState GetInputFileState() => _inputFileState;

    public void InputFile_StartInputFileStateForm(
        string message,
        Func<AbsolutePath, Task> onAfterSubmitFunc,
        Func<AbsolutePath, Task<bool>> selectionIsValidFunc,
        List<InputFilePattern> inputFilePatterns)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectionIsValidFunc = selectionIsValidFunc,
                OnAfterSubmitFunc = onAfterSubmitFunc,
                InputFilePatternsList = inputFilePatterns,
                SelectedInputFilePattern = inputFilePatterns.First(),
                Message = message
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_SetSelectedTreeViewModel(TreeViewAbsolutePath? selectedTreeViewModel)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedTreeViewModel = selectedTreeViewModel
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_SetSelectedInputFilePattern(InputFilePattern inputFilePattern)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedInputFilePattern = inputFilePattern
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_MoveBackwardsInHistory()
    {
        lock (_stateModificationLock)
        {
            if (_inputFileState.CanMoveBackwardsInHistory)
            {
                _inputFileState = _inputFileState with { IndexInHistory = _inputFileState.IndexInHistory - 1 };
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_MoveForwardsInHistory()
    {
        lock (_stateModificationLock)
        {
            if (_inputFileState.CanMoveForwardsInHistory)
            {
                _inputFileState = _inputFileState with { IndexInHistory = _inputFileState.IndexInHistory + 1 };
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_OpenParentDirectory(
        CommonService commonService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            var currentSelection = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            // If has a ParentDirectory select it
            if (currentSelection.Item.ParentDirectory is not null)
            {
                var parentDirectory = currentSelection.Item.ParentDirectory;

                var parentDirectoryAbsolutePath = commonService.EnvironmentProvider.AbsolutePathFactory(
                    parentDirectory,
                    true,
                    tokenBuilder: new StringBuilder(),
                    formattedBuilder: new StringBuilder(),
                    AbsolutePathNameKind.NameWithExtension);

                parentDirectoryTreeViewModel = new TreeViewAbsolutePath(
                    parentDirectoryAbsolutePath,
                    commonService,
                    false,
                    true);
            }

            if (parentDirectoryTreeViewModel is not null)
            {
                _inputFileState = InputFileState.NewOpenedTreeViewModelHistory(
                    inState,
                    parentDirectoryTreeViewModel,
                    commonService);

                goto finalize;
            }

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_RefreshCurrentSelection(TreeViewAbsolutePath? currentSelection)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            currentSelection = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_SetSearchQuery(string searchQuery)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            var openedTreeViewModel = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            foreach (var treeViewModel in openedTreeViewModel.ChildList)
            {
                var treeViewAbsolutePath = (TreeViewAbsolutePath)treeViewModel;

                treeViewModel.IsHidden = !treeViewAbsolutePath.Item.Name.Contains(
                    searchQuery,
                    StringComparison.InvariantCultureIgnoreCase);
            }

            _inputFileState = inState with { SearchQuery = searchQuery };

            goto finalize;
        }

        finalize:
        IdeStateChanged?.Invoke(IdeStateChangedKind.InputFileStateChanged);
    }

    public void InputFile_Enqueue_OpenParentDirectoryAction(
        CommonService commonService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
        {
            Enqueue(new IdeWorkArgs
            {
                WorkKind = IdeWorkKind.OpenParentDirectoryAction,
                TreeViewAbsolutePath = parentDirectoryTreeViewModel
            });
        }
    }

    public async ValueTask InputFile_Do_OpenParentDirectoryAction(
        CommonService commonService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
            await parentDirectoryTreeViewModel.LoadChildListAsync().ConfigureAwait(false);
    }

    public void InputFile_Enqueue_RefreshCurrentSelectionAction(CommonService commonService, TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
        {
            currentSelection.ChildList.Clear();

            Enqueue(new IdeWorkArgs
            {
                WorkKind = IdeWorkKind.RefreshCurrentSelectionAction,
                TreeViewAbsolutePath = currentSelection
            });
        }
    }

    public async ValueTask InputFile_Do_RefreshCurrentSelectionAction(TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
            await currentSelection.LoadChildListAsync().ConfigureAwait(false);
    }
}
