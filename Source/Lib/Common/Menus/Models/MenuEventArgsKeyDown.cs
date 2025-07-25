namespace Walk.Common.RazorLib.Menus.Models;

public record struct MenuEventArgsKeyDown(
    string Key,
    string Code,
    bool CtrlKey,
    bool ShiftKey,
    bool AltKey,
    bool MetaKey,
    double ScrollLeft,
    double ScrollTop,
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop);
