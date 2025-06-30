using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct StartupControlState(
	Key<IStartupControlModel> ActiveStartupControlKey,
	IReadOnlyList<IStartupControlModel> StartupControlList)
{
	public StartupControlState() : this(
		Key<IStartupControlModel>.Empty,
		Array.Empty<IStartupControlModel>())
	{
	}
}
