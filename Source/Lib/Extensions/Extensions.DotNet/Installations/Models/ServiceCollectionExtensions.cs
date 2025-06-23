using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.CSharpProjects.Displays;
using Walk.Extensions.DotNet.DotNetSolutions.Displays;
using Walk.Extensions.DotNet.Menus.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.Nugets.Displays;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CompilerServices.Displays;
using Walk.Extensions.DotNet.Commands;

namespace Walk.Extensions.DotNet.Installations.Models;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddWalkExtensionsDotNetServices(
		this IServiceCollection services,
		WalkHostingInformation hostingInformation,
		Func<WalkIdeConfig, WalkIdeConfig>? configure = null)
	{
		return services
			.AddScoped<INugetPackageManagerProvider, NugetPackageManagerProviderAzureSearchUsnc>()
			.AddScoped<DotNetCliOutputParser>()
			.AddScoped<DotNetBackgroundTaskApi>()
			.AddScoped<IDotNetCommandFactory, DotNetCommandFactory>()
			.AddScoped<IDotNetMenuOptionsFactory, DotNetMenuOptionsFactory>()
			.AddScoped<IDotNetComponentRenderers>(_ => _dotNetComponentRenderers);
	}

	private static readonly DotNetComponentRenderers _dotNetComponentRenderers = new(
		typeof(NuGetPackageManager),
		typeof(RemoveCSharpProjectFromSolutionDisplay));
}