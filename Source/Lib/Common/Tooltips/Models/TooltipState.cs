using System.Text;

namespace Walk.Common.RazorLib.Tooltips.Models;

public record struct TooltipState
{
    public TooltipState(TooltipModel tooltipModel)
    {
        TooltipModel = tooltipModel;
    }

	public TooltipModel TooltipModel { get; }
}
