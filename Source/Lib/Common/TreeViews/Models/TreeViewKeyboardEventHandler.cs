using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.TreeViews.Models;

/// <summary>
/// To implement custom KeyboardEvent handling logic one should
/// inherit <see cref="TreeViewKeyboardEventHandler"/> and override the corresponding method.
/// </summary>
public class TreeViewKeyboardEventHandler
{
    protected readonly CommonService CommonService;

    public TreeViewKeyboardEventHandler(CommonService commonService)
    {
        CommonService = commonService;
    }

    /// <summary>
    /// Invoked, and awaited, as part of the async UI event handler for 'onkeydownwithpreventscroll' events.<br/><br/>
    /// 
    /// The synchronous version: '<see cref="OnKeyDown(TreeViewCommandArgs)"/>' will be invoked
    /// immediately from within this method, to allow the synchronous code to block the UI purposefully.
    /// 
    /// Any overrides of this method are intended to have 'base.MethodBeingOverridden()' prior to their code.<br/><br/>
    /// </summary>
    public virtual Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
    {
        // Run the synchronous code first
        OnKeyDown(commandArgs);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invoked, and awaited, as part of the synchronous UI event handler for 'onkeydownwithpreventscroll' events.<br/><br/>
    /// 
    /// This method is invoked by the async version: '<see cref="OnKeyDownAsync(TreeViewCommandArgs)"/>'.<br/><br/>
    /// 
    /// Any overrides of this method are intended to have 'base.MethodBeingOverridden()' prior to their code.<br/><br/>
    /// </summary>
    protected virtual void OnKeyDown(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return;

        switch (commandArgs.KeyboardEventArgs.Key)
        {
            case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
                CommonService.TreeView_MoveLeftAction(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_DOWN:
                CommonService.TreeView_MoveDownAction(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_UP:
                CommonService.TreeView_MoveUpAction(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
                CommonService.TreeView_MoveRight(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            case KeyboardKeyFacts.MovementKeys.HOME:
                CommonService.TreeView_MoveHomeAction(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            case KeyboardKeyFacts.MovementKeys.END:
                CommonService.TreeView_MoveEndAction(
                    commandArgs.TreeViewContainer.Key,
                    commandArgs.KeyboardEventArgs.ShiftKey,
					false);
                break;
            default:
                break;
        }
    }
}