using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class SolutionExplorerDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	private SolutionExplorerTreeViewKeyboardEventHandler _solutionExplorerTreeViewKeymap = null!;
	private SolutionExplorerTreeViewMouseEventHandler _solutionExplorerTreeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		IdeService.TextEditorService.CommonService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	
		_solutionExplorerTreeViewKeymap = new SolutionExplorerTreeViewKeyboardEventHandler(
			IdeService);

		_solutionExplorerTreeViewMouseEventHandler = new SolutionExplorerTreeViewMouseEventHandler(
			IdeService);
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

		IdeService.TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
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

		IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
	}
	
	public async void OnDotNetSolutionStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}