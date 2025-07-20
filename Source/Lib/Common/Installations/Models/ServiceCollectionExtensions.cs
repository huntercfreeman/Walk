using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Notifications.Displays;
using Walk.Common.RazorLib.Dimensions.Models;

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
            .AddScoped<CommonService, CommonService>(sp =>
            {
                var commonService = new CommonService(
                    hostingInformation,
                    commonConfig,
                    sp.GetRequiredService<IJSRuntime>());
            
                commonService.SetContinuousQueue(new BackgroundTaskQueue(
                    CommonFacts.ContinuousQueueKey,
                    "Continuous"));
                commonService.SetIndefiniteQueue(new BackgroundTaskQueue(
                    CommonFacts.IndefiniteQueueKey,
                    "Blocking"));
            
                commonService.SetContinuousWorker(new ContinuousBackgroundTaskWorker(
				    commonService.ContinuousQueue,
					commonService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				commonService.SetIndefiniteWorker(new IndefiniteBackgroundTaskWorker(
				    commonService.IndefiniteQueue,
					commonService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));
            
                return commonService;
            });
        
        return services;
    }
}
