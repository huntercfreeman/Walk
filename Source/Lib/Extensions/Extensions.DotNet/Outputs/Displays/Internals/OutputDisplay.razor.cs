using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.Outputs.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays.Internals;

public partial class OutputDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetCliOutputParser DotNetCliOutputParser { get; set; } = null!;
    [Inject]
    private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
	private WalkTextEditorConfig TextEditorConfig { get; set; } = null!;
	[Inject]
	private ICommonUiService CommonUiService { get; set; } = null!;
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    [Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;
	[Inject]
	private ICommonUtilityService CommonUtilityService { get; set; } = null!;
    
    private readonly Throttle _eventThrottle = new Throttle(TimeSpan.FromMilliseconds(333));
    
    private OutputTreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;
	private OutputTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));
    
    protected override void OnInitialized()
    {
    	_treeViewKeyboardEventHandler = new OutputTreeViewKeyboardEventHandler(
			TextEditorService,
			TextEditorConfig,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);

		_treeViewMouseEventHandler = new OutputTreeViewMouseEventHandler(
			TextEditorService,
			TextEditorConfig,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);
    
    	DotNetCliOutputParser.StateChanged += DotNetCliOutputParser_StateChanged;
    	DotNetBackgroundTaskApi.OutputService.OutputStateChanged += OnOutputStateChanged;
    	
    	if (DotNetBackgroundTaskApi.OutputService.GetOutputState().DotNetRunParseResultId != DotNetCliOutputParser.GetDotNetRunParseResult().Id)
    		DotNetCliOutputParser_StateChanged();
    }
    
    public void DotNetCliOutputParser_StateChanged()
    {
    	_eventThrottle.Run(_ =>
    	{
    		if (DotNetBackgroundTaskApi.OutputService.GetOutputState().DotNetRunParseResultId == DotNetCliOutputParser.GetDotNetRunParseResult().Id)
    			return Task.CompletedTask;
    		
    		BackgroundTaskService.Continuous_EnqueueGroup(new BackgroundTask(
    			Key<IBackgroundTaskGroup>.Empty,
    			DotNetBackgroundTaskApi.OutputService.Do_ConstructTreeView));
    		return Task.CompletedTask;
    	});
    }
    
    public async void OnOutputStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
	{
		var dropdownRecord = new DropdownRecord(
			OutputContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(OutputContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(OutputContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			restoreFocusOnClose: null);

		CommonUiService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}
    
    public void Dispose()
    {
    	DotNetCliOutputParser.StateChanged -= DotNetCliOutputParser_StateChanged;
    	DotNetBackgroundTaskApi.OutputService.OutputStateChanged -= OnOutputStateChanged;
    }
}