using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib.Dimensions.Models;

/// <summary>
/// default when 'Purpose is null'.
/// </summary>
public struct DimensionUnit
{
    public DimensionUnit(
        DimensionUnitKind dimensionUnitKind,
        DimensionOperatorKind dimensionOperatorKind,
        DimensionUnitPurposeKind purpose)
    {
        DimensionUnitKind = dimensionUnitKind;
        DimensionOperatorKind = dimensionOperatorKind;
        Purpose = purpose;
    }
    
    public DimensionUnit(
        double value,
        DimensionUnitKind dimensionUnitKind)
    {
        Value = value;
        DimensionUnitKind = dimensionUnitKind;
        DimensionOperatorKind = DimensionOperatorKind.Add;
        Purpose = DimensionUnitPurposeKind.None;
    }
    
    public DimensionUnit(
        double value,
        DimensionUnitKind dimensionUnitKind,
        DimensionOperatorKind dimensionOperatorKind,
        DimensionUnitPurposeKind purpose)
    {
        Value = value;
        DimensionUnitKind = dimensionUnitKind;
        DimensionOperatorKind = dimensionOperatorKind;
        Purpose = purpose;
    }
    
    public DimensionUnit(
        double value,
        DimensionUnitKind dimensionUnitKind,
        DimensionOperatorKind dimensionOperatorKind)
    {
        Value = value;
        DimensionUnitKind = dimensionUnitKind;
        DimensionOperatorKind = dimensionOperatorKind;
        Purpose = DimensionUnitPurposeKind.None;
    }

    private double _value;

    public double Value
    {
        get
        {
            return _value;
        }
        set
        {
            IsUsed = true;
            _value = value;
        }
    }
    
    public DimensionUnitKind DimensionUnitKind { get; set; }
    public DimensionOperatorKind DimensionOperatorKind { get; set; } = DimensionOperatorKind.Add;
    public DimensionUnitPurposeKind Purpose { get; set; }
    public bool IsUsed { get; set; } = true;
}
