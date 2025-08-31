using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.Installations.Models;

namespace Walk.Extensions.Config.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkConfigServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation)
    {
        return services
            .AddWalkExtensionsDotNetServices(hostingInformation);
    }
}
