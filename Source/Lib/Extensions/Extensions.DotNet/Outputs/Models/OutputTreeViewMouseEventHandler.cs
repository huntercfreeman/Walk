using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.DotNet.Outputs.Models;

public class OutputTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly TextEditorService _textEditorService;

    public OutputTreeViewMouseEventHandler(TextEditorService textEditorService)
        : base(textEditorService.CommonService)
    {
        _textEditorService = textEditorService;
    }

    public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnDoubleClickAsync(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewDiagnosticLine treeViewDiagnosticLine)
            return Task.CompletedTask;
            
        return OutputTextSpanHelper.OpenInEditorOnClick(
            treeViewDiagnosticLine,
            true,
            _textEditorService);
    }
}
