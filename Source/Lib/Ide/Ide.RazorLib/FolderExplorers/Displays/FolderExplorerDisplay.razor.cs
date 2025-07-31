using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Displays;

public partial class FolderExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private TreeViewContainerParameter _treeViewContainerParameter;

    protected override void OnInitialized()
    {
        IdeService.IdeStateChanged += OnFolderExplorerStateChanged;
    
        _treeViewContainerParameter = new(
            FolderExplorerState.TreeViewContentStateKey,
            new FolderExplorerTreeViewKeyboardEventHandler(IdeService),
            new FolderExplorerTreeViewMouseEventHandler(IdeService),
            OnTreeViewContextMenuFunc);
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
