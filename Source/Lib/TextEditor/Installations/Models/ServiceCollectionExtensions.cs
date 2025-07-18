using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.FindAlls.Models;
using Walk.TextEditor.RazorLib.Edits.Models;

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
                    _textEditorComponentRenderers,
                    sp.GetRequiredService<IJSRuntime>(),
                    sp.GetRequiredService<CommonUtilityService>(),
            		sp.GetRequiredService<IServiceProvider>());
            });
        
        return services;
    }

    private static readonly WalkTextEditorComponentRenderers _textEditorComponentRenderers = new(
        typeof(TextEditors.Displays.Internals.DiagnosticDisplay));
}