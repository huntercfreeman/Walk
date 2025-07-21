using System.Text;

namespace Walk.Common.RazorLib.Dimensions.Models;

public class ElementDimensions
{
    public DimensionAttribute WidthDimensionAttribute { get; set; } = new(DimensionAttributeKind.Width);
    public DimensionAttribute HeightDimensionAttribute { get; set; } = new(DimensionAttributeKind.Height);
    public DimensionAttribute LeftDimensionAttribute { get; set; } = new(DimensionAttributeKind.Left);
    public DimensionAttribute RightDimensionAttribute { get; set; } = new(DimensionAttributeKind.Right);
    public DimensionAttribute TopDimensionAttribute { get; set; } = new(DimensionAttributeKind.Top);
    public DimensionAttribute BottomDimensionAttribute { get; set; } = new(DimensionAttributeKind.Bottom);
    
    public ElementPositionKind ElementPositionKind { get; set; } = ElementPositionKind.Static;

    /// <summary>
    /// This method invokes `styleBuilder.Clear();` immediately.
    /// </summary>
    public string GetStyleString(StringBuilder styleBuilder)
    {
        styleBuilder.Clear();
    
        styleBuilder.Append($"position: ");
        styleBuilder.Append(ElementPositionKind.GetStyleString());
        styleBuilder.Append("; ");
        
        WidthDimensionAttribute.AppendStyleString(styleBuilder);
        HeightDimensionAttribute.AppendStyleString(styleBuilder);
        LeftDimensionAttribute.AppendStyleString(styleBuilder);
        RightDimensionAttribute.AppendStyleString(styleBuilder);
        TopDimensionAttribute.AppendStyleString(styleBuilder);
        BottomDimensionAttribute.AppendStyleString(styleBuilder);

        return styleBuilder.ToString();
    }
}
