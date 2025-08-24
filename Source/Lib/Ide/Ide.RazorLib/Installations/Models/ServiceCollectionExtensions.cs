using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.AppDatas.Models;

namespace Walk.Ide.RazorLib.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkIdeRazorLibServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation)
    {
        services.AddWalkTextEditor(hostingInformation);
    
        if (hostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            services.AddScoped<IAppDataService, NativeAppDataService>();
        else
            services.AddScoped<IAppDataService, DoNothingAppDataService>();

        services
            .AddScoped<IdeService>(sp =>
            {
                return new IdeService(
                    sp.GetRequiredService<TextEditorService>(),
                    sp);
            });

        return services;
    }
}
