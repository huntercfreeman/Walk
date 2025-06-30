using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public interface IPanelTab : ITab
{
    public Key<Panel> Key { get; }
    public Key<ContextRecord> ContextRecordKey { get; }
    public ICommonUiService CommonUiService { get; }
    public CommonBackgroundTaskApi CommonBackgroundTaskApi { get; }
}
