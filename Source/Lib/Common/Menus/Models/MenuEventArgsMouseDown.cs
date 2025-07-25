namespace Walk.Common.RazorLib.Menus.Models;

public record struct MenuEventArgsMouseDown(
    long Buttons,
    long Button,
    double X,
    double Y,
    bool ShiftKey,
    double ScrollLeft,
    double ScrollTop,
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop);
