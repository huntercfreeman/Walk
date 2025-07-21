using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Models;

public class FolderExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly IdeService _ideService;

    public FolderExplorerTreeViewMouseEventHandler(
            IdeService ideService)
        : base(ideService.CommonService)
    {
        _ideService = ideService;
    }

    public override async Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        await base.OnDoubleClickAsync(commandArgs).ConfigureAwait(false);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewAbsolutePath treeViewAbsolutePath)
            return;

        _ideService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await _ideService.TextEditorService.OpenInEditorAsync(
                editContext,
                treeViewAbsolutePath.Item.Value,
                true,
                cursorPositionIndex: null,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
    }
}
