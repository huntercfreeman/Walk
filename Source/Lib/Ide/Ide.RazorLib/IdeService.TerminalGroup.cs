using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private TerminalGroupState _terminalGroupState = new();

    public TerminalGroupState GetTerminalGroupState() => _terminalGroupState;

    public void TerminalGroup_SetActiveTerminal(Key<ITerminal> terminalKey)
    {
        lock (_stateModificationLock)
        {
            _terminalGroupState = _terminalGroupState with
            {
                ActiveTerminalKey = terminalKey
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalGroupStateChanged);
    }

    public void TerminalGroup_InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
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

        IdeStateChanged?.Invoke(IdeStateChangedKind.TerminalGroupStateChanged);
    }
}
