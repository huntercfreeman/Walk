namespace Walk.Common.RazorLib.Commands.Models;

public abstract class CommandWithType<T> : CommandNoType where T : notnull
{
    protected CommandWithType(
            string displayName,
            bool shouldBubble,
            Func<ICommandArgs, ValueTask> commandFunc) 
        : base(displayName, shouldBubble, commandFunc)
    {
    }
}
