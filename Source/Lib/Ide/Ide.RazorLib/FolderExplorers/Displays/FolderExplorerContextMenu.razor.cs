using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Displays;

public partial class FolderExplorerContextMenu : ComponentBase
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    [Parameter, EditorRequired]
    public TreeViewCommandArgs TreeViewCommandArgs { get; set; }

    public static readonly Key<DropdownRecord> ContextMenuEventDropdownKey = Key<DropdownRecord>.NewKey();

    /// <summary>
    /// The program is currently running using Photino locally on the user's computer
    /// therefore this static solution works without leaking any information.
    /// </summary>
    public static TreeViewNoType? ParentOfCutFile;

	private (TreeViewCommandArgs treeViewCommandArgs, MenuRecord menuRecord) _previousGetMenuRecordInvocation;

    private MenuRecord GetMenuRecord(TreeViewCommandArgs treeViewCommandArgs)
    {
		if (_previousGetMenuRecordInvocation.treeViewCommandArgs == treeViewCommandArgs)
			return _previousGetMenuRecordInvocation.menuRecord;

        if (treeViewCommandArgs.NodeThatReceivedMouseEvent is null)
        {
			var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
			_previousGetMenuRecordInvocation = (treeViewCommandArgs, menuRecord);
			return menuRecord;
		}

        var menuRecordsList = new List<MenuOptionRecord>();

        var treeViewModel = treeViewCommandArgs.NodeThatReceivedMouseEvent;
        var parentTreeViewModel = treeViewModel.Parent;

        var parentTreeViewAbsolutePath = parentTreeViewModel as TreeViewAbsolutePath;

        if (treeViewModel is not TreeViewAbsolutePath treeViewAbsolutePath)
		{
			var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
			_previousGetMenuRecordInvocation = (treeViewCommandArgs, menuRecord);
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
			var menuRecord = new MenuRecord(menuRecordsList);
			_previousGetMenuRecordInvocation = (treeViewCommandArgs, menuRecord);
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
            IdeService.PasteClipboard(
                treeViewModel.Item,
                async () => 
                {
                    var localParentOfCutFile = ParentOfCutFile;
                    ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false);
                }),
        };
    }

    private MenuOptionRecord[] GetFileMenuOptions(
        TreeViewAbsolutePath treeViewModel,
        TreeViewAbsolutePath? parentTreeViewModel)
    {
        return new[]
        {
			IdeService.CopyFile(
                treeViewModel.Item,
                (Func<Task>)(() => {
					NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewModel.Item.NameWithExtension}", (Common.RazorLib.Options.Models.CommonUtilityService)IdeService.CommonUtilityService, TimeSpan.FromSeconds(7));
                    return Task.CompletedTask;
                })),
			IdeService.CutFile(
                treeViewModel.Item,
                (Func<Task>)(() => {
					NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewModel.Item.NameWithExtension}", (Common.RazorLib.Options.Models.CommonUtilityService)IdeService.CommonUtilityService, TimeSpan.FromSeconds(7));
					ParentOfCutFile = parentTreeViewModel;
                    return Task.CompletedTask;
                })),
			IdeService.DeleteFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false)),
			IdeService.RenameFile(
                treeViewModel.Item,
                (Common.RazorLib.Options.Models.CommonUtilityService)IdeService.CommonUtilityService,
                async ()  => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false))
        };
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

        IdeService.CommonUtilityService.TreeView_ReRenderNodeAction(
            FolderExplorerState.TreeViewContentStateKey,
            treeViewModel);

        IdeService.CommonUtilityService.TreeView_MoveUpAction(
            FolderExplorerState.TreeViewContentStateKey,
            false,
			false);
    }
}