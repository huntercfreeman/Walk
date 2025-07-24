using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
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
    
    /// <summary>
    /// UI thread only.
    ///
    /// This property is somewhat-awkwardly set from the '.razor'.
    /// All usage of this is expected to be on UI thread for that reason.
    /// If doing async work, be sure to NOT use '.ConfigureAwait(false)'
    /// if you intend to use this property after the task finishes.
    /// </summary>
    private int IndexActiveNode { get; set; }

    private TreeViewCommandArgs _treeViewContextMenuCommandArgs;
    private TreeViewContainer _treeViewContainer;
    private TreeViewMeasurements _treeViewMeasurements;
    private DotNetObjectReference<TreeViewContainerDisplay>? _dotNetHelper;
    
    /// <summary>
    /// UI thread only.
    /// </summary>
    private readonly List<TreeViewNoType> _flatNodeList = new();
    /// <summary>
    /// UI thread only.
    /// Contains the "used to be" targetNode, and the index that it left off at.
    /// </summary>
    private readonly Stack<(TreeViewNoType Node, int Index)> _nodeRecursionStack = new();

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
        }
    }
    
    [JSInvokable]
    public async Task ReceiveOnKeyDown(TreeViewEventArgsKeyDown eventArgsKeyDown)
    {
        if (_treeViewContainer is null)
            return;

        switch (eventArgsKeyDown.Key)
        {
            case "ContextMenu":
            {
                var mouseEventArgs = new MouseEventArgs { Button = -1 };
                
                ReceiveOnContextMenu(
                    new TreeViewEventArgsMouseDown(
                        Buttons: 0,
                        Button: -1,
                        X: 0,
                        Y: 0,
                        ShiftKey: false,
                        eventArgsKeyDown.ScrollLeft,
                        eventArgsKeyDown.ScrollTop,
                        eventArgsKeyDown.ViewWidth,
                        eventArgsKeyDown.ViewHeight,
                        eventArgsKeyDown.BoundingClientRectLeft,
                        eventArgsKeyDown.BoundingClientRectTop));
                return;
            }
            case ".":
            {
                if (eventArgsKeyDown.CtrlKey)
                {
                    ReceiveOnContextMenu(
                        new TreeViewEventArgsMouseDown(
                            Buttons: 0,
                            Button: -1,
                            X: 0,
                            Y: 0,
                            ShiftKey: false,
                            eventArgsKeyDown.ScrollLeft,
                            eventArgsKeyDown.ScrollTop,
                            eventArgsKeyDown.ViewWidth,
                            eventArgsKeyDown.ViewHeight,
                            eventArgsKeyDown.BoundingClientRectLeft,
                            eventArgsKeyDown.BoundingClientRectTop));
                }
                return;
            }
            case "F10":
            {
                if (eventArgsKeyDown.ShiftKey)
                {
                    ReceiveOnContextMenu(
                        new TreeViewEventArgsMouseDown(
                            Buttons: 0,
                            Button: -1,
                            X: 0,
                            Y: 0,
                            ShiftKey: false,
                            eventArgsKeyDown.ScrollLeft,
                            eventArgsKeyDown.ScrollTop,
                            eventArgsKeyDown.ViewWidth,
                            eventArgsKeyDown.ViewHeight,
                            eventArgsKeyDown.BoundingClientRectLeft,
                            eventArgsKeyDown.BoundingClientRectTop));
                }
                return;
            }
        }
        
        var treeViewCommandArgs = new TreeViewCommandArgs(
            CommonService,
            _treeViewContainer,
            null,
            async () =>
            {
                _treeViewMeasurements = await JsRuntime.InvokeAsync<TreeViewMeasurements>(
                    "walkCommon.focusAndMeasureTreeView",
                    _htmlId,
                    /*preventScroll:*/ false);
            },
            null,
            null,
            new KeyboardEventArgs
            {
                Key = eventArgsKeyDown.Key,
                Code = eventArgsKeyDown.Code,
            });

        // Do not ConfigureAwait(false) here, the _flatNodeList is made on the UI thread
        // and after this await we need to read the _flatNodeList to scroll the newly active node into view.
        await TreeViewKeyboardEventHandler.OnKeyDownAsync(treeViewCommandArgs);
        
        var treeViewContainerLocal = CommonService.GetTreeViewContainer(TreeViewContainerKey);
        
        if (treeViewContainerLocal is null)
            return;
    
        for (int i = 0; i < _flatNodeList.Count; i++)
        {
            var node = _flatNodeList[i];
            if (node == treeViewContainerLocal.ActiveNode)
            {
                var top = _lineHeight * i;
                
                if (top < eventArgsKeyDown.ScrollTop)
                {
                    await JsRuntime.InvokeVoidAsync("walkCommon.treeViewScrollVertical", _htmlId, top - eventArgsKeyDown.ScrollTop);
                }
                else if (top + _lineHeight > eventArgsKeyDown.ScrollTop + eventArgsKeyDown.ViewHeight)
                {
                    await JsRuntime.InvokeVoidAsync("walkCommon.treeViewScrollVertical", _htmlId, top - (eventArgsKeyDown.ScrollTop + eventArgsKeyDown.ViewHeight) + _lineHeight);
                }
                break;
            }
        }
    }
    
    [JSInvokable]
    public void ReceiveOnContextMenu(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
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
            contextMenuTarget = _flatNodeList[IndexActiveNode];
            
            contextMenuFixedPosition = new ContextMenuFixedPosition(
                OccurredDueToMouseEvent: false,
                LeftPositionInPixels: eventArgsMouseDown.BoundingClientRectLeft,
                TopPositionInPixels: eventArgsMouseDown.BoundingClientRectTop + _lineHeight + (IndexActiveNode * _lineHeight) - eventArgsMouseDown.ScrollTop);
        }
        else if (eventArgsMouseDown.Button == 2)
        {
            var relativeY = eventArgsMouseDown.Y - _treeViewMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
            relativeY = Math.Max(0, relativeY);
            
            var indexLocal = (int)(relativeY / _lineHeight);
            
            IndexActiveNode = IndexBasicValidation(indexLocal);
            contextMenuTarget = _flatNodeList[IndexActiveNode];
            
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
            _flatNodeList[IndexActiveNode],
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
        
        IndexActiveNode = IndexBasicValidation(indexLocal);
        
        var relativeX = eventArgsMouseDown.X - _treeViewMeasurements.BoundingClientRectLeft + eventArgsMouseDown.ScrollLeft;
        relativeX = Math.Max(0, relativeX);
        
        // TODO: Determine why my math is wrong...
        // ...I need to subtract 1.1 for lower bound and subtract 1 for upper bound.
        // So the question is, "Why do I need to add this arbitrary subtractions,
        // and are the arbitrary subtractions different depending on display settings / font sizes / etc...".
        // Given my setup, these arbitrary subtractions make the hitbox "feel" pixel perfect.
        //
        if (relativeX >= (_flatNodeList[IndexActiveNode].Depth * OffsetPerDepthInPixels - 1.1) &&
            relativeX <= (_flatNodeList[IndexActiveNode].Depth * OffsetPerDepthInPixels + WalkTreeViewIconWidth - 1))
        {
            HandleChevronOnClick(eventArgsMouseDown);
        }
        
        CommonService.TreeView_SetActiveNodeAction(
            _treeViewContainer.Key,
            _flatNodeList[IndexActiveNode],
            addSelectedNodes: false,
            selectNodesBetweenCurrentAndNextActiveNode: false);
    }
    
    [JSInvokable]
    public async Task ReceiveOnClick(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        Console.WriteLine("ReceiveOnClick");
    
        _treeViewMeasurements = new TreeViewMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    
        var relativeY = eventArgsMouseDown.Y - _treeViewMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
        relativeY = Math.Max(0, relativeY);
        
        var indexLocal = (int)(relativeY / _lineHeight);
        
        IndexActiveNode = IndexBasicValidation(indexLocal);
        
        await TreeViewMouseEventHandler.OnClickAsync(new TreeViewCommandArgs(
            CommonService,
            _treeViewContainer,
            _flatNodeList[IndexActiveNode],
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
        
        IndexActiveNode = IndexBasicValidation(indexLocal);
        
        await TreeViewMouseEventHandler.OnDoubleClickAsync(new TreeViewCommandArgs(
            CommonService,
            _treeViewContainer,
            _flatNodeList[IndexActiveNode],
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
            childNode.Depth = depth;
            _flatNodeList.Add(childNode);
        
            if (childNode.IsExpanded && childNode.ChildList.Count > 0)
            {
                _nodeRecursionStack.Push((targetNode, index));
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
    
    private void HandleChevronOnClick(TreeViewEventArgsMouseDown eventArgsMouseDown)
    {
        var localTreeViewNoType = _flatNodeList[IndexActiveNode];
        
        if (!localTreeViewNoType.IsExpandable)
            return;

        localTreeViewNoType.IsExpanded = !localTreeViewNoType.IsExpanded;

        if (localTreeViewNoType.IsExpanded)
        {
            CommonService.Enqueue(new CommonWorkArgs
            {
                WorkKind = CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown,
                TreeViewNoType = localTreeViewNoType,
                TreeViewContainer = _treeViewContainer
            });
        }
        else
        {
            CommonService.TreeView_ReRenderNodeAction(_treeViewContainer.Key, localTreeViewNoType);
        }
    }

    private string GetHasActiveNodeCssClass(TreeViewContainer? treeViewContainer)
    {
        if (treeViewContainer?.ActiveNode is null)
            return string.Empty;

        return "di_active";
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
                             
    /// <summary>
    /// This method should only be invoked from the "UI thread" due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private string GetNodeElementCssStyle(TreeViewNoType node)
    {
        
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("display: flex; align-items: center; padding-left: ");
        CommonService.UiStringBuilder.Append(node.Depth * OffsetPerDepthInPixels);
        CommonService.UiStringBuilder.Append("px; height: ");
        CommonService.UiStringBuilder.Append(_lineHeight);
        CommonService.UiStringBuilder.Append("px;");
        
        return CommonService.UiStringBuilder.ToString();
    }
    
    public void Dispose()
    {
        CommonService.CommonUiStateChanged -= OnTreeViewStateChanged;
        _dotNetHelper?.Dispose();
    }
}
