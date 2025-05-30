using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib.Dimensions.Models;

public static class DimensionUnitKindExtensions
{
    public static string GetStyleString(this DimensionUnitKind dimensionUnitKind)
    {
        return dimensionUnitKind switch
        {
            DimensionUnitKind.Pixels => "px",
            DimensionUnitKind.ViewportWidth => "vw",
            DimensionUnitKind.ViewportHeight => "vh",
            DimensionUnitKind.Percentage => "%",
            DimensionUnitKind.RootCharacterWidth => "rch",
            DimensionUnitKind.RootCharacterHeight => "rem",
            DimensionUnitKind.CharacterWidth => "ch",
            DimensionUnitKind.CharacterHeight => "em",
            _ => throw new WalkCommonException($"The {nameof(DimensionUnitKind)}: '{dimensionUnitKind}' was not recognized.")
        };
    }
}