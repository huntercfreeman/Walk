using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib.FindAlls.Models;
using Walk.TextEditor.RazorLib.Installations.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Displays;

public partial class FindAllDisplay : ComponentBase, IDisposable
{
	[Inject]
    private IFindAllService FindAllService { get; set; } = null!;
    [Inject]
	private ICommonUtilityService CommonUtilityService { get; set; } = null!;
	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;	
	[Inject]
	private WalkTextEditorConfig TextEditorConfig { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
    
    private FindAllTreeViewKeyboardEventHandler _treeViewKeymap = null!;
	private FindAllTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    
    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	private string SearchQuery
    {
        get => FindAllService.GetFindAllState().SearchQuery;
        set
        {
            if (value is not null)
                FindAllService.SetSearchQuery(value);
        }
    }

	private string StartingDirectoryPath
    {
        get => FindAllService.GetFindAllState().StartingDirectoryPath;
        set
        {
            if (value is not null)
                FindAllService.SetStartingDirectoryPath(value);
        }
    }
    
    protected override void OnInitialized()
	{
		FindAllService.FindAllStateChanged += OnFindAllStateChanged;
	
		_treeViewKeymap = new FindAllTreeViewKeyboardEventHandler(
			TextEditorService,
			TextEditorConfig,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);

		_treeViewMouseEventHandler = new FindAllTreeViewMouseEventHandler(
			TextEditorService,
			TextEditorConfig,
			ServiceProvider,
			TreeViewService,
			BackgroundTaskService);
	}
	
	private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
	{
		var dropdownRecord = new DropdownRecord(
			FindAllContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(FindAllContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(FindAllContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			null);

		CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}

	private void DoSearchOnClick()
    {
    	FindAllService.HandleStartSearchAction();
    }

	private void CancelSearchOnClick()
    {
    	FindAllService.CancelSearch();
    }
    
    public async void OnFindAllStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	FindAllService.FindAllStateChanged -= OnFindAllStateChanged;
    }
}