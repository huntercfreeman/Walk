using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Extensions.DotNet.Outputs.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays.Internals;

public partial class OutputDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    
    private readonly Throttle _eventThrottle = new Throttle(TimeSpan.FromMilliseconds(333));
    
    private TreeViewContainerParameter _treeViewContainerParameter;

    protected override void OnInitialized()
    {
        _treeViewContainerParameter = new(
            OutputState.TreeViewContainerKey,
            new OutputTreeViewKeyboardEventHandler(DotNetService.TextEditorService),
            new OutputTreeViewMouseEventHandler(DotNetService.TextEditorService),
            OnTreeViewContextMenuFunc);
    
        DotNetService.DotNetStateChanged += DotNetCliOutputParser_StateChanged;
        
        if (DotNetService.GetOutputState().DotNetRunParseResultId != DotNetService.GetDotNetRunParseResult().Id)
            DotNetCliOutputParser_StateChanged(DotNetStateChangedKind.CliOutputParserStateChanged);
    }
    
    public async void DotNetCliOutputParser_StateChanged(DotNetStateChangedKind dotNetStateChangedKind)
    {
        if (dotNetStateChangedKind == DotNetStateChangedKind.OutputStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
        else if (dotNetStateChangedKind == DotNetStateChangedKind.CliOutputParserStateChanged)
        {
            _eventThrottle.Run((Func<CancellationToken, Task>)(_ =>
            {
                if (DotNetService.GetOutputState().DotNetRunParseResultId == DotNetService.GetDotNetRunParseResult().Id)
                    return Task.CompletedTask;
    
                DotNetService.TextEditorService.CommonService.Continuous_Enqueue((IBackgroundTaskGroup)new BackgroundTask(
                    Key<IBackgroundTaskGroup>.Empty,
                    DotNetService.OutputService_Do_ConstructTreeView));
                return Task.CompletedTask;
            }));
        }
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
        DotNetService.DotNetStateChanged -= DotNetCliOutputParser_StateChanged;
    }
}
