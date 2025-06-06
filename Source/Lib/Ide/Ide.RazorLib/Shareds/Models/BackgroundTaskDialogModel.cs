using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public class BackgroundTaskDialogModel
{
    public BackgroundTaskDialogModel(List<IBackgroundTaskGroup> seenBackgroundTasks)
    {
        SeenBackgroundTasks = seenBackgroundTasks;
    }

    public List<IBackgroundTaskGroup> SeenBackgroundTasks { get; }
    public Func<Task>? ReRenderFuncAsync { get; set; }
    public int CountTracked { get; set; } = 500;
}
