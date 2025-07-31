using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class SolutionExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private TreeViewContainerParameter _treeViewContainerParameter;

    protected override void OnInitialized()
    {
        DotNetService.DotNetStateChanged += OnDotNetSolutionStateChanged;
    
        _treeViewContainerParameter = new(
            DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            new SolutionExplorerTreeViewKeyboardEventHandler(DotNetService.IdeService),
            new SolutionExplorerTreeViewMouseEventHandler(DotNetService.IdeService),
            OnTreeViewContextMenuFunc);
    }

    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
        var dropdownRecord = new DropdownRecord(
            SolutionExplorerContextMenu.ContextMenuEventDropdownKey,
            treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
            treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
            typeof(SolutionExplorerContextMenu),
            new Dictionary<string, object?>
            {
                {
                    nameof(SolutionExplorerContextMenu.TreeViewCommandArgs),
                    treeViewCommandArgs
                }
            },
            null);

        DotNetService.IdeService.TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }

    private void OpenNewDotNetSolutionDialog()
    {
        var dialogRecord = new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "New .NET Solution",
            typeof(DotNetSolutionFormDisplay),
            null,
            null,
            true,
            null);

        DotNetService.IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
    }
    
    public async void OnDotNetSolutionStateChanged(DotNetStateChangedKind dotNetStateChangedKind)
    {
        if (dotNetStateChangedKind == DotNetStateChangedKind.SolutionStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        DotNetService.DotNetStateChanged -= OnDotNetSolutionStateChanged;
    }
}
