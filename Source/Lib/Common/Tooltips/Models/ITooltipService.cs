namespace Walk.Common.RazorLib.Tooltips.Models;

public interface ITooltipService
{
	public event Action? TooltipStateChanged;
	
	public TooltipState GetTooltipState();
	
	public void SetTooltipModel(TooltipModel tooltipModel);
}
