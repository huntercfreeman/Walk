using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Extensions.DotNet.Outputs.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays.Internals;

public partial class OutputDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    [Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;
    
    private readonly Throttle _eventThrottle = new Throttle(TimeSpan.FromMilliseconds(333));
    
    private OutputTreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;
	private OutputTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		DotNetService.TextEditorService.CommonService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));
    
    protected override void OnInitialized()
    {
    	_treeViewKeyboardEventHandler = new OutputTreeViewKeyboardEventHandler(
			DotNetService.TextEditorService,
			ServiceProvider);

		_treeViewMouseEventHandler = new OutputTreeViewMouseEventHandler(
			DotNetService.TextEditorService,
			ServiceProvider);
    
    	DotNetService.StateChanged += DotNetCliOutputParser_StateChanged;
    	DotNetService.OutputStateChanged += OnOutputStateChanged;
    	
    	if (DotNetService.GetOutputState().DotNetRunParseResultId != DotNetService.GetDotNetRunParseResult().Id)
    		DotNetCliOutputParser_StateChanged();
    }
    
    public void DotNetCliOutputParser_StateChanged()
    {
		_eventThrottle.Run((Func<CancellationToken, Task>)(_ =>
    	{
    		if (DotNetService.GetOutputState().DotNetRunParseResultId == DotNetService.GetDotNetRunParseResult().Id)
    			return Task.CompletedTask;

			DotNetService.TextEditorService.CommonService.Continuous_EnqueueGroup((IBackgroundTaskGroup)new BackgroundTask(
				Key<IBackgroundTaskGroup>.Empty,
				DotNetService.OutputService_Do_ConstructTreeView));
    		return Task.CompletedTask;
    	}));
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

		DotNetService.TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}
    
    public void Dispose()
    {
    	DotNetService.StateChanged -= DotNetCliOutputParser_StateChanged;
    	DotNetService.OutputStateChanged -= OnOutputStateChanged;
    }
}