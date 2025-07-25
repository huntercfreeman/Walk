namespace Walk.Common.RazorLib.Menus.Models;

public record struct MenuMeasurements(
    double ViewWidth,
    double ViewHeight,
    double BoundingClientRectLeft,
    double BoundingClientRectTop);
