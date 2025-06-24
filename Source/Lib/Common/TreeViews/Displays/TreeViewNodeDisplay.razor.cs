using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.TreeViews.Displays;

public partial class TreeViewNodeDisplay : ComponentBase
{
    [CascadingParameter]
    public TreeViewCascadingValueBatch RenderBatch { get; set; } = null!;

    [Parameter, EditorRequired]
    public TreeViewNoType TreeViewNoType { get; set; } = null!;
    [Parameter, EditorRequired]
    public int Depth { get; set; }

    private ElementReference? _treeViewTitleElementReference;
    private Key<TreeViewChanged> _previousTreeViewChangedKey = Key<TreeViewChanged>.Empty;
    private bool _previousIsActive;

    private int OffsetInPixels => RenderBatch.OffsetPerDepthInPixels * Depth;

    private bool IsSelected => RenderBatch.TreeViewContainer.SelectedNodeList.Any(x => x.Key == TreeViewNoType.Key);

    private bool IsActive => RenderBatch.TreeViewContainer.ActiveNode is not null &&
                             RenderBatch.TreeViewContainer.ActiveNode.Key == TreeViewNoType.Key;
                             
    private string IsActiveId => IsActive ? RenderBatch.TreeViewContainer.ActiveNodeElementId : string.Empty;

    private string IsSelectedCssClass => IsSelected ? "di_selected" : string.Empty;
    private string IsActiveCssClass => IsActive ? "di_active" : string.Empty;

    protected override bool ShouldRender()
    {
        if (_previousTreeViewChangedKey != TreeViewNoType.TreeViewChangedKey)
        {
            _previousTreeViewChangedKey = TreeViewNoType.TreeViewChangedKey;
            return true;
        }

        return false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var localIsActive = IsActive;

        if (_previousIsActive != localIsActive)
        {
            _previousIsActive = localIsActive;

            if (localIsActive)
                await FocusAsync().ConfigureAwait(false);
        }
    }

    private async Task FocusAsync()
    {
        try
        {
            var localTreeViewTitleElementReference = _treeViewTitleElementReference;

            if (localTreeViewTitleElementReference is not null)
            {
                await localTreeViewTitleElementReference.Value
                    .FocusAsync()
                    .ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
            //             This bug is seemingly happening randomly. I have a suspicion
            //             that there are race-condition exceptions occurring with "FocusAsync"
            //             on an ElementReference.
        }
    }

    private void HandleExpansionChevronOnMouseDown(TreeViewNoType localTreeViewNoType)
    {
        if (!localTreeViewNoType.IsExpandable)
            return;

        localTreeViewNoType.IsExpanded = !localTreeViewNoType.IsExpanded;

        if (localTreeViewNoType.IsExpanded)
        {
            RenderBatch.CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
            {
    			WorkKind = CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown,
            	TreeViewNoType = localTreeViewNoType,
            	TreeViewContainer = RenderBatch.TreeViewContainer
            });
        }
        else
        {
            RenderBatch.TreeViewService.ReduceReRenderNodeAction(RenderBatch.TreeViewContainer.Key, localTreeViewNoType);
        }
    }

    private async Task ManuallyPropagateOnContextMenu(
        MouseEventArgs mouseEventArgs,
        TreeViewContainer treeViewContainer,
        TreeViewNoType treeViewNoType)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            RenderBatch.TreeViewService,
            RenderBatch.TreeViewContainer,
            TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await RenderBatch.TreeViewMouseEventHandler
            .OnMouseDownAsync(treeViewCommandArgs)
            .ConfigureAwait(false);

        RenderBatch.CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
        {
    		WorkKind = CommonWorkKind.TreeView_ManuallyPropagateOnContextMenu,
        	HandleTreeViewOnContextMenu = RenderBatch.HandleTreeViewOnContextMenu,
            MouseEventArgs = mouseEventArgs,
            ContainerKey = treeViewContainer.Key,
            TreeViewNoType = treeViewNoType,
        });
    }

    private async Task HandleOnClick(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            RenderBatch.TreeViewService,
            RenderBatch.TreeViewContainer,
            TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await RenderBatch.TreeViewMouseEventHandler
            .OnClickAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private async Task HandleOnDoubleClick(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            RenderBatch.TreeViewService,
            RenderBatch.TreeViewContainer,
            TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await RenderBatch.TreeViewMouseEventHandler
            .OnDoubleClickAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private async Task HandleOnMouseDown(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            RenderBatch.TreeViewService,
            RenderBatch.TreeViewContainer,
            TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await RenderBatch.TreeViewMouseEventHandler
            .OnMouseDownAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
    	switch (keyboardEventArgs.Key)
    	{
    		case "ContextMenu":
    		{
    			var mouseEventArgs = new MouseEventArgs { Button = -1 };
	            return ManuallyPropagateOnContextMenu(mouseEventArgs, RenderBatch.TreeViewContainer, TreeViewNoType);
    		}
    		case ".":
    		{
    			if (keyboardEventArgs.CtrlKey)
    			{
    				var mouseEventArgs = new MouseEventArgs { Button = -1 };
	            	return ManuallyPropagateOnContextMenu(mouseEventArgs, RenderBatch.TreeViewContainer, TreeViewNoType);
    			}
    			break;
    		}
    	}
    
        return Task.CompletedTask;
    }

    private string GetShowDefaultCursorCssClass(bool isExpandable)
    {
        return isExpandable
            ? string.Empty
            : "di_tree-view-use-default-cursor";
    }
}