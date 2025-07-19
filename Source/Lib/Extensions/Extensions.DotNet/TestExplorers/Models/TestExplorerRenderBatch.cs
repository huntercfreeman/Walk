using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TestExplorerRenderBatch : ITestExplorerRenderBatch
{
	public TestExplorerRenderBatch(
		TestExplorerState testExplorerState,
		AppOptionsState appOptionsState,
		TreeViewContainer? treeViewContainer)
	{
		TestExplorerState = testExplorerState;
		AppOptionsState = appOptionsState;
		TreeViewContainer = treeViewContainer;
	}

	public TestExplorerState TestExplorerState { get; }
	public AppOptionsState AppOptionsState { get; }
	public TreeViewContainer? TreeViewContainer { get; }
}
