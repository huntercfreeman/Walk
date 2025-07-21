using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalkTextEditor(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkTextEditorConfig, WalkTextEditorConfig>? configure = null)
    {
        var textEditorConfig = new WalkTextEditorConfig();

        if (configure is not null)
            textEditorConfig = configure.Invoke(textEditorConfig);

        if (textEditorConfig.AddWalkCommon)
            services.AddWalkCommonServices(hostingInformation);

        services
            .AddScoped<TextEditorService>(sp =>
            {
                return new TextEditorService(
                    textEditorConfig,
                    sp.GetRequiredService<IJSRuntime>(),
                    sp.GetRequiredService<CommonService>(),
                    sp.GetRequiredService<IServiceProvider>());
            });
        
        return services;
    }
}
