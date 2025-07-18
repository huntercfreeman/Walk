using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Reactives.Models;

namespace Walk.Common.RazorLib.Notifications.Models;

public static class NotificationHelper
{
    public static void DispatchInformative(
        string title,
        string message,
        CommonService commonService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationInformative = new NotificationViewModel(
            Key<IDynamicViewModel>.NewKey(),
            title,
            commonService.CommonComponentRenderers.InformativeNotificationRendererType,
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

        commonService.Notification_ReduceRegisterAction(notificationInformative);
    }

    public static void DispatchError(
        string title,
        string message,
        CommonService commonService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationError = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonService.CommonComponentRenderers.ErrorNotificationRendererType,
            new Dictionary<string, object?>
            {
                { nameof(IErrorNotificationRendererType.Message), $"ERROR: {message}" },
            },
            notificationOverlayLifespan,
            true,
            IErrorNotificationRendererType.CSS_CLASS_STRING);

        commonService.Notification_ReduceRegisterAction(notificationError);
    }

    public static void DispatchProgress(
        string title,
        ProgressBarModel progressBarModel,
        CommonService commonService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationProgress = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonService.CommonComponentRenderers.ProgressNotificationRendererType,
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

        commonService.Notification_ReduceRegisterAction(notificationProgress);
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
        CommonService commonService,
        TimeSpan? notificationOverlayLifespan)
    {
        var notificationError = new NotificationViewModel(Key<IDynamicViewModel>.NewKey(),
            title,
            commonService.CommonComponentRenderers.ErrorNotificationRendererType,
            new Dictionary<string, object?>
            {
                { nameof(IErrorNotificationRendererType.Message), $"DEBUG: {messageFunc.Invoke()}" },
            },
            notificationOverlayLifespan,
            true,
            IErrorNotificationRendererType.CSS_CLASS_STRING);

        commonService.Notification_ReduceRegisterAction(notificationError);
    }
}