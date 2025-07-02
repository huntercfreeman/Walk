using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Models;

public class InputFileService : IInputFileService, IBackgroundTaskGroup
{
    private readonly object _stateModificationLock = new();

    private InputFileState _inputFileState = new();
	
	public event Action? InputFileStateChanged;
	
	public InputFileState GetInputFileState() => _inputFileState;

    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly Queue<InputFileServiceWorkKind> _workKindQueue = new();
    private readonly object _workLock = new();

    public void StartInputFileStateForm(
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

        InputFileStateChanged?.Invoke();
    }

    public void SetSelectedTreeViewModel(TreeViewAbsolutePath? selectedTreeViewModel)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedTreeViewModel = selectedTreeViewModel
            };
        }

        InputFileStateChanged?.Invoke();
    }

    public void SetOpenedTreeViewModel(
    	TreeViewAbsolutePath treeViewModel,
        IIdeComponentRenderers ideComponentRenderers,
        ICommonUtilityService commonUtilityService)
    {
        lock (_stateModificationLock)
        {
            if (treeViewModel.Item.IsDirectory)
            {
                _inputFileState = InputFileState.NewOpenedTreeViewModelHistory(
                    _inputFileState,
                    treeViewModel,
                    ideComponentRenderers,
                    commonUtilityService);
            }
            else
            {
                _inputFileState = _inputFileState;
            }
        }

        InputFileStateChanged?.Invoke();
    }

    public void SetSelectedInputFilePattern(InputFilePattern inputFilePattern)
    {
        lock (_stateModificationLock)
        {
            _inputFileState = _inputFileState with
            {
                SelectedInputFilePattern = inputFilePattern
            };
        }

        InputFileStateChanged?.Invoke();
    }

    public void MoveBackwardsInHistory()
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

        InputFileStateChanged?.Invoke();
    }

    public void MoveForwardsInHistory()
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

        InputFileStateChanged?.Invoke();
    }

    public void OpenParentDirectory(
        IIdeComponentRenderers ideComponentRenderers,
        ICommonUtilityService commonUtilityService,
        BackgroundTaskService backgroundTaskService,
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

                var parentDirectoryAbsolutePath = commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                    parentDirectory,
                    true);

                parentDirectoryTreeViewModel = new TreeViewAbsolutePath(
                    parentDirectoryAbsolutePath,
                    ideComponentRenderers,
                    commonUtilityService,
                    false,
                    true);
            }

            if (parentDirectoryTreeViewModel is not null)
            {
                _inputFileState = InputFileState.NewOpenedTreeViewModelHistory(
                    inState,
                    parentDirectoryTreeViewModel,
                    ideComponentRenderers,
                    commonUtilityService);

                goto finalize;
            }

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    public void RefreshCurrentSelection(
    	BackgroundTaskService backgroundTaskService,
    	TreeViewAbsolutePath? currentSelection)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            currentSelection = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            _inputFileState = inState;

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    public void SetSearchQuery(string searchQuery)
    {
        lock (_stateModificationLock)
        {
            var inState = GetInputFileState();

            var openedTreeViewModel = inState.OpenedTreeViewModelHistoryList[inState.IndexInHistory];

            foreach (var treeViewModel in openedTreeViewModel.ChildList)
            {
                var treeViewAbsolutePath = (TreeViewAbsolutePath)treeViewModel;

                treeViewModel.IsHidden = !treeViewAbsolutePath.Item.NameWithExtension.Contains(
                    searchQuery,
                    StringComparison.InvariantCultureIgnoreCase);
            }

            _inputFileState = inState with { SearchQuery = searchQuery };

            goto finalize;
        }

        finalize:
        InputFileStateChanged?.Invoke();
    }

    private readonly
        Queue<(IIdeComponentRenderers ideComponentRenderers, ICommonUtilityService commonUtilityService, BackgroundTaskService backgroundTaskService, TreeViewAbsolutePath? parentDirectoryTreeViewModel)>
        _queue_OpenParentDirectoryAction = new();

    public void Enqueue_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        ICommonUtilityService commonUtilityService,
        BackgroundTaskService backgroundTaskService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
        {
            lock (_workLock)
            {
                _workKindQueue.Enqueue(InputFileServiceWorkKind.OpenParentDirectoryAction);

                _queue_OpenParentDirectoryAction.Enqueue((
                    ideComponentRenderers, commonUtilityService, backgroundTaskService, parentDirectoryTreeViewModel));

                backgroundTaskService.Continuous_EnqueueGroup(this);
            }
        }
    }
    
    public async ValueTask Do_OpenParentDirectoryAction(
    	IIdeComponentRenderers ideComponentRenderers,
        ICommonUtilityService commonUtilityService,
        BackgroundTaskService backgroundTaskService,
        TreeViewAbsolutePath? parentDirectoryTreeViewModel)
    {
        if (parentDirectoryTreeViewModel is not null)
            await parentDirectoryTreeViewModel.LoadChildListAsync().ConfigureAwait(false);
    }

    private readonly
        Queue<(BackgroundTaskService backgroundTaskService, TreeViewAbsolutePath? currentSelection)>
        _queue_RefreshCurrentSelectionAction = new();

    public void Enqueue_RefreshCurrentSelectionAction(
        BackgroundTaskService backgroundTaskService,
    	TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
        {
            currentSelection.ChildList.Clear();

            lock (_workLock)
            {
                _workKindQueue.Enqueue(InputFileServiceWorkKind.RefreshCurrentSelectionAction);
                _queue_RefreshCurrentSelectionAction.Enqueue((backgroundTaskService, currentSelection));
                backgroundTaskService.Continuous_EnqueueGroup(this);
            }
        }
    }
    
    public async ValueTask Do_RefreshCurrentSelectionAction(
        BackgroundTaskService backgroundTaskService,
    	TreeViewAbsolutePath? currentSelection)
    {
        if (currentSelection is not null)
            await currentSelection.LoadChildListAsync().ConfigureAwait(false);
    }

    public ValueTask HandleEvent()
    {
        InputFileServiceWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case InputFileServiceWorkKind.OpenParentDirectoryAction:
            {
                var args = _queue_OpenParentDirectoryAction.Dequeue();
                return Do_OpenParentDirectoryAction(
                    args.ideComponentRenderers, args.commonUtilityService, args.backgroundTaskService, args.parentDirectoryTreeViewModel);
            }
            case InputFileServiceWorkKind.RefreshCurrentSelectionAction:
            {
                var args = _queue_RefreshCurrentSelectionAction.Dequeue();
                return Do_RefreshCurrentSelectionAction(
                    args.backgroundTaskService, args.currentSelection);
            }
            default:
            {
                Console.WriteLine($"{nameof(InputFileService)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
            }
        }
    }
}
