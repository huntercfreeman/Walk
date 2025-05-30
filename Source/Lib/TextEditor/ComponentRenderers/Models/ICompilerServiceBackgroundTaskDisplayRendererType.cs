using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.TextEditor.RazorLib.ComponentRenderers.Models;

public interface ICompilerServiceBackgroundTaskDisplayRendererType
{
    public IBackgroundTaskGroup BackgroundTask { get; set; }
}