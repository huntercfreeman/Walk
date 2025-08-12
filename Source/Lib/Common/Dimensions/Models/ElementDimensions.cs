using System.Text;

namespace Walk.Common.RazorLib.Dimensions.Models;

public class ElementDimensions
{
    public DimensionUnit Width_Base_0 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Width_Base_1 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Width_Offset { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Width_WildCard { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    
    public DimensionUnit Height_Base_0 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Height_Base_1 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Height_Offset { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Height_WildCard { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    
    public DimensionUnit Left_Base_0 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Left_Base_1 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Left_Offset { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    
    public DimensionUnit Top_Base_0 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Top_Base_1 { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    public DimensionUnit Top_Offset { get; set; } = new() { DimensionUnitKind = DimensionUnitKind.Pixels };
    
    public ElementPositionKind ElementPositionKind { get; set; } = ElementPositionKind.Static;
    
    public void DisableWidth()
    {
        /*
        Width_Base_0 = Width_Base_0 with { IsUsed = false };
        Width_Base_1 = Width_Base_1 with { IsUsed = false };
        Width_Offset = Width_Offset with { IsUsed = false };
        Width_WildCard = Width_WildCard with { IsUsed = false };
        */
    }
    
    public void DisableHeight()
    {
        /*
        Height_Base_0 = Height_Base_0 with { IsUsed = false };
        Height_Base_1 = Height_Base_1 with { IsUsed = false };
        Height_Offset = Height_Offset with { IsUsed = false };
        Height_WildCard = Height_WildCard with { IsUsed = false };
        */
    }
    
    public void DisableLeft()
    {
        /*
        Left_Base_0 = Left_Base_0 with { IsUsed = false };
        Left_Base_1 = Left_Base_1 with { IsUsed = false };
        Left_Offset = Left_Offset with { IsUsed = false };
        */
    }
    
    public void DisableTop()
    {
        /*
        Top_Base_0 = Top_Base_0 with { IsUsed = false };
        Top_Base_1 = Top_Base_1 with { IsUsed = false };
        Top_Offset = Top_Offset with { IsUsed = false };
        */
    }

    /// <summary>
    /// This method invokes `styleBuilder.Clear();` immediately.
    /// </summary>
    public string GetStyleString(StringBuilder styleBuilder)
    {
        styleBuilder.Clear();
    
        styleBuilder.Append($"position: ");
        styleBuilder.Append(ElementPositionKind.GetStyleString());
        styleBuilder.Append("; ");
        
        // width
        if (Width_Base_0.IsUsed || Width_Base_1.IsUsed || Width_Offset.IsUsed || Width_WildCard.IsUsed)
        {
            styleBuilder.Append("width: calc(");
            if (Width_Base_0.IsUsed)
            {
                styleBuilder.Append(Width_Base_0.Value.ToCssValue());
                styleBuilder.Append(Width_Base_0.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Width_Base_1.IsUsed)
            {
                styleBuilder.Append(Width_Base_1.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Width_Base_1.Value.ToCssValue());
                styleBuilder.Append(Width_Base_1.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Width_Offset.IsUsed)
            {
                styleBuilder.Append(Width_Offset.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Width_Offset.Value.ToCssValue());
                styleBuilder.Append(Width_Offset.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Width_WildCard.IsUsed)
            {
                styleBuilder.Append(Width_WildCard.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Width_WildCard.Value.ToCssValue());
                styleBuilder.Append(Width_WildCard.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            styleBuilder.Append(");");
        }
        
        // height
        if (Height_Base_0.IsUsed || Height_Base_1.IsUsed || Height_Offset.IsUsed || Height_WildCard.IsUsed)
        {
            styleBuilder.Append("height: calc(");
            if (Height_Base_0.IsUsed)
            {
                styleBuilder.Append(Height_Base_0.Value.ToCssValue());
                styleBuilder.Append(Height_Base_0.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Height_Base_1.IsUsed)
            {
                styleBuilder.Append(Height_Base_1.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Height_Base_1.Value.ToCssValue());
                styleBuilder.Append(Height_Base_1.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Height_Offset.IsUsed)
            {
                styleBuilder.Append(Height_Offset.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Height_Offset.Value.ToCssValue());
                styleBuilder.Append(Height_Offset.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Height_WildCard.IsUsed)
            {
                styleBuilder.Append(Height_WildCard.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Height_WildCard.Value.ToCssValue());
                styleBuilder.Append(Height_WildCard.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            styleBuilder.Append(");");
        }
        
        // left
        if (Left_Base_0.IsUsed || Left_Base_1.IsUsed || Left_Offset.IsUsed)
        {
            styleBuilder.Append("left: calc(");
            if (Left_Base_0.IsUsed)
            {
                styleBuilder.Append(Left_Base_0.Value.ToCssValue());
                styleBuilder.Append(Left_Base_0.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Left_Base_1.IsUsed)
            {
                styleBuilder.Append(Left_Base_1.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Left_Base_1.Value.ToCssValue());
                styleBuilder.Append(Left_Base_1.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Left_Offset.IsUsed)
            {
                styleBuilder.Append(Left_Offset.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Left_Offset.Value.ToCssValue());
                styleBuilder.Append(Left_Offset.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            styleBuilder.Append(");");
        }
        
        // top
        if (Top_Base_0.IsUsed || Top_Base_1.IsUsed || Top_Offset.IsUsed)
        {
            styleBuilder.Append("top: calc(");
            
            if (Top_Base_0.IsUsed)
            {
                styleBuilder.Append(Top_Base_0.Value.ToCssValue());
                styleBuilder.Append(Top_Base_0.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Top_Base_1.IsUsed)
            {
                styleBuilder.Append(Top_Base_1.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Top_Base_1.Value.ToCssValue());
                styleBuilder.Append(Top_Base_1.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            if (Top_Offset.IsUsed)
            {
                styleBuilder.Append(" ");
                styleBuilder.Append(Top_Offset.DimensionOperatorKind.GetStyleString());
                styleBuilder.Append(" ");
                styleBuilder.Append(Top_Offset.Value.ToCssValue());
                styleBuilder.Append(Top_Offset.DimensionUnitKind.GetStyleString());
                styleBuilder.Append(" ");
            }
            styleBuilder.Append(");");
        }
        
        return styleBuilder.ToString();
    }
}
