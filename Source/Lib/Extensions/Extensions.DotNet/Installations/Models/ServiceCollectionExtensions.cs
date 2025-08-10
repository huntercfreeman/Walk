using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.AppDatas.Models;
using Walk.Ide.RazorLib.Installations.Models;

namespace Walk.Extensions.DotNet.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkExtensionsDotNetServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkIdeConfig, WalkIdeConfig>? configure = null)
    {
        return services
            .AddScoped<DotNetService>(sp =>
            {
                return new DotNetService(
                    sp.GetRequiredService<IdeService>(),
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IAppDataService>(),
                    sp);
            });
    }
}
