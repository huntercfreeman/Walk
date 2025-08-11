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
        ValueFunc = null;
    
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
            var localValueFunc = ValueFunc;
            
            if (localValueFunc is null)
                return _value;
            else
                return localValueFunc.Invoke();
        }
        init
        {
            var localValueFunc = ValueFunc;
            
            if (localValueFunc is null)
                _value = value;
            else
                throw new WalkCommonException(
                    $"{nameof(DimensionUnit)} should use the setter for either the property '{nameof(Value)}' or '{nameof(ValueFunc)}', but not both. TODO: change this implementation as it is a bit hacky.");
        }
    }
    
    /// <summary>
    /// <see cref="DimensionUnit"/> should use the setter for either the property
    /// <see cref="Value"/> or '{nameof(ValueFunc)}', but not both.
    ///
    /// TODO: change this implementation as it is a bit hacky...
    ///       ...The reason for this hacky addition was to support dimensions that are dependent on some other state.
    /// </summary>
    public Func<double> ValueFunc { get; }
    
    public DimensionUnitKind DimensionUnitKind { get; }
    public DimensionOperatorKind DimensionOperatorKind { get; } = DimensionOperatorKind.Add;
    public DimensionUnitPurposeKind Purpose { get; }
    public bool IsUsed { get; set; }
}
