using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class ProjectTestModel
{
	public ProjectTestModel(
		Guid projectIdGuid,
		AbsolutePath absolutePath,
		Func<Func<Dictionary<string, StringFragment>, Task>, Task> enqueueDiscoverTestsFunc,
		Action<TreeViewNoType> reRenderNodeAction)
	{
		ProjectIdGuid = projectIdGuid;
		AbsolutePath = absolutePath;
		EnqueueDiscoverTestsFunc = enqueueDiscoverTestsFunc;
		ReRenderNodeAction = reRenderNodeAction;
	}

	/// <summary>
	/// If null then the test was not a "test-project".
	/// As of this comment that means that the text "The following Tests are available:"
	/// was not found in the output (2024-08-15).
	/// </summary>
	public List<string>? TestNameFullyQualifiedList { get; set; }
	public Func<Func<Dictionary<string, StringFragment>, Task>, Task> EnqueueDiscoverTestsFunc { get; set; }
	public Dictionary<string, StringFragment> RootStringFragmentMap { get; set; } = new();

	public Guid ProjectIdGuid { get; }
	public AbsolutePath AbsolutePath { get; }
	public TerminalCommandRequest? TerminalCommandRequest { get; set; }
	public TerminalCommandParsed? TerminalCommandParsed { get; set; }
	public Key<TerminalCommandRequest> DotNetTestListTestsTerminalCommandRequestKey { get; } = Key<TerminalCommandRequest>.NewKey();
	public Action<TreeViewNoType> ReRenderNodeAction { get; }

	public string DirectoryNameForTestDiscovery => AbsolutePath.ParentDirectory ?? string.Empty;
}
