using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.Installations.Models;

namespace Walk.Extensions.Config.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkConfigServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkIdeConfig, WalkIdeConfig>? configure = null)
    {
        return services
            .AddWalkExtensionsDotNetServices(hostingInformation, configure);
    }
}