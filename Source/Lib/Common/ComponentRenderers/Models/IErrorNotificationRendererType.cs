namespace Walk.Common.RazorLib.ComponentRenderers.Models;

public interface IErrorNotificationRendererType
{
    public const string CSS_CLASS_STRING = "di_error";
    public string Message { get; set; }
}
