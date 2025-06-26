using Microsoft.AspNetCore.Components.Web;

namespace Walk.Common.RazorLib.Tooltips.Models;

public sealed class TooltipModel<T> : ITooltipModel
{
    /// <summary>
    /// -1 to left and top inside this, it is kind of odd behavior.
    /// not feeling great, just trying to get something going in terms of tooltip repositioning.
    ///
    /// The -1 maybe will help a smidge with the tooltip immediately disappearing when you try to
    /// move the cursor "onto" the tooltip itself to click hover underline goto.
    /// </summary>
    public TooltipModel(
        Type rendererType,
        Dictionary<string, object?>? parameterMap,
        double x,
        double y,
        string? cssClassString,
        Func<Task> onMouseOver,
        Func<ITooltipModel, WheelEventArgs, Task> onWheel,
        T item)
    {
        RendererType = rendererType;
        ParameterMap = parameterMap;
        X = x - 1;
        Y = y - 1;
        CssClassString = cssClassString;
        OnMouseOver = onMouseOver;
        OnWheel = onWheel;
        Item = item;
    }

    public Type RendererType { get; set; }
    public Dictionary<string, object?>? ParameterMap { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string? CssClassString { get; set; }
    public Func<Task> OnMouseOver { get; set; }
    public Func<ITooltipModel, WheelEventArgs, Task> OnWheel { get; set; }
    public object ItemUntyped => Item;
    public bool WasRepositioned { get; set; }
    
    public T Item { get; set; }
}
