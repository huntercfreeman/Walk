using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Models;

public class FolderExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;
    private readonly TextEditorService _textEditorService;
    private readonly IMenuOptionsFactory _menuOptionsFactory;
    private readonly CommonUtilityService _commonUtilityService;

    public FolderExplorerTreeViewKeyboardEventHandler(
            IdeBackgroundTaskApi ideBackgroundTaskApi,
            TextEditorService textEditorService,
            IMenuOptionsFactory menuOptionsFactory,
            CommonUtilityService commonUtilityService)
        : base(commonUtilityService)
    {
        _ideBackgroundTaskApi = ideBackgroundTaskApi;
        _textEditorService = textEditorService;
        _menuOptionsFactory = menuOptionsFactory;
        _commonUtilityService = commonUtilityService;
    }

    public override Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return Task.CompletedTask;

        base.OnKeyDownAsync(commandArgs);

        switch (commandArgs.KeyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                return InvokeOpenInEditorAsync(commandArgs, true);
            case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
                return InvokeOpenInEditorAsync(commandArgs, false);
        }

        if (commandArgs.KeyboardEventArgs.CtrlKey)
            CtrlModifiedKeymap(commandArgs);
        else if (commandArgs.KeyboardEventArgs.AltKey)
            AltModifiedKeymap(commandArgs);

        return Task.CompletedTask;
    }

    private void CtrlModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return;

        if (commandArgs.KeyboardEventArgs.AltKey)
        {
            CtrlAltModifiedKeymap(commandArgs);
        }
        else
        {
            switch (commandArgs.KeyboardEventArgs.Key)
            {
                case "c":
                    CopyFile(commandArgs);
                    return;
                case "x":
                    CutFile(commandArgs);
                    return;
                case "v":
                    PasteClipboard(commandArgs);
                    return;
            }
        }
    }

    /// <summary>
    /// Do not go from <see cref="AltModifiedKeymap" /> to <see cref="CtrlAltModifiedKeymap" />
    /// <br /><br />
    /// Code in this method should only be here if it does not include a Ctrl key being pressed.
    /// <br /><br />
    /// As otherwise, we'd have to permute over all the possible keyboard modifier keys and have a method for each permutation.
    /// </summary>
    private void AltModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        return;
    }

    private void CtrlAltModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        return;
    }

    private Task CopyFile(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewAbsolutePath treeViewAbsolutePath)
            return Task.CompletedTask;

        var copyFileMenuOption = _menuOptionsFactory.CopyFile(
            treeViewAbsolutePath.Item,
            () =>
            {
                NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewAbsolutePath.Item.NameWithExtension}", _commonUtilityService, TimeSpan.FromSeconds(7));
                return Task.CompletedTask;
            });

        if (copyFileMenuOption.OnClickFunc is null)
            return Task.CompletedTask;

        return copyFileMenuOption.OnClickFunc.Invoke();
    }

    private Task PasteClipboard(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewAbsolutePath treeViewAbsolutePath)
            return Task.CompletedTask;

        MenuOptionRecord pasteMenuOptionRecord;

        if (treeViewAbsolutePath.Item.IsDirectory)
        {
            pasteMenuOptionRecord = _menuOptionsFactory.PasteClipboard(
                treeViewAbsolutePath.Item,
                async () =>
                {
                    var localParentOfCutFile = FolderExplorerContextMenu.ParentOfCutFile;
                    FolderExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewAbsolutePath).ConfigureAwait(false);
                });
        }
        else
        {
            var parentDirectory = treeViewAbsolutePath.Item.ParentDirectory;

            var parentDirectoryAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true);

            pasteMenuOptionRecord = _menuOptionsFactory.PasteClipboard(
                parentDirectoryAbsolutePath,
                async () =>
                {
                    var localParentOfCutFile = FolderExplorerContextMenu.ParentOfCutFile;
                    FolderExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewAbsolutePath).ConfigureAwait(false);
                });
        }

        if (pasteMenuOptionRecord.OnClickFunc is null)
            return Task.CompletedTask;

        return pasteMenuOptionRecord.OnClickFunc.Invoke();
    }

    private Task CutFile(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewAbsolutePath treeViewAbsolutePath)
            return Task.CompletedTask;

        var parent = treeViewAbsolutePath.Parent as TreeViewAbsolutePath;

        MenuOptionRecord cutFileOptionRecord = _menuOptionsFactory.CutFile(
            treeViewAbsolutePath.Item,
            () =>
            {
                FolderExplorerContextMenu.ParentOfCutFile = parent;
                NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewAbsolutePath.Item.NameWithExtension}", _commonUtilityService, TimeSpan.FromSeconds(7));
                return Task.CompletedTask;
            });

        if (cutFileOptionRecord.OnClickFunc is null)
            return Task.CompletedTask;

        return cutFileOptionRecord.OnClickFunc.Invoke();
    }

    private Task InvokeOpenInEditorAsync(TreeViewCommandArgs commandArgs, bool shouldSetFocusToEditor)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewAbsolutePath treeViewAbsolutePath)
            return Task.CompletedTask;

		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			await _textEditorService.OpenInEditorAsync(
				editContext,
				treeViewAbsolutePath.Item.Value,
				shouldSetFocusToEditor,
				cursorPositionIndex: null,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
		});
		return Task.CompletedTask;
    }

    private async Task ReloadTreeViewModel(TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildListAsync().ConfigureAwait(false);

        _commonUtilityService.TreeView_ReRenderNodeAction(
            FolderExplorerState.TreeViewContentStateKey,
            treeViewModel);

        _commonUtilityService.TreeView_MoveUpAction(
            FolderExplorerState.TreeViewContentStateKey,
            false,
			false);
    }
}