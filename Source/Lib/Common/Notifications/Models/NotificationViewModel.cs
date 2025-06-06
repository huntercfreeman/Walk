using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Notifications.Models;

public class NotificationViewModel : INotification
{
	public NotificationViewModel(
		Key<IDynamicViewModel> dynamicViewModelKey,
        string title,
        Type componentType,
        Dictionary<string, object?>? componentParameterMap,
        TimeSpan? notificationOverlayLifespan,
        bool deleteNotificationAfterOverlayIsDismissed,
        string? cssClassString)
	{
		DynamicViewModelKey = dynamicViewModelKey;
		Title = title;
        ComponentType = componentType;
        ComponentParameterMap = componentParameterMap;
		NotificationOverlayLifespan = notificationOverlayLifespan;
		DeleteNotificationAfterOverlayIsDismissed = deleteNotificationAfterOverlayIsDismissed;
		CssClassString = cssClassString;
	}
	
	public Key<IDynamicViewModel> DynamicViewModelKey { get; }
	public TimeSpan? NotificationOverlayLifespan { get; init; }
    public bool DeleteNotificationAfterOverlayIsDismissed { get; init; }
	public string? CssClassString { get; init; }
	public string Title { get; init; }
	public string TitleVerbose => Title;
    public Type ComponentType { get; init; }
    public Dictionary<string, object?>? ComponentParameterMap { get; init; }
    public string? CssClass { get; set; }
    public string? CssStyle { get; set; }
	public ElementDimensions ElementDimensions { get; set; } = new();
	public string? NotificationCssClass { get; set; }
	public string? NotificationCssStyle { get; set; }
	public string? SetFocusOnCloseElementId { get; set; }
}
