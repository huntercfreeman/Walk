using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Models;

public class InputFileTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly IdeService _ideService;
    private readonly Func<AbsolutePath, Task> _setInputFileContentTreeViewRootFunc;
    private readonly Func<Task> _focusSearchInputElementFunc;
    private readonly Func<List<(Key<TreeViewContainer> treeViewStateKey, TreeViewAbsolutePath treeViewAbsolutePath)>> _getSearchMatchTuplesFunc;

    public InputFileTreeViewKeyboardEventHandler(
	        IdeService ideService,
	        Func<AbsolutePath, Task> setInputFileContentTreeViewRootFunc,
	        Func<Task> focusSearchInputElementFunc,
	        Func<List<(Key<TreeViewContainer> treeViewStateKey, TreeViewAbsolutePath treeViewAbsolutePath)>> getSearchMatchTuplesFunc)
        : base(ideService.CommonService)
    {
        _ideService = ideService;
        _setInputFileContentTreeViewRootFunc = setInputFileContentTreeViewRootFunc;
        _focusSearchInputElementFunc = focusSearchInputElementFunc;
        _getSearchMatchTuplesFunc = getSearchMatchTuplesFunc;
    }

    public override Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnKeyDownAsync(commandArgs);

        if (commandArgs.KeyboardEventArgs is null)
            return Task.CompletedTask;

        switch (commandArgs.KeyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                SetInputFileContentTreeViewRoot(commandArgs);
                return Task.CompletedTask;
            case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
                SetSelectedTreeViewModel(commandArgs);
                return Task.CompletedTask;
        }

        switch (commandArgs.KeyboardEventArgs.Key)
        {
            // Tried to have { "Ctrl" + "f" } => MoveFocusToSearchBar however, the webview was ending up taking over
            // and displaying its search bar with focus being set to it.
            //
            // Doing preventDefault just for this one case would be a can of worms as JSInterop is needed, as well a custom Blazor event.
            case "/":
            case "?":
                MoveFocusToSearchBar(commandArgs);
                return Task.CompletedTask;
                // TODO: Add move to next match and move to previous match
                //
                // case "*":
                //     treeViewCommand = new TreeViewCommand(SetNextMatchAsActiveTreeViewNode);
                //     return Task.CompletedTask true;
                // case "#":
                //     treeViewCommand = new TreeViewCommand(SetPreviousMatchAsActiveTreeViewNode);
                //     return Task.CompletedTask true;
        }

        if (commandArgs.KeyboardEventArgs.AltKey)
        {
            AltModifiedKeymap(commandArgs);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private void AltModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return;

        switch (commandArgs.KeyboardEventArgs.Key)
        {
            case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
                HandleBackButtonOnClick(commandArgs);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_UP:
                HandleUpwardButtonOnClick(commandArgs);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
                HandleForwardButtonOnClick(commandArgs);
                break;
            case "r":
                HandleRefreshButtonOnClick(commandArgs);
                break;
        }
    }

    private void SetInputFileContentTreeViewRoot(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewAbsolutePath treeViewAbsolutePath)
            return;

        _setInputFileContentTreeViewRootFunc.Invoke(treeViewAbsolutePath.Item);
    }

    private void HandleBackButtonOnClick(TreeViewCommandArgs commandArgs)
    {
        _ideService.InputFile_MoveBackwardsInHistory();
        ChangeContentRootToOpenedTreeView(_ideService.GetInputFileState());
    }

    private void HandleForwardButtonOnClick(TreeViewCommandArgs commandArgs)
    {
        _ideService.InputFile_MoveForwardsInHistory();
        ChangeContentRootToOpenedTreeView(_ideService.GetInputFileState());
    }

    private void HandleUpwardButtonOnClick(TreeViewCommandArgs commandArgs)
    {
        _ideService.InputFile_OpenParentDirectory(
            _ideService.IdeComponentRenderers,
            _ideService.CommonService,
            parentDirectoryTreeViewModel: null);

        ChangeContentRootToOpenedTreeView(_ideService.GetInputFileState());
    }

    private void HandleRefreshButtonOnClick(TreeViewCommandArgs commandArgs)
    {
        _ideService.InputFile_RefreshCurrentSelection(currentSelection: null);
        ChangeContentRootToOpenedTreeView(_ideService.GetInputFileState());
    }

    private void ChangeContentRootToOpenedTreeView(InputFileState inputFileState)
    {
        var openedTreeView = inputFileState.GetOpenedTreeView();

        if (openedTreeView?.Item is not null)
            _setInputFileContentTreeViewRootFunc.Invoke(openedTreeView.Item);
    }

    private void SetSelectedTreeViewModel(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;
        var treeViewAbsolutePath = activeNode as TreeViewAbsolutePath;

        if (treeViewAbsolutePath is null)
            return;

        _ideService.InputFile_SetSelectedTreeViewModel(treeViewAbsolutePath);
        return;
    }

    private void MoveFocusToSearchBar(TreeViewCommandArgs commandArgs)
    {
        _ = Task.Run(async () => await _focusSearchInputElementFunc.Invoke().ConfigureAwait(false))
            .ConfigureAwait(false);
    }
}