namespace Walk.Common.RazorLib.Menus.Models;

public struct MenuOptionOnClickArgs
{
    public MenuMeasurements MenuMeasurements { get; set; }
    public double TopOffsetOptionFromMenu { get; set; }
    public string MenuHtmlId { get; set; }
}
