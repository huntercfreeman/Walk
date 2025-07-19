using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class SolutionExplorerDisplay : ComponentBase, IDisposable
{
	[Inject]
	private DotNetService DotNetService { get; set; } = null!;

	private SolutionExplorerTreeViewKeyboardEventHandler _solutionExplorerTreeViewKeymap = null!;
	private SolutionExplorerTreeViewMouseEventHandler _solutionExplorerTreeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		DotNetService.IdeService.TextEditorService.CommonService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	protected override void OnInitialized()
	{
		DotNetService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	
		_solutionExplorerTreeViewKeymap = new SolutionExplorerTreeViewKeyboardEventHandler(
			DotNetService.IdeService);

		_solutionExplorerTreeViewMouseEventHandler = new SolutionExplorerTreeViewMouseEventHandler(
			DotNetService.IdeService);
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
	
	public async void OnDotNetSolutionStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}