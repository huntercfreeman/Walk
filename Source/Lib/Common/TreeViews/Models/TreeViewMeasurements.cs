namespace Walk.Common.RazorLib.TreeViews.Models;

public record struct TreeViewMeasurements(
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop,
    double ScrollLeft,
    double ScrollTop,
    double ScrollWidth,
    double ScrollHeight);
