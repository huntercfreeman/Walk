using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Displays;

namespace Walk.Common.RazorLib.Menus.Models;

/// <summary>
/// Once the 'MenuOptionList' is exposed publically,
/// it should NOT be modified.
/// Make a shallow copy 'new List<MenuOptionRecord>(menuRecord.MenuOptionList);'
/// and modify the shallow copy if modification of the list
/// after exposing it publically is necessary.
/// </summary>
public record MenuRecord(IReadOnlyList<MenuOptionRecord> MenuOptionList)
{
    public static readonly IReadOnlyList<MenuOptionRecord> NoMenuOptionsExistList = new List<MenuOptionRecord>
    {
        new("No menu options exist for this item.", MenuOptionKind.Other)
    };
    
    public int InitialActiveMenuOptionRecordIndex { get; set; } = -1;
    public bool ShouldImmediatelyTakeFocus { get; set; } = true;
    public bool UseIcons { get; set; }
    public string? ElementIdToRestoreFocusToOnClose { get; set; } = CommonFacts.RootHtmlElementId;
    
    public static void OpenSubMenu(
        CommonService commonService,
        MenuRecord subMenu,
        MenuMeasurements menuMeasurements,
        double topOffsetOptionFromMenu,
        string elementIdToRestoreFocusToOnClose)
    {
        subMenu.ElementIdToRestoreFocusToOnClose = elementIdToRestoreFocusToOnClose;
        
        var submenuDropdown = new DropdownRecord(
            Key<DropdownRecord>.NewKey(),
            leftInitial: menuMeasurements.BoundingClientRectLeft + menuMeasurements.ViewWidth,
            topInitial: menuMeasurements.BoundingClientRectTop + topOffsetOptionFromMenu,
            typeof(MenuDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(MenuDisplay.Menu),
                    subMenu
                }
            },
            restoreFocusOnClose: null);
        commonService.Dropdown_ReduceRegisterAction(submenuDropdown);
    }
}
