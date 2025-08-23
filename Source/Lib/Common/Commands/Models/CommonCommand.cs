namespace Walk.Common.RazorLib.Commands.Models;

public class CommonCommand : CommandWithType<CommonCommandArgs>
{
    public static CommonCommand Empty { get; } = new CommonCommand(
        "Do Nothing",
        false,
        _ => ValueTask.CompletedTask);

    public CommonCommand(
            string displayName,
            bool shouldBubble,
            Func<ICommandArgs, ValueTask> commandFunc)
        : base(displayName, shouldBubble, commandFunc)
    {
    }
}
