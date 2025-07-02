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

		var continuousQueue = new BackgroundTaskQueue(
            BackgroundTaskFacts.ContinuousQueueKey,
            "Continuous");
            
        var indefiniteQueue = new BackgroundTaskQueue(
            BackgroundTaskFacts.IndefiniteQueueKey,
            "Blocking");
            
        hostingInformation.BackgroundTaskService.SetContinuousQueue(continuousQueue);
        hostingInformation.BackgroundTaskService.SetIndefiniteQueue(indefiniteQueue);
            
        services
			.AddScoped<BackgroundTaskService>(sp => 
            {
				hostingInformation.BackgroundTaskService.SetContinuousWorker(new ContinuousBackgroundTaskWorker(
				    continuousQueue,
					hostingInformation.BackgroundTaskService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				hostingInformation.BackgroundTaskService.SetIndefiniteWorker(new IndefiniteBackgroundTaskWorker(
				    indefiniteQueue,
					hostingInformation.BackgroundTaskService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				return hostingInformation.BackgroundTaskService;
			})
            .AddScoped<ITreeViewService, TreeViewService>()
            .AddScoped<IDragService, DragService>()
            .AddScoped<BrowserResizeInterop>()
            .AddScoped<ICommonUtilityService, CommonUtilityService>(sp => new CommonUtilityService(
                hostingInformation,
                _commonRendererTypes,
                sp.GetRequiredService<BackgroundTaskService>(),
                sp.GetRequiredService<ITreeViewService>(),
                commonConfig,
                sp.GetRequiredService<IJSRuntime>()));
        
        return services;
    }

    private static readonly CommonComponentRenderers _commonRendererTypes = new(
        typeof(CommonErrorNotificationDisplay),
        typeof(CommonInformativeNotificationDisplay),
        typeof(CommonProgressNotificationDisplay));
}