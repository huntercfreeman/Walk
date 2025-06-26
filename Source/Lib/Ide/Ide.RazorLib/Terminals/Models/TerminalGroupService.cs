using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalGroupService : ITerminalGroupService
{
    private readonly object _stateModificationLock = new();

    private TerminalGroupState _terminalGroupState = new();
	
	public event Action? TerminalGroupStateChanged;
	
	public TerminalGroupState GetTerminalGroupState() => _terminalGroupState;

    public void SetActiveTerminal(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            _terminalGroupState = _terminalGroupState with
            {
                ActiveTerminalKey = terminalKey
            };
        }

        TerminalGroupStateChanged?.Invoke();
    }
    
    public void InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
            {
                if (_terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }

                if (_terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
            }
        }

        TerminalGroupStateChanged?.Invoke();
    }
}
