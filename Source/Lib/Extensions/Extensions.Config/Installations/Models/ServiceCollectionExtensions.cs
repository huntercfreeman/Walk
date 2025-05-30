using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.Installations.Models;
using Walk.Extensions.Config.CompilerServices;
using Walk.Extensions.Config.Decorations;
// using Walk.Extensions.Git.Installations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.Config.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkConfigServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkIdeConfig, WalkIdeConfig>? configure = null)
    {
        return services
            .AddWalkExtensionsDotNetServices(hostingInformation, configure)
            // .AddWalkExtensionsGitServices(hostingInformation, configure)
            .AddScoped<ICompilerServiceRegistry, ConfigCompilerServiceRegistry>()
            .AddScoped<IDecorationMapperRegistry, DecorationMapperRegistry>();
    }
}