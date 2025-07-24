using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.TreeViews.Displays;

public partial class TreeViewContainerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TreeViewContainer> TreeViewContainerKey { get; set; } = Key<TreeViewContainer>.Empty;
    [Parameter, EditorRequired]
    public TreeViewMouseEventHandler TreeViewMouseEventHandler { get; set; } = null!;
    [Parameter, EditorRequired]
    public TreeViewKeyboardEventHandler TreeViewKeyboardEventHandler { get; set; } = null!;

    [Parameter]
    public string CssClassString { get; set; } = string.Empty;
    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;
    /// <summary>If a consumer of the TreeView component does not have logic for their own DropdownComponent, then one can provide a RenderFragment and a dropdown will be rendered for the consumer and their RenderFragment is rendered within that dropdown.<br/><br/>If one has their own DropdownComponent, then it is recommended that they use <see cref="OnContextMenuFunc"/> instead.</summary>
    [Parameter]
    public RenderFragment<TreeViewCommandArgs>? OnContextMenuRenderFragment { get; set; }
    /// <summary>If a consumer of the TreeView component does not have logic for their own DropdownComponent, then it is recommended to use <see cref="OnContextMenuRenderFragment"/><br/><br/> <see cref="OnContextMenuFunc"/> allows one to be notified of the ContextMenu event along with the necessary parameters by being given <see cref="TreeViewCommandArgs"/></summary>
    [Parameter]
    public Func<TreeViewCommandArgs, Task>? OnContextMenuFunc { get; set; }
    [Parameter]
    public int OffsetPerDepthInPixels { get; set; } = 12;
    [Parameter]
    public int WalkTreeViewIconWidth { get; set; } = 16;
    
    /// <summary>Pixels</summary>
    private int _lineHeight = 20;
    
    private Guid _guidId = Guid.NewGuid();
    private string _htmlId = null!;
    
    private int Index { get; set; }

    private TreeViewCommandArgs _treeViewContextMenuCommandArgs;
    private ElementReference? _treeViewStateDisplayElementReference;
    
    private TreeViewContainer _treeViewContainer;
    
    private TreeViewMeasurements _treeViewMeasurements;
    
    private DotNetObjectReference<TreeViewContainerDisplay>? _dotNetHelper;

    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
    
        _htmlId = $"luth_common_treeview-{_guidId}";
        
        CommonService.CommonUiStateChanged += OnTreeViewStateChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
            // before you finish setting up the events?
            // (is this a thing, I'm just presuming this would be true).
            _treeViewMeasurements = await JsRuntime.InvokeAsync<TreeViewMeasurements>(
                "walkCommon.treeViewInitialize",
                _dotNetHelper,
                _htmlId);
            
            Console.WriteLine(_treeViewMeasurements);
        }
    }
    
    [JSInvokable]
    public void ReceiveOnKeyDown(TreeViewEventArgsKeyDown eventArgsKeyDown)
    {
        Console.WriteLine(eventArgsKeyDown.Key);
    }
    
    [JSInvokable]
    public void ReceiveOnContextMenu(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        Console.WriteLine("ReceiveOnContextMenu");
        _treeViewMeasurements = new TreeViewMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
        
        if (OnContextMenuFunc is null)
            return;
        
        ContextMenuFixedPosition contextMenuFixedPosition;
        
        TreeViewNoType? contextMenuTarget;
        
        if (eventArgsMouseDown.Button == -1)
        {
            contextMenuTarget = _flatNodeList[Index];
            
            contextMenuFixedPosition = new ContextMenuFixedPosition(
                OccurredDueToMouseEvent: false,
                LeftPositionInPixels: eventArgsMouseDown.BoundingClientRectLeft,
                TopPositionInPixels: eventArgsMouseDown.BoundingClientRectTop + _lineHeight);
        }
        else if (eventArgsMouseDown.Button == 2)
        {
            var relativeY = eventArgsMouseDown.Y - _treeViewMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
            relativeY = Math.Max(0, relativeY);
            
            var indexLocal = (int)(relativeY / _lineHeight);
            
            Index = IndexBasicValidation(indexLocal);
            contextMenuTarget = _flatNodeList[Index];
            
            contextMenuFixedPosition = new ContextMenuFixedPosition(
                OccurredDueToMouseEvent: true,
                LeftPositionInPixels: eventArgsMouseDown.X,
                TopPositionInPixels: eventArgsMouseDown.Y);
        }
        else
        {
            return;
        }
        
        _treeViewContextMenuCommandArgs = new TreeViewCommandArgs(
            CommonService,
            _treeViewContainer,
            _flatNodeList[Index],
            async () =>
            {
                _treeViewMeasurements = await JsRuntime.InvokeAsync<TreeViewMeasurements>(
                    "walkCommon.focusAndMeasureTreeView",
                    _htmlId,
                    /*preventScroll:*/ false);
            },
            contextMenuFixedPosition,
            new MouseEventArgs
            {
                ClientX = eventArgsMouseDown.X,
                ClientY = eventArgsMouseDown.Y,
            },
            keyboardEventArgs: null);
    
        CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.TreeView_HandleTreeViewOnContextMenu,
            OnContextMenuFunc = OnContextMenuFunc,
            TreeViewContextMenuCommandArgs = _treeViewContextMenuCommandArgs,
        });
    }
    
    [JSInvokable]
    public void ReceiveContentOnMouseDown(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        _treeViewMeasurements = new TreeViewMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    
        var relativeY = eventArgsMouseDown.Y - _treeViewMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
        relativeY = Math.Max(0, relativeY);
        
        var indexLocal = (int)(relativeY / _lineHeight);
        
        Index = IndexBasicValidation(indexLocal);
        
        var relativeX = eventArgsMouseDown.X - _treeViewMeasurements.BoundingClientRectLeft + eventArgsMouseDown.ScrollLeft;
        relativeX = Math.Max(0, relativeX);
        
        if (Math.Abs(relativeX - _flatNodeList[Index].Depth * OffsetPerDepthInPixels) <= 5)
        {
            HandleChevronOnClick(eventArgsMouseDown);
        }
        
        CommonService.TreeView_SetActiveNodeAction(
            _treeViewContainer.Key,
            _flatNodeList[Index],
            addSelectedNodes: false,
            selectNodesBetweenCurrentAndNextActiveNode: false);
    }
    
    [JSInvokable]
    public async Task ReceiveOnDoubleClick(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        _treeViewMeasurements = new TreeViewMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    
        var relativeY = eventArgsMouseDown.Y - _treeViewMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
        relativeY = Math.Max(0, relativeY);
        
        var indexLocal = (int)(relativeY / _lineHeight);
        
        Index = IndexBasicValidation(indexLocal);
        
        await TreeViewMouseEventHandler.OnDoubleClickAsync(new TreeViewCommandArgs(
            CommonService,
            _treeViewContainer,
            _flatNodeList[Index],
            async () =>
            {
                _treeViewMeasurements = await JsRuntime.InvokeAsync<TreeViewMeasurements>(
                    "walkCommon.focusAndMeasureTreeView",
                    _htmlId,
                    /*preventScroll:*/ false);
            },
            contextMenuFixedPosition: null,
            new MouseEventArgs
            {
                ClientX = eventArgsMouseDown.X,
                ClientY = eventArgsMouseDown.Y,
            },
            keyboardEventArgs: null));
    }
    
    /// <summary>
    /// UI thread only.
    /// </summary>
    private readonly List<TreeViewNoType> _flatNodeList = new();
    /// <summary>
    /// Contains the "used to be" targetNode, and the index that it left off at.
    /// </summary>
    private readonly Stack<(TreeViewNoType Node, int Index)> _nodeRecursionStack = new();
    
    private List<TreeViewNoType> GetFlatNodes()
    {
        _flatNodeList.Clear();
        _nodeRecursionStack.Clear();
        
        int depth;
        
        // I'm only going to include 'IsHidden' with the root node for now.
        if (_treeViewContainer.RootNode.IsHidden)
        {
            depth = 0;
        }
        else
        {
            _flatNodeList.Add(_treeViewContainer.RootNode);
            depth = 1;
        }
        
        _treeViewContainer.RootNode.IsExpanded = true;
        
        if (!_treeViewContainer.RootNode.IsExpanded ||
            _treeViewContainer.RootNode.ChildList.Count == 0)
        {
            return _flatNodeList;
        }
        
        var targetNode = _treeViewContainer.RootNode;
        
        int index = 0;
    
        // TODO: Rewrite the comment below this.
        // Loop iterates 1 layer above in order to avoid the break case every child if the child is not expanded or ChildList.Count is == 0
        // Thus, the root case has to be handled entirely outside the loop.
        while (true)
        {
            if (index >= targetNode.ChildList.Count)
            {
                if (_nodeRecursionStack.Count > 0)
                {
                    var recursionEntry = _nodeRecursionStack.Pop();
                    depth--;
                    targetNode = recursionEntry.Node;
                    index = recursionEntry.Index;
                    continue;
                }
                else
                {
                    break;
                }
            }
        
            var childNode = targetNode.ChildList[index++];
            childNode.IsExpanded = true;
            childNode.LoadChildListAsync().Wait();
            _flatNodeList.Add(childNode);
        
            if (childNode.IsExpanded && childNode.ChildList.Count > 0)
            {
                _nodeRecursionStack.Push((targetNode, index));
                childNode.Depth = depth;
                depth++;
                targetNode = childNode;
                index = 0;
            }
        }
        
        return _flatNodeList;
    }
    
    private int IndexBasicValidation(int indexLocal)
    {
        if (indexLocal < 0)
            return 0;
        else if (indexLocal >= _flatNodeList.Count)
            return _flatNodeList.Count - 1;
        
        return indexLocal;
    }

    private int GetRootDepth(TreeViewNoType rootNode)
    {
        return rootNode is TreeViewAdhoc ? -1 : 0;
    }
    
    private void HandleChevronOnClick(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        Console.WriteLine("HandleChevronOnClick");
        /*if (!localTreeViewNoType.IsExpandable)
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
        }*/
    }

    private async Task HandleTreeViewOnKeyDownWithPreventScroll(
        KeyboardEventArgs keyboardEventArgs,
        TreeViewContainer? treeViewContainer)
    {
        if (treeViewContainer is null)
            return;

        var treeViewCommandArgs = new TreeViewCommandArgs(
            CommonService,
            treeViewContainer,
            null,
            async () =>
            {
                _treeViewContextMenuCommandArgs = default;
                await InvokeAsync(StateHasChanged);

                var localTreeViewStateDisplayElementReference = _treeViewStateDisplayElementReference;

                try
                {
                    if (localTreeViewStateDisplayElementReference.HasValue)
                    {
                        await localTreeViewStateDisplayElementReference.Value
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
            },
            null,
            null,
            keyboardEventArgs);

        await TreeViewKeyboardEventHandler
            .OnKeyDownAsync(treeViewCommandArgs)
            .ConfigureAwait(false);
    }

    private async Task HandleTreeViewOnContextMenu(
        MouseEventArgs? mouseEventArgs,
        Key<TreeViewContainer> treeViewContainerKey,
        TreeViewNoType? treeViewMouseWasOver)
    {
        if (treeViewContainerKey == Key<TreeViewContainer>.Empty || mouseEventArgs is null)
            return;

        var treeViewContainer = CommonService.GetTreeViewContainer(TreeViewContainerKey);
        // Validate that the treeViewContainer did not change out from under us
        if (treeViewContainer is null || treeViewContainer.Key != treeViewContainerKey)
            return;

        ContextMenuFixedPosition contextMenuFixedPosition;
        TreeViewNoType contextMenuTargetTreeViewNoType;

        if (mouseEventArgs.Button == -1) // -1 here means ContextMenu event was from keyboard
        {
            if (treeViewContainer.ActiveNode is null)
                return;

            // If dedicated context menu button or shift + F10 was pressed as opposed to
            // a mouse RightClick then use JavaScript to determine the ContextMenu position.
            contextMenuFixedPosition = await CommonService.JsRuntimeCommonApi
                .GetTreeViewContextMenuFixedPosition(treeViewContainer.ActiveNodeElementId)
                .ConfigureAwait(false);

            contextMenuTargetTreeViewNoType = treeViewContainer.ActiveNode;
        }
        else
        {
            // If a mouse RightClick caused the event then
            // use the MouseEventArgs to determine the ContextMenu position
            if (treeViewMouseWasOver is null)
            {
                // 'whitespace' of the TreeView was right clicked as opposed to
                // a TreeView node and the event should be ignored.
                return;
            }

            contextMenuFixedPosition = new ContextMenuFixedPosition(
                true,
                mouseEventArgs.ClientX,
                mouseEventArgs.ClientY);

            contextMenuTargetTreeViewNoType = treeViewMouseWasOver;
        }

        _treeViewContextMenuCommandArgs = new TreeViewCommandArgs(
            CommonService,
            treeViewContainer,
            contextMenuTargetTreeViewNoType,
            async () =>
            {
                _treeViewContextMenuCommandArgs = default;
                await InvokeAsync(StateHasChanged);

                var localTreeViewStateDisplayElementReference = _treeViewStateDisplayElementReference;

                try
                {
                    if (localTreeViewStateDisplayElementReference.HasValue)
                    {
                        await localTreeViewStateDisplayElementReference.Value
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
            },
            contextMenuFixedPosition,
            mouseEventArgs,
            null);

        if (OnContextMenuFunc is not null)
        {
            CommonService.Enqueue(new CommonWorkArgs
            {
                WorkKind = CommonWorkKind.TreeView_HandleTreeViewOnContextMenu,
                OnContextMenuFunc = OnContextMenuFunc,
                TreeViewContextMenuCommandArgs = _treeViewContextMenuCommandArgs,
            });
        }

        await InvokeAsync(StateHasChanged);
    }

    private string GetHasActiveNodeCssClass(TreeViewContainer? treeViewContainer)
    {
        if (treeViewContainer?.ActiveNode is null)
            return string.Empty;

        return "di_active";
    }

    private string GetContextMenuCssStyleString()
    {
        if (_treeViewContextMenuCommandArgs.CommonService is null || _treeViewContextMenuCommandArgs.ContextMenuFixedPosition is null)
        {
            // This should never happen.
            return "display: none;";
        }

        var left =
            $"left: {_treeViewContextMenuCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels.ToCssValue()}px;";

        var top =
            $"top: {_treeViewContextMenuCommandArgs.ContextMenuFixedPosition.TopPositionInPixels.ToCssValue()}px;";

        return $"{left} {top}";
    }
    
    public async void OnTreeViewStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.TreeViewStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetContainerElementCssClass(TreeViewContainer treeViewContainer)
    {
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("di_tree-view-state di_unselectable ");
        CommonService.UiStringBuilder.Append(GetHasActiveNodeCssClass(treeViewContainer));
        CommonService.UiStringBuilder.Append(" ");
        CommonService.UiStringBuilder.Append(CssClassString);
        
        return CommonService.UiStringBuilder.ToString();
    }
    
    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnTreeViewStateChanged;
        _dotNetHelper?.Dispose();
    }
    
    /* Start TreeViewNodeDisplay */
    private ElementReference? _treeViewTitleElementReference;
    private Key<TreeViewChanged> _previousTreeViewChangedKey = Key<TreeViewChanged>.Empty;
    private bool _previousIsActive;

    // private int OffsetInPixels => TreeViewNodeParameter.RenderBatch.OffsetPerDepthInPixels * TreeViewNodeParameter.Depth;

    
    
    

    /*protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var localIsActive = IsActive;

        if (_previousIsActive != localIsActive)
        {
            _previousIsActive = localIsActive;

            if (localIsActive)
                await FocusAsync().ConfigureAwait(false);
        }
    }*/

    /*private async Task FocusAsync()
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
    }*/

    /*private void HandleExpansionChevronOnMouseDown(TreeViewNoType localTreeViewNoType)
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
    }*/

    /*private async Task ManuallyPropagateOnContextMenu(
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
    }*/

    /*private async Task HandleOnClick(MouseEventArgs? mouseEventArgs)
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
    }*/

    /*private async Task HandleOnDoubleClick(MouseEventArgs? mouseEventArgs)
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
    }*/

    /*private async Task HandleOnMouseDown(MouseEventArgs? mouseEventArgs)
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
    }*/

    /*private Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
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
    }*/

    /*private string GetShowDefaultCursorCssClass(bool isExpandable)
    {
        return isExpandable
            ? string.Empty
            : "di_tree-view-use-default-cursor";
    }*/

    private bool GetIsSelected(TreeViewNoType node) => CommonService.GetTreeViewContainer(TreeViewContainerKey)?.SelectedNodeList.Any(x => x.Key == node.Key) ?? false;
    private string GetIsSelectedCssClass(TreeViewNoType node) => GetIsSelected(node) ? "di_selected" : string.Empty;

    private bool GetIsActive(TreeViewNoType node) => CommonService.GetTreeViewContainer(TreeViewContainerKey)?.ActiveNode is not null &&
                             (CommonService.GetTreeViewContainer(TreeViewContainerKey)?.ActiveNode.Key ?? Key<TreeViewNoType>.Empty) == node.Key;
                             
    private string GetIsActiveId(TreeViewNoType node) => GetIsActive(node)
        ? CommonService.GetTreeViewContainer(TreeViewContainerKey)?.ActiveNodeElementId ?? "string.Empty"
        : string.Empty;
    
    private string GetIsActiveCssClass(TreeViewNoType node) => GetIsActive(node) ? "di_active" : string.Empty;
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetNodeElementCssClass(TreeViewNoType node)
    {
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("di_tree-view-title ");
        CommonService.UiStringBuilder.Append(GetIsSelectedCssClass(node));
        CommonService.UiStringBuilder.Append(" ");
        CommonService.UiStringBuilder.Append(GetIsActiveCssClass(node));
        
        return CommonService.UiStringBuilder.ToString();
    }
    
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    /*private string GetNodeChevronCssClass()
    {
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Clear();
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append("di_tree-view-expansion-chevron ");
        TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.Append(GetShowDefaultCursorCssClass(TreeViewNodeParameter.TreeViewNoType.IsExpandable));
        
        return TreeViewNodeParameter.RenderBatch.CommonService.UiStringBuilder.ToString();
    }*/
    /* End TreeViewNodeDisplay */
}
