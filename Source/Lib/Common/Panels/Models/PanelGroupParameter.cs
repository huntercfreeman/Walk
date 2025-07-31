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
        Func<Task> reRenderSelfAndAdjacentElementDimensionsFunc,
        string cssClassString,
        IReadOnlyList<IBadgeModel>? badgeList)
    {
        PanelGroupKey = panelGroupKey;
        AdjacentElementDimensions = adjacentElementDimensions;
        DimensionAttributeKind = dimensionAttributeKind;
        ReRenderSelfAndAdjacentElementDimensionsFunc = reRenderSelfAndAdjacentElementDimensionsFunc;
        CssClassString = cssClassString;
        BadgeList = badgeList;
    }

    public Key<PanelGroup> PanelGroupKey { get; set; } = Key<PanelGroup>.Empty;
    public ElementDimensions AdjacentElementDimensions { get; set; }
    public DimensionAttributeKind DimensionAttributeKind { get; set; }
    public Func<Task> ReRenderSelfAndAdjacentElementDimensionsFunc { get; set; } = null!;
    public string CssClassString { get; set; } = null!;
    public IReadOnlyList<IBadgeModel>? BadgeList { get; set; } = null;
}
