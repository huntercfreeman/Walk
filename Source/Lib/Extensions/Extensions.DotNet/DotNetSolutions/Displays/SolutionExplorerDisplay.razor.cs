using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class SolutionExplorerDisplay : ComponentBase, IDisposable
{
	[Inject]
	private ITreeViewService TreeViewService { get; set; } = null!;
	[Inject]
	private IDialogService DialogService { get; set; } = null!;
	[Inject]
	private INotificationService NotificationService { get; set; } = null!;
	[Inject]
	private IDropdownService DropdownService { get; set; } = null!;
	[Inject]
	private IAppOptionsService AppOptionsService { get; set; } = null!;
	[Inject]
	private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
	[Inject]
	private IMenuOptionsFactory MenuOptionsFactory { get; set; } = null!;
	[Inject]
	private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
	private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;

	private SolutionExplorerTreeViewKeyboardEventHandler _solutionExplorerTreeViewKeymap = null!;
	private SolutionExplorerTreeViewMouseEventHandler _solutionExplorerTreeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	
		_solutionExplorerTreeViewKeymap = new SolutionExplorerTreeViewKeyboardEventHandler(
			IdeBackgroundTaskApi,
			MenuOptionsFactory,
			CommonComponentRenderers,
			TextEditorService,
			TreeViewService,
			NotificationService,
			BackgroundTaskService,
			EnvironmentProvider);

		_solutionExplorerTreeViewMouseEventHandler = new SolutionExplorerTreeViewMouseEventHandler(
			IdeBackgroundTaskApi,
			TextEditorService,
			TreeViewService,
			BackgroundTaskService);
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

		DropdownService.ReduceRegisterAction(dropdownRecord);
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

		DialogService.ReduceRegisterAction(dialogRecord);
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