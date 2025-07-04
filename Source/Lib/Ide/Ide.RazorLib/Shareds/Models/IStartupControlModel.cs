using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public interface IStartupControlModel
{
	public Key<IStartupControlModel> Key { get; }
	
	/// <summary>
	/// By default, this is used per option html element within the select dropdown.
	/// </summary>
	public string Title { get; }
	
	/// <summary>
	/// By default, this is used as hover text (HTML 'title' attribute on the select dropdown)
	/// </summary>
	public string TitleVerbose { get; }
	
	public AbsolutePath StartupProjectAbsolutePath { get; }
	
	/// <summary>
	/// If more than a 'start button' is necessary, one can provide a Blazor component,
	/// and it will be rendered "to the left"/"prior" to the start button.
	/// </summary>
	public Type? ComponentType { get; }
	
	/// <summary>
	/// The Blazor parameters to pass to the <see cref="ComponentType"/>
	/// </summary>
	public Dictionary<string, object?>? ComponentParameterMap { get; }

	public Func<IStartupControlModel, Task> StartButtonOnClickTask { get; }
	public Func<IStartupControlModel, Task> StopButtonOnClickTask { get; }
	
	public bool IsExecuting { get; }
}
