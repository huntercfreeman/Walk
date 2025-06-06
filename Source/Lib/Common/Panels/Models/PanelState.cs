using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Panels.Models;

/// <summary>
/// Once the 'PanelGroupList'/'PanelList' are exposed publically,
/// they should NOT be modified.
/// Make a shallow copy 'new List<PanelGroup>(panelState.PanelGroupList);'/
/// 'new List<Panel>(panelState.PanelList);'
/// and modify the shallow copy if modification of the list
/// after exposing it publically is necessary.
///
/// ---
///
/// TODO: SphagettiCode - The resizing and hiding/showing is a bit scuffed. (2023-09-19)
/// </summary>
public record struct PanelState(
	IReadOnlyList<PanelGroup> PanelGroupList,
	IReadOnlyList<Panel> PanelList,
	(IPanelTab PanelTab, PanelGroup PanelGroup)? DragEventArgs)
{
    public PanelState() : this(Array.Empty<PanelGroup>(), Array.Empty<Panel>(), null)
    {
        var topLeftGroup = ConstructTopLeftGroup();
        var topRightGroup = ConstructTopRightGroup();
        var bottomGroup = ConstructBottomGroup();
        
        var outPanelGroupList = new List<PanelGroup>();
        outPanelGroupList.Add(topLeftGroup);
        outPanelGroupList.Add(topRightGroup);
        outPanelGroupList.Add(bottomGroup);
        
        PanelGroupList = outPanelGroupList;
    }

    private static PanelGroup ConstructTopLeftGroup()
    {
        var leftPanelGroup = new PanelGroup(
            PanelFacts.LeftPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        leftPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(33.3333, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET)
        });

        return leftPanelGroup;
    }

    private static PanelGroup ConstructTopRightGroup()
    {
        var rightPanelGroup = new PanelGroup(
            PanelFacts.RightPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        rightPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(33.3333, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET),
        });

        return rightPanelGroup;
    }

    private static PanelGroup ConstructBottomGroup()
    {
        var bottomPanelGroup = new PanelGroup(
            PanelFacts.BottomPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        bottomPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(22, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET),
            new DimensionUnit(SizeFacts.Ide.Header.Height.Value / 2, SizeFacts.Ide.Header.Height.DimensionUnitKind, DimensionOperatorKind.Subtract)
        });

        return bottomPanelGroup;
    }
}