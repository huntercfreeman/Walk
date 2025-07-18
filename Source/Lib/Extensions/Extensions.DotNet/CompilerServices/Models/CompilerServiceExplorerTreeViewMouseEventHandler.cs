using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public class CompilerServiceExplorerTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
	private readonly IdeService _ideService;

	public CompilerServiceExplorerTreeViewMouseEventHandler(
			IdeService ideService)
		: base(ideService.CommonUtilityService)
	{
		_ideService = ideService;
	}

	public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
	{
		return base.OnDoubleClickAsync(commandArgs);
	}
}