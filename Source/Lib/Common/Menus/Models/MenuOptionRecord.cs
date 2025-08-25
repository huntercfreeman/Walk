using Walk.Common.RazorLib.Widgets.Models;

namespace Walk.Common.RazorLib.Menus.Models;

public struct MenuOptionRecord
{
    public MenuOptionRecord(
        string displayName,
        MenuOptionKind menuOptionKind,
        Func<Task>? onClickFunc = null)
    {
        DisplayName = displayName;
        MenuOptionKind = menuOptionKind;
        OnClickFunc = onClickFunc;
    }
    
    public string DisplayName { get; init; }
    public MenuOptionKind MenuOptionKind { get; init; }
    public Func<Task>? OnClickFunc{ get; init; }
    public AutocompleteEntryKind IconKind { get; set; }
}
