using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public class CompilerServiceExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
	private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;

	public CompilerServiceExplorerTreeViewMouseEventHandler(
			IdeBackgroundTaskApi ideBackgroundTaskApi,
			ITreeViewService treeViewService,
			BackgroundTaskService backgroundTaskService)
		: base(treeViewService, backgroundTaskService)
	{
		_ideBackgroundTaskApi = ideBackgroundTaskApi;
	}

	public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
	{
		return base.OnDoubleClickAsync(commandArgs);
	}
}