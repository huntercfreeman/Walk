using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
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
        WalkHostingInformation hostingInformation)
    {
        var commonConfig = new WalkCommonConfig();

        services
            .AddScoped<BrowserResizeInterop>()
            .AddScoped<CommonService, CommonService>(sp =>
            {
                var commonService = new CommonService(
                    hostingInformation,
                    commonConfig,
                    sp.GetRequiredService<IJSRuntime>());
            
                return commonService;
            });
        
        return services;
    }
}
