using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Extensions.Config.Installations.Models;

namespace Walk.Website.RazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkWebsiteServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation)
    {
        services.AddWalkIdeRazorLibServices(hostingInformation);
        services.AddWalkConfigServices(hostingInformation);
        
        return services;
    }
}
