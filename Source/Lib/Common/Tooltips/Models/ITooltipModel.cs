using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Common.RazorLib.Tooltips.Models;

public interface ITooltipModel
{
	public Type RendererType { get; set; }
    public Dictionary<string, object?>? ParameterMap { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string? CssClassString { get; set; }
    public Func<Task> OnMouseOver { get; set; }
    public Func<ITooltipModel, WheelEventArgs, Task> OnWheel { get; set; }
    public object ItemUntyped { get; }
    public bool WasRepositioned { get; set; }
}
