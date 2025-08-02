using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class FindAllTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly TextEditorService _textEditorService;

    public FindAllTreeViewMouseEventHandler(TextEditorService textEditorService)
        : base(textEditorService.CommonService)
    {
        _textEditorService = textEditorService;
    }

    public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnDoubleClickAsync(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewFindAllTextSpan treeViewFindAllTextSpan)
            return Task.CompletedTask;

        _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await _textEditorService.OpenInEditorAsync(
                editContext,
                treeViewFindAllTextSpan.AbsolutePath.Value,
                true,
                treeViewFindAllTextSpan.Item.TextSpan.StartInclusiveIndex,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
        return Task.CompletedTask;
    }
}
