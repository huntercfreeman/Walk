using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public interface IStartupControlModel
{
    /// <summary>
    /// By default, this is used per option html element within the select dropdown.
    /// </summary>
    public string Title { get; }
    
    public AbsolutePath StartupProjectAbsolutePath { get; }
    
    public bool IsExecuting { get; }
    
    /// <summary>
    /// This accepts an object because:
    /// The StartupControlModel for C# needs the DotNetService.
    /// Rather than store a reference to the DotNetService
    /// foreach of the projects.
    /// Can just pass it in for the occasions where
    /// the user clicks the start/stop buttons.
    /// </summary>
    public Task StartButtonOnClick(object item);
    /// <summary>
    /// This accepts an object because:
    /// The StartupControlModel for C# needs the DotNetService.
    /// Rather than store a reference to the DotNetService
    /// foreach of the projects.
    /// Can just pass it in for the occasions where
    /// the user clicks the start/stop buttons.
    /// </summary>
    public Task StopButtonOnClick(object item);
}
