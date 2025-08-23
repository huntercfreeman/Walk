namespace Walk.Common.RazorLib.TreeViews.Models;

public record struct TreeViewEventArgsMouseDown(
    long Buttons,
    long Button,
    double X,
    double Y,
    bool ShiftKey,
    double ScrollLeft,
    double ScrollTop,
    double ScrollWidth,
    double ScrollHeight,
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop);
