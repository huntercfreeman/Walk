namespace Walk.Common.RazorLib.ComponentRenderers.Models;

public class CommonComponentRenderers : ICommonComponentRenderers
{
    public CommonComponentRenderers(
        Type errorNotificationRendererType,
        Type informativeNotificationRendererType,
		Type progressNotificationRendererType)
    {
        ErrorNotificationRendererType = errorNotificationRendererType;
        InformativeNotificationRendererType = informativeNotificationRendererType;
		ProgressNotificationRendererType = progressNotificationRendererType;
    }

    public Type ErrorNotificationRendererType { get; }
    public Type InformativeNotificationRendererType { get; }
    public Type ProgressNotificationRendererType { get; }
}
