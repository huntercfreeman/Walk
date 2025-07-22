using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.TreeViews.Displays;

public partial class TreeViewNodeDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public TreeViewNodeParameter TreeViewNodeParameter { get; set; }

    private ElementReference? _treeViewTitleElementReference;
    private Key<TreeViewChanged> _previousTreeViewChangedKey = Key<TreeViewChanged>.Empty;
    private bool _previousIsActive;

    private int OffsetInPixels => TreeViewNodeParameter.RenderBatch.OffsetPerDepthInPixels * TreeViewNodeParameter.Depth;

    private bool IsSelected => TreeViewNodeParameter.RenderBatch.TreeViewContainer.SelectedNodeList.Any(x => x.Key == TreeViewNodeParameter.TreeViewNoType.Key);

    private bool IsActive => TreeViewNodeParameter.RenderBatch.TreeViewContainer.ActiveNode is not null &&
                             TreeViewNodeParameter.RenderBatch.TreeViewContainer.ActiveNode.Key == TreeViewNodeParameter.TreeViewNoType.Key;
                             
    private string IsActiveId => IsActive ? TreeViewNodeParameter.RenderBatch.TreeViewContainer.ActiveNodeElementId : string.Empty;

    private string IsSelectedCssClass => IsSelected ? "di_selected" : string.Empty;
    private string IsActiveCssClass => IsActive ? "di_active" : string.Empty;

    protected override bool ShouldRender()
    {
        if (_previousTreeViewChangedKey != TreeViewNodeParameter.TreeViewNoType.TreeViewChangedKey)
        {
            _previousTreeViewChangedKey = TreeViewNodeParameter.TreeViewNoType.TreeViewChangedKey;
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
            TreeViewNodeParameter.RenderBatch.CommonService.Enqueue(new CommonWorkArgs
            {
                WorkKind = CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown,
                TreeViewNoType = localTreeViewNoType,
                TreeViewContainer = TreeViewNodeParameter.RenderBatch.TreeViewContainer
            });
        }
        else
        {
            TreeViewNodeParameter.RenderBatch.CommonService.TreeView_ReRenderNodeAction(TreeViewNodeParameter.RenderBatch.TreeViewContainer.Key, localTreeViewNoType);
        }
    }

    private async Task ManuallyPropagateOnContextMenu(
        MouseEventArgs mouseEventArgs,
        TreeViewContainer treeViewContainer,
        TreeViewNoType treeViewNoType)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            TreeViewNodeParameter.RenderBatch.CommonService,
            TreeViewNodeParameter.RenderBatch.TreeViewContainer,
            TreeViewNodeParameter.TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await TreeViewNodeParameter.RenderBatch.TreeViewMouseEventHandler
            .OnMouseDownAsync(treeViewCommandArgs)
            .ConfigureAwait(false);

        TreeViewNodeParameter.RenderBatch.CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.TreeView_ManuallyPropagateOnContextMenu,
            HandleTreeViewOnContextMenu = TreeViewNodeParameter.RenderBatch.HandleTreeViewOnContextMenu,
            MouseEventArgs = mouseEventArgs,
            ContainerKey = treeViewContainer.Key,
            TreeViewNoType = treeViewNoType,
        });
    }

    private async Task HandleOnClick(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            TreeViewNodeParameter.RenderBatch.CommonService,
            TreeViewNodeParameter.RenderBatch.TreeViewContainer,
            TreeViewNodeParameter.TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await TreeViewNodeParameter.RenderBatch.TreeViewMouseEventHandler
            .OnClickAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private async Task HandleOnDoubleClick(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            TreeViewNodeParameter.RenderBatch.CommonService,
            TreeViewNodeParameter.RenderBatch.TreeViewContainer,
            TreeViewNodeParameter.TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await TreeViewNodeParameter.RenderBatch.TreeViewMouseEventHandler
            .OnDoubleClickAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private async Task HandleOnMouseDown(MouseEventArgs? mouseEventArgs)
    {
        var treeViewCommandArgs = new TreeViewCommandArgs(
            TreeViewNodeParameter.RenderBatch.CommonService,
            TreeViewNodeParameter.RenderBatch.TreeViewContainer,
            TreeViewNodeParameter.TreeViewNoType,
            FocusAsync,
            null,
            mouseEventArgs,
            null);

        await TreeViewNodeParameter.RenderBatch.TreeViewMouseEventHandler
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
                return ManuallyPropagateOnContextMenu(mouseEventArgs, TreeViewNodeParameter.RenderBatch.TreeViewContainer, TreeViewNodeParameter.TreeViewNoType);
            }
            case ".":
            {
                if (keyboardEventArgs.CtrlKey)
                {
                    var mouseEventArgs = new MouseEventArgs { Button = -1 };
                    return ManuallyPropagateOnContextMenu(mouseEventArgs, TreeViewNodeParameter.RenderBatch.TreeViewContainer, TreeViewNodeParameter.TreeViewNoType);
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
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetNodeElementCssClass()
    {
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Clear();
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append("di_tree-view-title ");
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append(IsSelectedCssClass);
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append(" ");
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append(IsActiveCssClass);
        
        return TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.ToString();
    }
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetNodeChevronCssClass()
    {
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Clear();
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append("di_tree-view-expansion-chevron ");
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append(GetShowDefaultCursorCssClass(TreeViewNodeParameter.TreeViewNoType.IsExpandable));
        
        return TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.ToString();
    }
}
