using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class CommonUiDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    private int _eventMatchedCount;

    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
    
    public string TooltipRelativeX { get; set; } = string.Empty;
    public string TooltipRelativeY { get; set; } = string.Empty;
    
    private ITooltipModel? _tooltipModelPrevious = null;
    
    private readonly string _measureLineHeightElementId = "di_measure-lineHeight";
    
    /// <summary>The unit of measurement is Pixels (px)</summary>
    public const double OUTLINE_THICKNESS = 4;
    
    private string _lineHeightCssStyle;
    
    private string _measureCharacterWidthAndLineHeightElementId = "di_te_measure-charWidth-lineHeight";
    
    private string _wrapperCssClass;
    private string _wrapperCssStyle;
    
    protected override void OnInitialized()
    {
        
    }
    
    /// <summary>TODO: Thread safety</summary>
    protected override bool ShouldRender()
    {
        if (_eventMatchedCount > 0)
        {
            _eventMatchedCount--;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void Dispose()
    {
    }
}
