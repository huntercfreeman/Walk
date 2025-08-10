using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private readonly ConcurrentQueue<CommonWorkArgs> _workQueue = new();
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public bool __TaskCompletionSourceWasCreated { get; set; }
    
    public StringBuilder UiStringBuilder { get; } = new();
    
    public WalkCommonConfig CommonConfig { get; }

    public void Enqueue(CommonWorkArgs commonWorkArgs)
    {
        _workQueue.Enqueue(commonWorkArgs);
        Continuous_Enqueue(this);
    }

    public async ValueTask Do_WriteToLocalStorage(string key, object value)
    {
        var valueJson = System.Text.Json.JsonSerializer.Serialize(value);
        await Storage_SetValue(key, valueJson).ConfigureAwait(false);
    }

    public async ValueTask Do_Tab_ManuallyPropagateOnContextMenu(
        Func<TabContextMenuEventArgs, Task> localHandleTabButtonOnContextMenu, TabContextMenuEventArgs tabContextMenuEventArgs)
    {
        await localHandleTabButtonOnContextMenu.Invoke(tabContextMenuEventArgs).ConfigureAwait(false);
    }

    public async ValueTask Do_TreeView_HandleTreeViewOnContextMenu(
        Func<TreeViewCommandArgs, Task>? onContextMenuFunc, TreeViewCommandArgs treeViewContextMenuCommandArgs)
    {
        if (onContextMenuFunc is not null)
        {
            await onContextMenuFunc
                .Invoke(treeViewContextMenuCommandArgs)
                .ConfigureAwait(false);
        }
    }

    public async ValueTask Do_TreeView_HandleExpansionChevronOnMouseDown(TreeViewNoType localTreeViewNoType, TreeViewContainer treeViewContainer)
    {
        await localTreeViewNoType.LoadChildListAsync().ConfigureAwait(false);
        TreeView_ReRenderNodeAction(treeViewContainer.Key, localTreeViewNoType);
    }

    public async ValueTask Do_TreeView_ManuallyPropagateOnContextMenu(Func<MouseEventArgs?, Key<TreeViewContainer>, TreeViewNoType?, Task> handleTreeViewOnContextMenu, MouseEventArgs mouseEventArgs, Key<TreeViewContainer> key, TreeViewNoType treeViewNoType)
    {
        await handleTreeViewOnContextMenu.Invoke(
                mouseEventArgs,
                key,
                treeViewNoType)
            .ConfigureAwait(false);
    }

    public async ValueTask Do_TreeViewService_LoadChildList(Key<TreeViewContainer> containerKey, TreeViewNoType treeViewNoType)
    {
        try
        {
            await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

            TreeView_ReRenderNodeAction(
                containerKey,
                treeViewNoType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out CommonWorkArgs workArgs))
            return ValueTask.CompletedTask;
            
        switch (workArgs.WorkKind)
        {
            case CommonWorkKind.WriteToLocalStorage:
                return Do_WriteToLocalStorage(workArgs.WriteToLocalStorage_Key, workArgs.WriteToLocalStorage_Value);
            case CommonWorkKind.Tab_ManuallyPropagateOnContextMenu:
                return Do_Tab_ManuallyPropagateOnContextMenu(workArgs.HandleTabButtonOnContextMenu, workArgs.TabContextMenuEventArgs);
            case CommonWorkKind.TreeView_HandleTreeViewOnContextMenu:
                return Do_TreeView_HandleTreeViewOnContextMenu(workArgs.OnContextMenuFunc, workArgs.TreeViewContextMenuCommandArgs);
            case CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown:
                return Do_TreeView_HandleExpansionChevronOnMouseDown(workArgs.TreeViewNoType, workArgs.TreeViewContainer);
            case CommonWorkKind.TreeView_ManuallyPropagateOnContextMenu:
                return Do_TreeView_ManuallyPropagateOnContextMenu(workArgs.HandleTreeViewOnContextMenu, workArgs.MouseEventArgs, workArgs.ContainerKey, workArgs.TreeViewNoType);
            case CommonWorkKind.TreeViewService_LoadChildList:
                return Do_TreeViewService_LoadChildList(workArgs.ContainerKey, workArgs.TreeViewNoType);
            default:
                Console.WriteLine($"{nameof(CommonService)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
        }
    }
}
