using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Models;

public class FolderExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;

    public FolderExplorerTreeViewMouseEventHandler(
            IdeBackgroundTaskApi ideBackgroundTaskApi)
        : base(ideBackgroundTaskApi.CommonUtilityService)
    {
        _ideBackgroundTaskApi = ideBackgroundTaskApi;
    }

    public override async Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        await base.OnDoubleClickAsync(commandArgs).ConfigureAwait(false);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewAbsolutePath treeViewAbsolutePath)
            return;

		_ideBackgroundTaskApi.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			await _ideBackgroundTaskApi.TextEditorService.OpenInEditorAsync(
				editContext,
				treeViewAbsolutePath.Item.Value,
				true,
				cursorPositionIndex: null,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
		});
    }
}