namespace Walk.Common.RazorLib.Tooltips.Models;

public record struct TooltipState
{
    public TooltipState(ITooltipModel tooltipModel)
    {
        TooltipModel = tooltipModel;
    }

	public ITooltipModel TooltipModel { get; }
}
