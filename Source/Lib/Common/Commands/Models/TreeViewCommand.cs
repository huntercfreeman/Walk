namespace Walk.Common.RazorLib.Commands.Models;

public class TreeViewCommand : CommandWithType<TreeViewCommandArgs>
{
    public TreeViewCommand(
            string displayName,
            bool shouldBubble,
            Func<ICommandArgs, ValueTask> commandFunc)
        : base(displayName, shouldBubble, commandFunc)
    {
    }
}
