namespace Walk.Common.RazorLib.TreeViews.Models;

public record struct TreeViewEventArgsMouseDown(
    long Buttons,
    double X,
    double Y,
    bool ShiftKey,
    double ScrollLeft,
    double ScrollTop,
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop);
