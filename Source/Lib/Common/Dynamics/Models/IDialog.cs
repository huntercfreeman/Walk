using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Dynamics.Models;

/// <summary>
/// TODO: Does dragging a dialog around cause the "dialog initializer" to re-render? This would probably be very bad if true.
/// </summary>
public interface IDialog : IDynamicViewModel
{
    public bool DialogIsMinimized { get; set; }
    public bool DialogIsMaximized { get; set; }
    public bool DialogIsResizable { get; set; }
    public string DialogFocusPointHtmlElementId { get; init; }
    public ElementDimensions DialogElementDimensions { get; set; }
    public string? DialogCssClass { get; set; }
    public string? DialogCssStyle { get; set; }

    public IDialog SetDialogIsMaximized(bool isMaximized);
}
