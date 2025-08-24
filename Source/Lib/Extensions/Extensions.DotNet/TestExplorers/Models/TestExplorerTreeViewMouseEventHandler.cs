using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TestExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly TextEditorService _textEditorService;

    public TestExplorerTreeViewMouseEventHandler(TextEditorService textEditorService)
        : base(textEditorService.CommonService)
    {
        _textEditorService = textEditorService;
    }

    public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnDoubleClickAsync(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewStringFragment treeViewStringFragment)
        {
            CommonFacts.DispatchInformative(
                nameof(TestExplorerTreeViewMouseEventHandler),
                $"Could not open in editor because node is not type: {nameof(TreeViewStringFragment)}",
                _textEditorService.CommonService,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        if (treeViewStringFragment.Parent is not TreeViewStringFragment parentTreeViewStringFragment)
        {
            CommonFacts.DispatchInformative(
                nameof(TestExplorerTreeViewMouseEventHandler),
                $"Could not open in editor because node's parent does not seem to include a class name",
                _textEditorService.CommonService,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        var className = parentTreeViewStringFragment.Item.Value.Split('.').Last();

        CommonFacts.DispatchInformative(
            nameof(TestExplorerTreeViewMouseEventHandler),
            className + ".cs",
            _textEditorService.CommonService,
            TimeSpan.FromSeconds(5));

        var methodName = treeViewStringFragment.Item.Value.Trim();

        CommonFacts.DispatchInformative(
            nameof(TestExplorerTreeViewMouseEventHandler),
            methodName + "()",
            _textEditorService.CommonService,
            TimeSpan.FromSeconds(5));

        _textEditorService.WorkerArbitrary.PostUnique(
            TestExplorerHelper.ShowTestInEditorFactory(
                className,
                methodName,
                _textEditorService));

        return Task.CompletedTask;
    }
}
