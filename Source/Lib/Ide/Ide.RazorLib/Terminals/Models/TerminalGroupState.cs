using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public record struct TerminalGroupState(Key<ITerminal> ActiveTerminalKey)
{
    public TerminalGroupState() : this(IdeFacts.GENERAL_KEY)
    {
        // _bodyElementDimensions
        BodyElementDimensions.Width_Base_0 = new DimensionUnit(80, DimensionUnitKind.Percentage);
        BodyElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // _tabsElementDimensions
        TabsElementDimensions.Width_Base_0 = new DimensionUnit(20, DimensionUnitKind.Percentage);
        TabsElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);
    }

    public ElementDimensions BodyElementDimensions { get; } = new();
    public ElementDimensions TabsElementDimensions { get; } = new();
}
