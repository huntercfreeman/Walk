using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkTextEditor(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation)
    {
        services.AddWalkCommonServices(hostingInformation);

        services
            .AddScoped<TextEditorService>(sp =>
            {
                return new TextEditorService(
                    sp.GetRequiredService<IJSRuntime>(),
                    sp.GetRequiredService<CommonService>());
            });
        
        return services;
    }
}
