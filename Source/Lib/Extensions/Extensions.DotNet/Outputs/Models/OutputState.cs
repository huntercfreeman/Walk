using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.Outputs.Models;

public record struct OutputState(Guid DotNetRunParseResultId)
{
	public static readonly Key<TreeViewContainer> TreeViewContainerKey = Key<TreeViewContainer>.NewKey();
	
	public OutputState() : this(Guid.Empty)
	{
	}
}
