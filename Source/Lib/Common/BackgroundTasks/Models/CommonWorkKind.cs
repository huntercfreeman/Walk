namespace Walk.Common.RazorLib.BackgroundTasks.Models;

public enum CommonWorkKind
{
    None,
    WriteToLocalStorage,
    Tab_ManuallyPropagateOnContextMenu,
    TreeView_HandleTreeViewOnContextMenu,
    TreeView_HandleExpansionChevronOnMouseDown,
    TreeView_ManuallyPropagateOnContextMenu,
    TreeViewService_LoadChildList,
}
