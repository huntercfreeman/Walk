using Walk.Common.RazorLib.Widgets.Models;

namespace Walk.Common.RazorLib.Menus.Models;

public record MenuOptionRecord
{
    public MenuOptionRecord(
        string displayName,
        MenuOptionKind menuOptionKind,
        Func<Task>? onClickFunc = null,
        MenuRecord? subMenu = null,
        SimpleWidgetKind simpleWidgetKind = SimpleWidgetKind.None,
        Dictionary<string, object?>? widgetParameterMap = null)
    {
        DisplayName = displayName;
        MenuOptionKind = menuOptionKind;
        OnClickFunc = onClickFunc;
        SubMenu = subMenu;
        SimpleWidgetKind = simpleWidgetKind;
        WidgetParameterMap = widgetParameterMap;
    }
    
    public string DisplayName { get; init; }
    public MenuOptionKind MenuOptionKind { get; init; }
    public Func<Task>? OnClickFunc{ get; init; }
    public MenuRecord? SubMenu { get; set; }
    public SimpleWidgetKind SimpleWidgetKind { get; init; }
    public Dictionary<string, object?>? WidgetParameterMap { get; init; }
    public AutocompleteEntryKind IconKind { get; set; }
}
