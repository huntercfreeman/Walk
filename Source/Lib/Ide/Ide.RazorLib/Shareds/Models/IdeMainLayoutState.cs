using Walk.Common.RazorLib.Badges.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct IdeMainLayoutState(IReadOnlyList<IBadgeModel> FooterJustifyEndComponentList)
{
	public IdeMainLayoutState() : this(Array.Empty<IBadgeModel>())
	{
	}
}
