namespace Walk.Common.RazorLib.ComponentRenderers.Models;

public interface ICommonComponentRenderers
{
    public Type ErrorNotificationRendererType { get; }
    public Type InformativeNotificationRendererType { get; }
    public Type ProgressNotificationRendererType { get; }
}
