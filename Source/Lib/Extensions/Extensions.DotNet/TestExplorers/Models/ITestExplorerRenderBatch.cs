using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public interface ITestExplorerRenderBatch
{
	public TestExplorerState TestExplorerState { get; }
	public AppOptionsState AppOptionsState { get; }
	public TreeViewContainer? TreeViewContainer { get; }
}
