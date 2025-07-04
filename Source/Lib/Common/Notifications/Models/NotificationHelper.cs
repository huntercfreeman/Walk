using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Notifications.Models;

public static class NotificationHelper
{
    public static void DispatchInformative(
        string title,
        string message,
        CommonUtilityService commonUtilityService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationInformative = new NotificationViewModel(
            Key<IDynamicViewModel>.NewKey(),
            title,
            commonUtilityService.CommonComponentRenderers.InformativeNotificationRendererType,
            new Dictionary<string, object?>
            {
                {
                    nameof(IInformativeNotificationRendererType.Message),
                    message
                },
            },
            notificationOverlayLifespan,
            true,
            null);

        commonUtilityService.Notification_ReduceRegisterAction(notificationInformative);
    }

    public static void DispatchError(
        string title,
        string message,
        CommonUtilityService commonUtilityService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationError = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonUtilityService.CommonComponentRenderers.ErrorNotificationRendererType,
            new Dictionary<string, object?>
            {
                { nameof(IErrorNotificationRendererType.Message), $"ERROR: {message}" },
            },
            notificationOverlayLifespan,
            true,
            IErrorNotificationRendererType.CSS_CLASS_STRING);

        commonUtilityService.Notification_ReduceRegisterAction(notificationError);
    }

    public static void DispatchProgress(
        string title,
        ProgressBarModel progressBarModel,
        CommonUtilityService commonUtilityService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationProgress = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonUtilityService.CommonComponentRenderers.ProgressNotificationRendererType,
            new Dictionary<string, object?>
            {
                {
					nameof(IProgressNotificationRendererType.ProgressBarModel),
					progressBarModel
				},
            },
            notificationOverlayLifespan,
            true,
            null);

        commonUtilityService.Notification_ReduceRegisterAction(notificationProgress);
    }

    /// <summary>
    /// TODO: Finish the netcoredbg implementation. For now I'm going to do...
	/// ...a good ol' "Console.WriteLine()" style of debugging with this method...
	/// ...Then, when the debugger is implemented, I can find all references of this...
	/// ...method and remove it from existence.
	/// <br/>
	/// This method takes in a 'Func<string>' as opposed to a 'string' in order to
	/// ensure I encapsulate all of my debug message logic in the invocation of
	/// this method itself, since this method is only to exist short term.
    /// </summary>
	public static void DispatchDebugMessage(
        string title,
        Func<string> messageFunc,
        CommonUtilityService commonUtilityService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationError = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonUtilityService.CommonComponentRenderers.ErrorNotificationRendererType,
            new Dictionary<string, object?>
            {
                { nameof(IErrorNotificationRendererType.Message), $"DEBUG: {messageFunc.Invoke()}" },
            },
            notificationOverlayLifespan,
            true,
            IErrorNotificationRendererType.CSS_CLASS_STRING);

        commonUtilityService.Notification_ReduceRegisterAction(notificationError);
    }
}