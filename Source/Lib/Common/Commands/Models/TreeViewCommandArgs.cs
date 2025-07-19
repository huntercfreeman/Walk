using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.Commands.Models;

/// <summary>
/// Verify that 'TreeViewService is not null' to know this was constructed rather than default.
/// </summary>
public record struct TreeViewCommandArgs : ICommandArgs
{
    public TreeViewCommandArgs(
        CommonService commonService,
        TreeViewContainer treeViewContainer,
        TreeViewNoType? nodeThatReceivedMouseEvent,
        Func<Task> restoreFocusToTreeView,
        ContextMenuFixedPosition? contextMenuFixedPosition,
        MouseEventArgs? mouseEventArgs,
        KeyboardEventArgs? keyboardEventArgs)
    {
        CommonService = CommonService;
        TreeViewContainer = treeViewContainer;
        NodeThatReceivedMouseEvent = nodeThatReceivedMouseEvent;
        RestoreFocusToTreeView = restoreFocusToTreeView;
        ContextMenuFixedPosition = contextMenuFixedPosition;
        MouseEventArgs = mouseEventArgs;
        KeyboardEventArgs = keyboardEventArgs;
    }

    public CommonService CommonService { get; }
    public TreeViewContainer TreeViewContainer { get; }
    public TreeViewNoType? NodeThatReceivedMouseEvent { get; }
    public Func<Task> RestoreFocusToTreeView { get; }
    public ContextMenuFixedPosition? ContextMenuFixedPosition { get; }
    public MouseEventArgs? MouseEventArgs { get; }
    public KeyboardEventArgs? KeyboardEventArgs { get; }
}
