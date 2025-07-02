using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.FolderExplorers.Displays;

public partial class FolderExplorerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IFolderExplorerService FolderExplorerService { get; set; } = null!;
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IMenuOptionsFactory MenuOptionsFactory { get; set; } = null!;
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;

    private FolderExplorerTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    private FolderExplorerTreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;

    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
        CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

    protected override void OnInitialized()
    {
        FolderExplorerService.FolderExplorerStateChanged += OnFolderExplorerStateChanged;
        CommonUtilityService.AppOptionsStateChanged += OnAppOptionsStateChanged;

        _treeViewMouseEventHandler = new FolderExplorerTreeViewMouseEventHandler(
            IdeBackgroundTaskApi,
            TextEditorService,
            CommonUtilityService);

        _treeViewKeyboardEventHandler = new FolderExplorerTreeViewKeyboardEventHandler(
            IdeBackgroundTaskApi,
            TextEditorService,
            MenuOptionsFactory,
            CommonUtilityService);
    }

    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
		var dropdownRecord = new DropdownRecord(
			FolderExplorerContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(FolderExplorerContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(FolderExplorerContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			restoreFocusOnClose: null);

        CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}
	
	private async void OnFolderExplorerStateChanged() 
	{
		await InvokeAsync(StateHasChanged);
	}
	
	private async void OnAppOptionsStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}

    public void Dispose()
    {
        FolderExplorerService.FolderExplorerStateChanged -= OnFolderExplorerStateChanged;
        CommonUtilityService.AppOptionsStateChanged -= OnAppOptionsStateChanged;
    }
}