using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

public class SolutionExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly IdeService _ideService;

    public SolutionExplorerTreeViewKeyboardEventHandler(IdeService ideService)
        : base(ideService.CommonService)
    {
        _ideService = ideService;
    }

    public override Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return Task.CompletedTask;

        base.OnKeyDownAsync(commandArgs);

        switch (commandArgs.KeyboardEventArgs.Code)
        {
            case CommonFacts.ENTER_CODE:
                return InvokeOpenInEditor(commandArgs, true);
            case CommonFacts.SPACE_CODE:
                return InvokeOpenInEditor(commandArgs, false);
        }

        if (commandArgs.KeyboardEventArgs.CtrlKey)
        {
            CtrlModifiedKeymap(commandArgs);
            return Task.CompletedTask;
        }
        else if (commandArgs.KeyboardEventArgs.AltKey)
        {
            AltModifiedKeymap(commandArgs);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private void CtrlModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return;

        if (commandArgs.KeyboardEventArgs.AltKey)
        {
            CtrlAltModifiedKeymap(commandArgs);
            return;
        }

        switch (commandArgs.KeyboardEventArgs.Key)
        {
            case "c":
                InvokeCopyFile(commandArgs);
                return;
            case "x":
                InvokeCutFile(commandArgs);
                return;
            case "v":
                InvokePasteClipboard(commandArgs);
                return;
        }
    }

    private void AltModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        return;
    }

    private void CtrlAltModifiedKeymap(TreeViewCommandArgs commandArgs)
    {
        return;
    }

    private Task InvokeCopyFile(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
            return Task.CompletedTask;

        var copyFileMenuOption = _ideService.CopyFile(
            treeViewNamespacePath.Item,
            () =>
            {
                NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewNamespacePath.Item.Name}", _ideService.TextEditorService.CommonService, TimeSpan.FromSeconds(7));
                return Task.CompletedTask;
            });

        if (copyFileMenuOption.OnClickFunc is null)
            return Task.CompletedTask;

        return copyFileMenuOption.OnClickFunc.Invoke();
    }

    private Task InvokePasteClipboard(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
            return Task.CompletedTask;

        MenuOptionRecord pasteMenuOptionRecord;

        if (treeViewNamespacePath.Item.IsDirectory)
        {
            pasteMenuOptionRecord = _ideService.PasteClipboard(
                treeViewNamespacePath.Item,
                async () =>
                {
                    var localParentOfCutFile = SolutionExplorerContextMenu.ParentOfCutFile;
                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewNamespacePath).ConfigureAwait(false);
                });
        }
        else
        {
            var parentDirectory = treeViewNamespacePath.Item.CreateSubstringParentDirectory();
            var parentDirectoryAbsolutePath = _ideService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true,
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder(),
                AbsolutePathNameKind.NameWithExtension);

            pasteMenuOptionRecord = _ideService.PasteClipboard(
                parentDirectoryAbsolutePath,
                async () =>
                {
                    var localParentOfCutFile = SolutionExplorerContextMenu.ParentOfCutFile;
                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewNamespacePath).ConfigureAwait(false);
                });
        }

        if (pasteMenuOptionRecord.OnClickFunc is null)
            return Task.CompletedTask;

        return pasteMenuOptionRecord.OnClickFunc.Invoke();
    }

    private Task InvokeCutFile(TreeViewCommandArgs commandArgs)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
            return Task.CompletedTask;

        var parent = treeViewNamespacePath.Parent as TreeViewNamespacePath;

        MenuOptionRecord cutFileOptionRecord = _ideService.CutFile(
            treeViewNamespacePath.Item,
            () =>
            {
                NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewNamespacePath.Item.Name}", _ideService.TextEditorService.CommonService, TimeSpan.FromSeconds(7));
                SolutionExplorerContextMenu.ParentOfCutFile = parent;
                return Task.CompletedTask;
            });

        if (cutFileOptionRecord.OnClickFunc is null)
            return Task.CompletedTask;

        return cutFileOptionRecord.OnClickFunc.Invoke();
    }

    private Task InvokeOpenInEditor(TreeViewCommandArgs commandArgs, bool shouldSetFocusToEditor)
    {
        if (commandArgs.TreeViewContainer.ActiveNode is not TreeViewNamespacePath treeViewNamespacePath)
            return Task.CompletedTask;
            
        _ideService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await _ideService.TextEditorService.OpenInEditorAsync(
                editContext,
                treeViewNamespacePath.Item.Value,
                shouldSetFocusToEditor,
                null,
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

        _ideService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(
            DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            treeViewModel);

        _ideService.TextEditorService.CommonService.TreeView_MoveUpAction(
            DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            false,
            false);
    }
}
