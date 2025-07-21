using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Shareds.Displays;

namespace Walk.Ide.RazorLib.BackgroundTasks.Models;

public sealed class IdeWorkArgs
{
    public IdeWorkKind WorkKind { get; set; }
    public IdeMainLayout IdeMainLayout { get; set; }
    public string StringValue { get; set; }
    public TextEditorModel TextEditorModel { get; set; }
    public DateTime FileLastWriteTime { get; set; }
    public Key<IDynamicViewModel> NotificationInformativeKey { get; set; }
    public AbsolutePath AbsolutePath { get; set; }
    public TreeViewAbsolutePath TreeViewAbsolutePath { get; set; }
    public NamespacePath NamespacePath { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public Func<Task> OnAfterCompletion { get; set; }
    public Func<AbsolutePath, Task> OnAfterSubmitFunc { get; set; }
    public Func<AbsolutePath, Task<bool>> SelectionIsValidFunc { get; set; }
    public Func<DateTime?, Task> OnAfterSaveCompletedWrittenDateTimeFunc { get; set; }
    public IFileTemplate? ExactMatchFileTemplate { get; set; }
    public List<InputFilePattern> InputFilePatterns { get; set; }
    public List<IFileTemplate> RelatedMatchFileTemplatesList { get; set; }
}
