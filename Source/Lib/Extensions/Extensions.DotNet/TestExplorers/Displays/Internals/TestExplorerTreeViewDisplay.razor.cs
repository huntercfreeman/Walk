using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

public partial class TestExplorerTreeViewDisplay : ComponentBase
{
	[Inject]
	private ITreeViewService TreeViewService { get; set; } = null!;
	[Inject]
	private ICommonUiService CommonUiService { get; set; } = null!;
	[Inject]
	private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
	private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = null!;
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;

	[CascadingParameter]
	public TestExplorerRenderBatchValidated RenderBatch { get; set; } = null!;

	[Parameter, EditorRequired]
	public ElementDimensions ElementDimensions { get; set; } = null!;

	private TestExplorerTreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;
	private TestExplorerTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		RenderBatch.AppOptionsState.Options.IconSizeInPixels * (2.0 / 3.0));

	protected override void OnInitialized()
	{
		_treeViewKeyboardEventHandler = new TestExplorerTreeViewKeyboardEventHandler(
			CommonComponentRenderers,
			CompilerServiceRegistry,
			TextEditorService,
			CommonUiService,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);

		_treeViewMouseEventHandler = new TestExplorerTreeViewMouseEventHandler(
			CommonComponentRenderers,
			CompilerServiceRegistry,
			TextEditorService,
			CommonUiService,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);
	}

	private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
	{
		var dropdownRecord = new DropdownRecord(
			TestExplorerContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(TestExplorerContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(TestExplorerContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			restoreFocusOnClose: null);

		CommonUiService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}
}