using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.WatchWindows.Displays;

/// <summary>
/// TODO: SphagettiCode - TextEditor css classes are referenced in this
/// tree view? Furthermore, because of this, the Text Editor css classes are not being
/// set to the correct theme (try a non visual studio clone theme -- it doesn't work). (2023-09-19)
/// </summary>
public partial class WatchWindowDisplay : ComponentBase
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

    [Parameter, EditorRequired]
    public WatchWindowObject WatchWindowObject { get; set; } = null!;

    public static Key<TreeViewContainer> TreeViewContainerKey { get; } = Key<TreeViewContainer>.NewKey();
    public static Key<DropdownRecord> WatchWindowContextMenuDropdownKey { get; } = Key<DropdownRecord>.NewKey();

    private TreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    private TreeViewKeyboardEventHandler _treeViewKeyboardEventHandler = null!;

    protected override void OnInitialized()
    {
        _treeViewMouseEventHandler = new(CommonUtilityService);
        _treeViewKeyboardEventHandler = new(CommonUtilityService);
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!CommonUtilityService.TryGetTreeViewContainer(TreeViewContainerKey, out var treeViewContainer))
            {
                var rootNode = new TreeViewReflection(
                    WatchWindowObject,
                    true,
                    false,
                    CommonUtilityService.CommonComponentRenderers);

                CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                    TreeViewContainerKey,
                    rootNode,
                    new List<TreeViewNoType>() { rootNode }));
            }
        }
        
        return Task.CompletedTask;
    }

    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
		var dropdownRecord = new DropdownRecord(
			WatchWindowContextMenuDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(WatchWindowContextMenuDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(WatchWindowContextMenuDisplay.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			treeViewCommandArgs.RestoreFocusToTreeView);

        CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }
    
    public void Dispose()
	{
		CommonUtilityService.TreeView_DisposeContainerAction(TreeViewContainerKey);
	}
}