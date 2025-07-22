using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Displays;

public partial class FolderExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private FolderExplorerTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    private FolderExplorerTreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;

    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
        IdeService.CommonService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

    protected override void OnInitialized()
    {
        IdeService.IdeStateChanged += OnFolderExplorerStateChanged;

        _treeViewMouseEventHandler = new FolderExplorerTreeViewMouseEventHandler(IdeService);
        _treeViewKeyboardEventHandler = new FolderExplorerTreeViewKeyboardEventHandler(IdeService);
    }

    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
        var dropdownRecord = new DropdownRecord(
            FolderExplorerContextMenu.ContextMenuEventDropdownKey,
            treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
            treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
            typeof(FolderExplorerContextMenu),
            new Dictionary<string, object?>
            {
                {
                    nameof(FolderExplorerContextMenu.TreeViewCommandArgs),
                    treeViewCommandArgs
                }
            },
            restoreFocusOnClose: null);

        IdeService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }
    
    private async void OnFolderExplorerStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.FolderExplorerStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        IdeService.IdeStateChanged -= OnFolderExplorerStateChanged;
    }
}
