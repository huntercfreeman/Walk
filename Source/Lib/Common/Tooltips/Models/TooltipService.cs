using System.Text;
using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Common.RazorLib.Tooltips.Models;

public class TooltipService : ITooltipService
{
    public TooltipState _tooltipState = new(tooltipModel: null);

	public static readonly Guid _htmlElementIdSalt = Guid.NewGuid();
    
	public string HtmlElementId { get; } = $"di_dropdown_{_htmlElementIdSalt}";
	public MeasuredHtmlElementDimensions HtmlElementDimensions { get; set; }
	public MeasuredHtmlElementDimensions GlobalHtmlElementDimensions { get; set; }
	public bool IsOffScreenHorizontally { get; }
	public bool IsOffScreenVertically { get; }
	public int RenderCount { get; } = 1;
	
	public StringBuilder StyleBuilder { get; } = new();
	
	public event Action? TooltipStateChanged;
	
	public TooltipState GetTooltipState() => _tooltipState;
	
	public void SetTooltipModel(TooltipModel tooltipModel)
	{
	    _tooltipState = new TooltipState(tooltipModel);
	    TooltipStateChanged?.Invoke();
	}
}
