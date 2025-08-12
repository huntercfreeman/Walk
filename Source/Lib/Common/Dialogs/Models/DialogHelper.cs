using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Dialogs.Models;

public static class DialogHelper
{
    public static ElementDimensions ConstructDefaultElementDimensions()
    {
        var elementDimensions = new ElementDimensions
        {
            ElementPositionKind = ElementPositionKind.Fixed
        };

        elementDimensions.Width_Base_0 = new DimensionUnit(60, DimensionUnitKind.ViewportWidth);

        elementDimensions.Height_Base_0 = new DimensionUnit(60, DimensionUnitKind.ViewportHeight);

        elementDimensions.Left_Base_0 = new DimensionUnit(20, DimensionUnitKind.ViewportWidth);

        elementDimensions.Top_Base_0 = new DimensionUnit(20, DimensionUnitKind.ViewportHeight);

        return elementDimensions;
    }
}

