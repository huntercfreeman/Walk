using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Notifications.Displays;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Tooltips.Models;

namespace Walk.Common.RazorLib.Installations.Models;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The <see cref="configure"/> parameter provides an instance of a record type.
    /// Use the 'with' keyword to change properties and then return the new instance.
    /// </summary>
    public static IServiceCollection AddWalkCommonServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkCommonConfig, WalkCommonConfig>? configure = null)
    {
        var commonConfig = new WalkCommonConfig();

        if (configure is not null)
            commonConfig = configure.Invoke(commonConfig);

        services
            .AddScoped<BrowserResizeInterop>()
            .AddScoped<CommonUtilityService, CommonUtilityService>(sp =>
            {
                var commonUtilityService = new CommonUtilityService(
                    hostingInformation,
                    _commonRendererTypes,
                    commonConfig,
                    sp.GetRequiredService<IJSRuntime>());
            
                commonUtilityService.SetContinuousQueue(new BackgroundTaskQueue(
                    BackgroundTaskFacts.ContinuousQueueKey,
                    "Continuous"));
                commonUtilityService.SetIndefiniteQueue(new BackgroundTaskQueue(
                    BackgroundTaskFacts.IndefiniteQueueKey,
                    "Blocking"));
            
                commonUtilityService.SetContinuousWorker(new ContinuousBackgroundTaskWorker(
				    commonUtilityService.ContinuousQueue,
					commonUtilityService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				commonUtilityService.SetIndefiniteWorker(new IndefiniteBackgroundTaskWorker(
				    commonUtilityService.IndefiniteQueue,
					commonUtilityService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));
            
                return commonUtilityService;
            });
        
        return services;
    }

    private static readonly CommonComponentRenderers _commonRendererTypes = new(
        typeof(CommonErrorNotificationDisplay),
        typeof(CommonInformativeNotificationDisplay),
        typeof(CommonProgressNotificationDisplay));
}