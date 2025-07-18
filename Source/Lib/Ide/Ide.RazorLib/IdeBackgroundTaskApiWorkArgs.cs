using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Shareds.Displays;

namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

/*
These IBackgroundTaskGroup "args" structs are a bit heavy at the moment.
This is better than how things were, I need to find another moment
to go through and lean these out.
*/
public struct IdeBackgroundTaskApiWorkArgs
{
	public IdeWorkKind WorkKind { get; set; }
    public IdeMainLayout IdeMainLayout { get; set; }
    public string InputFileAbsolutePathString { get; set; }
    public TextEditorModel TextEditorModel { get; set; }
    public DateTime FileLastWriteTime { get; set; }
    public Key<IDynamicViewModel> NotificationInformativeKey { get; set; }
    public AbsolutePath AbsolutePath { get; set; }
    public string Content { get; set; }
    public Func<DateTime?, Task> OnAfterSaveCompletedWrittenDateTimeFunc { get; set; }
    public CancellationToken CancellationToken { get; set; }
    
    public string Message { get; set; }
    public Func<AbsolutePath, Task> OnAfterSubmitFunc { get; set; }
    public Func<AbsolutePath, Task<bool>> SelectionIsValidFunc { get; set; }
    public List<InputFilePattern> InputFilePatterns { get; set; }
}
