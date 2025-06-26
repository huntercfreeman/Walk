using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Extensions.DotNet.CSharpProjects.Displays;
using Walk.Extensions.DotNet.Menus.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.Nugets.Displays;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.Commands;

namespace Walk.Extensions.DotNet.Installations.Models;

/// <summary>
/// Replicate the following in 'WalkConfigInitializer.razor.cs'
///
/// using Microsoft.AspNetCore.Components;
/// using Walk.Extensions.DotNet.BackgroundTasks.Models;
/// 
/// namespace Walk.Extensions.DotNet.Installations.Displays;
/// 
/// public partial class WalkExtensionsDotNetInitializer : ComponentBase
/// {
///     [Inject]
/// 	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
/// 
///     protected override void OnInitialized()
/// 	{
/// 		DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
/// 		{
/// 			WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnInit,
/// 		});
/// 	}
/// 	
/// 	protected override void OnAfterRender(bool firstRender)
/// 	{
/// 		if (firstRender)
/// 		{
///             DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
///             {
///             	WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnAfterRender
///             });
/// 		}
/// 	}
/// }
/// </summary>
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