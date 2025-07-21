using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib.Dimensions.Models;

public static class DimensionExtensions
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
    
    public static string GetStyleString(this DimensionOperatorKind dimensionOperatorKind)
    {
        return dimensionOperatorKind switch
        {
            DimensionOperatorKind.Add => "+",
            DimensionOperatorKind.Subtract => "-",
            DimensionOperatorKind.Multiply => "*",
            DimensionOperatorKind.Divide => "/",
            _ => throw new WalkCommonException($"The {nameof(DimensionOperatorKind)}: '{dimensionOperatorKind}' was not recognized.")
        };
    }
    
    public static string GetStyleString(this DimensionAttributeKind dimensionAttributeKind)
    {
        return dimensionAttributeKind switch
        {
            DimensionAttributeKind.Width => "width",
            DimensionAttributeKind.Height => "height",
            DimensionAttributeKind.Left => "left",
            DimensionAttributeKind.Right => "right",
            DimensionAttributeKind.Top => "top",
            DimensionAttributeKind.Bottom => "bottom",
            _ => throw new WalkCommonException($"The {nameof(DimensionAttributeKind)}: '{dimensionAttributeKind}' was not recognized.")
        };
    }
    
    public static string GetStyleString(this ElementPositionKind elementPositionKind)
    {
        return elementPositionKind switch
        {
            ElementPositionKind.Static => "static",
            ElementPositionKind.Absolute => "absolute",
            ElementPositionKind.Fixed => "fixed",
            ElementPositionKind.Inherit => "inherit",
            ElementPositionKind.Relative => "relative",
            ElementPositionKind.Revert => "revert",
            ElementPositionKind.Sticky => "sticky",
            ElementPositionKind.Unset => "unset",
            _ => throw new WalkCommonException($"The {nameof(ElementPositionKind)}: '{elementPositionKind}' was not recognized.")
        };
    }
}
