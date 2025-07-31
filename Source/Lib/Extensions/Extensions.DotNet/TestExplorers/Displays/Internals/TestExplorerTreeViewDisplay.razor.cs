using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.DotNet.TestExplorers.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

public partial class TestExplorerTreeViewDisplay : ComponentBase
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter]
    public TestExplorerRenderBatchValidated RenderBatch { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions ElementDimensions { get; set; } = null!;

    private TreeViewContainerParameter _treeViewContainerParameter;

    protected override void OnInitialized()
    {
        _treeViewContainerParameter = new(
            TestExplorerState.TreeViewTestExplorerKey,
            new TestExplorerTreeViewKeyboardEventHandler(TextEditorService),
            new TestExplorerTreeViewMouseEventHandler(TextEditorService),
            OnTreeViewContextMenuFunc);
    }

    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
        var dropdownRecord = new DropdownRecord(
            TestExplorerContextMenu.ContextMenuEventDropdownKey,
            treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
            treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
            typeof(TestExplorerContextMenu),
            new Dictionary<string, object?>
            {
                {
                    nameof(TestExplorerContextMenu.TreeViewCommandArgs),
                    treeViewCommandArgs
                }
            },
            restoreFocusOnClose: null);

        TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }
}
