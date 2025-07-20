using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public record struct TerminalGroupState(Key<ITerminal> ActiveTerminalKey)
{
    public TerminalGroupState() : this(IdeFacts.GENERAL_KEY)
    {
        // _bodyElementDimensions
        {
            BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
            {
                new DimensionUnit(
                	80,
                	DimensionUnitKind.Percentage),
                new DimensionUnit(
                	0,
                	DimensionUnitKind.Pixels,
                	DimensionOperatorKind.Subtract,
                	CommonFacts.PURPOSE_OFFSET),
            });
        }

        // _tabsElementDimensions
        {
            TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
            {
                new DimensionUnit(
                	20,
                	DimensionUnitKind.Percentage),
                new DimensionUnit(
                	0,
                	DimensionUnitKind.Pixels,
                	DimensionOperatorKind.Subtract,
                	CommonFacts.PURPOSE_OFFSET),
            });
        }
    }

    public ElementDimensions BodyElementDimensions { get; } = new();
	public ElementDimensions TabsElementDimensions { get; } = new();
}
