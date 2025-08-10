using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Badges.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public struct PanelGroupParameter
{
    public PanelGroupParameter(
        Key<PanelGroup> panelGroupKey,
        ElementDimensions adjacentElementDimensions,
        DimensionAttributeKind dimensionAttributeKind,
        string cssClassString)
    {
        PanelGroupKey = panelGroupKey;
        AdjacentElementDimensions = adjacentElementDimensions;
        DimensionAttributeKind = dimensionAttributeKind;
        CssClassString = cssClassString;
    }

    public Key<PanelGroup> PanelGroupKey { get; set; } = Key<PanelGroup>.Empty;
    public ElementDimensions AdjacentElementDimensions { get; set; }
    public DimensionAttributeKind DimensionAttributeKind { get; set; }
    public string CssClassString { get; set; } = null!;
    
    public DimensionUnitPurposeKind DimensionUnitPurposeKind { get; set; }
    public string PanelPositionCss { get; set; }
    public string HtmlIdTabs { get; set; }
}
