using System.Text;
using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Common.RazorLib.Tooltips.Models;

public interface ITooltipService
{
	public string HtmlElementId { get; }
	public MeasuredHtmlElementDimensions HtmlElementDimensions { get; set; }
	public MeasuredHtmlElementDimensions GlobalHtmlElementDimensions { get; set; }
	public bool IsOffScreenHorizontally { get; }
	public bool IsOffScreenVertically { get; }
	public int RenderCount { get; }
	
	public StringBuilder StyleBuilder { get; }

	public event Action? TooltipStateChanged;
	
	public TooltipState GetTooltipState();
	
	public void SetTooltipModel(TooltipModel tooltipModel);
}
