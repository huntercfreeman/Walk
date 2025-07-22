using System.Text;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Tooltips.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    public TooltipState _tooltipState = new(tooltipModel: null);

    public static readonly Guid Tooltip_htmlElementIdSalt = Guid.NewGuid();
    
    public string Tooltip_HtmlElementId { get; } = $"di_dropdown_{Tooltip_htmlElementIdSalt}";
    public bool Tooltip_IsOffScreenHorizontally { get; }
    public bool Tooltip_IsOffScreenVertically { get; }
    public int Tooltip_RenderCount { get; } = 1;
    
    public StringBuilder Tooltip_StyleBuilder { get; } = new();
    
    public TooltipState GetTooltipState() => _tooltipState;
    
    public void SetTooltipModel(ITooltipModel tooltipModel)
    {
        _tooltipState = new TooltipState(tooltipModel);
        CommonUiStateChanged?.Invoke(CommonUiEventKind.TooltipStateChanged);
    }
}
