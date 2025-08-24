using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileContextMenu : ComponentBase
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    [Parameter, EditorRequired]
    public TreeViewCommandArgs TreeViewCommandArgs { get; set; }

    public static readonly Key<DropdownRecord> ContextMenuKey = Key<DropdownRecord>.NewKey();

    private (TreeViewCommandArgs treeViewCommandArgs, MenuRecord menuRecord) _previousGetMenuRecordInvocation;

    private MenuRecord GetMenuRecord(TreeViewCommandArgs commandArgs)
    {
        if (_previousGetMenuRecordInvocation.treeViewCommandArgs == commandArgs)
            return _previousGetMenuRecordInvocation.menuRecord;

        if (commandArgs.NodeThatReceivedMouseEvent is null)
        {
            var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }

        var menuRecordsList = new List<MenuOptionRecord>();

        var treeViewModel = commandArgs.NodeThatReceivedMouseEvent;
        var parentTreeViewModel = treeViewModel.Parent;

        var parentTreeViewAbsolutePath = parentTreeViewModel as TreeViewAbsolutePath;

        if (treeViewModel is not TreeViewAbsolutePath treeViewAbsolutePath)
        {
            var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }

        if (treeViewAbsolutePath.Item.IsDirectory)
        {
            menuRecordsList.AddRange(GetFileMenuOptions(treeViewAbsolutePath, parentTreeViewAbsolutePath)
                .Union(GetDirectoryMenuOptions(treeViewAbsolutePath))
                .Union(GetDebugMenuOptions(treeViewAbsolutePath)));
        }
        else
        {
            menuRecordsList.AddRange(GetFileMenuOptions(treeViewAbsolutePath, parentTreeViewAbsolutePath)
                .Union(GetDebugMenuOptions(treeViewAbsolutePath)));
        }

        // Default case
        {
            if (menuRecordsList.Count == 0)                menuRecordsList = MenuRecord.NoMenuOptionsExistList.ToList();

            var menuRecord = new MenuRecord(menuRecordsList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }
    }

    private MenuOptionRecord[] GetDirectoryMenuOptions(TreeViewAbsolutePath treeViewModel)
    {
        return new[]
        {
            IdeService.NewEmptyFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            IdeService.NewDirectory(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            /*IdeService.PasteClipboard(
                treeViewModel.Item,
                async () =>
                {
                    var localParentOfCutFile = ParentOfCutFile;
                    ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false);
                }),*/
        };
    }

    private MenuOptionRecord[] GetFileMenuOptions(
        TreeViewAbsolutePath treeViewModel,
        TreeViewAbsolutePath? parentTreeViewModel)
    {
        return Array.Empty<MenuOptionRecord>();
        /*return new[]
        {
            IdeService.CopyFile(
                treeViewModel.Item,
                (Func<Task>)(() => {
                    NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewModel.Item.NameWithExtension}", IdeService.CommonService, TimeSpan.FromSeconds(7));
                    return Task.CompletedTask;
                })),
            IdeService.CutFile(
                treeViewModel.Item,
                (Func<Task>)(() => {
                    NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewModel.Item.NameWithExtension}", IdeService.CommonService, TimeSpan.FromSeconds(7));
                    ParentOfCutFile = parentTreeViewModel;
                    return Task.CompletedTask;
                })),
            IdeService.DeleteFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false)),
            IdeService.RenameFile(
                treeViewModel.Item,
                IdeService.CommonService,
                async ()  => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false)),
        };*/
    }

    private MenuOptionRecord[] GetDebugMenuOptions(TreeViewAbsolutePath treeViewModel)
    {
        return new MenuOptionRecord[]
        {
            // new MenuOptionRecord(
            //     $"namespace: {treeViewModel.Item.Namespace}",
            //     MenuOptionKind.Read)
        };
    }

    /// <summary>
    /// This method I believe is causing bugs
    /// <br/><br/>
    /// For example, when removing a C# Project the
    /// solution is reloaded and a new root is made.
    /// <br/><br/>
    /// Then there is a timing issue where the new root is made and set
    /// as the root. But this method erroneously reloads the old root.
    /// </summary>
    /// <param name="treeViewModel"></param>
    private async Task ReloadTreeViewModel(TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildListAsync().ConfigureAwait(false);

        IdeService.CommonService.TreeView_ReRenderNodeAction(InputFileDisplay.InputFileSidebar_TreeViewContainerKey, treeViewModel);
        
        IdeService.CommonService.TreeView_MoveUpAction(
            InputFileDisplay.InputFileSidebar_TreeViewContainerKey,
            false,
            false);
    }
}
