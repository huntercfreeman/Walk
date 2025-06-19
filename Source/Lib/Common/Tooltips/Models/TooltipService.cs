namespace Walk.Common.RazorLib.Tooltips.Models;

public class TooltipService : ITooltipService
{
    private TooltipState _tooltipState = new(tooltipModel: null);

	public event Action? TooltipStateChanged;
	
	public TooltipState GetTooltipState() => _tooltipState;
	
	public void SetTooltipModel(TooltipModel tooltipModel)
	{
	    _tooltipState = new TooltipState(tooltipModel);
	    TooltipStateChanged?.Invoke();
	}
}
