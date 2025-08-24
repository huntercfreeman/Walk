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
        WalkHostingInformation hostingInformation,
        Func<WalkIdeConfig, WalkIdeConfig>? configure = null)
    {
        var ideConfig = new WalkIdeConfig();

        if (configure is not null)
            ideConfig = configure.Invoke(ideConfig);

        if (ideConfig.AddWalkTextEditor)
        {
            services.AddWalkTextEditor(hostingInformation, (Func<WalkTextEditorConfig, WalkTextEditorConfig>?)(inTextEditorOptions => (inTextEditorOptions with
            {
                InitialThemeKey = CommonFacts.VisualStudioDarkThemeClone.Key,
                RegisterModelFunc = async (registerModelArgs) =>
                {
                    var standardizedAbsolutePathString = registerModelArgs.CommonService.TextEditor_AbsolutePathStandardize(
                        registerModelArgs.ResourceUri.Value);
                        
                    var standardizedResourceUri = new ResourceUri((string)standardizedAbsolutePathString);
                
                    registerModelArgs = new RegisterModelArgs(
                        registerModelArgs.EditContext,
                        standardizedResourceUri,
                        registerModelArgs.CommonService,
                        registerModelArgs.IdeBackgroundTaskApi)
                    {
                        ShouldBlockUntilBackgroundTaskIsCompleted = registerModelArgs.ShouldBlockUntilBackgroundTaskIsCompleted
                    };

                    await ((IdeService)registerModelArgs.IdeBackgroundTaskApi).Editor_RegisterModelFunc(registerModelArgs);
                },
                TryRegisterViewModelFunc = async (tryRegisterViewModelArgs) =>
                {
                    var standardizedAbsolutePathString = tryRegisterViewModelArgs.CommonService.TextEditor_AbsolutePathStandardize(
                        tryRegisterViewModelArgs.ResourceUri.Value);
                        
                    var standardizedResourceUri = new ResourceUri((string)standardizedAbsolutePathString);
                    
                    tryRegisterViewModelArgs = new TryRegisterViewModelArgs(
                        tryRegisterViewModelArgs.EditContext,
                        tryRegisterViewModelArgs.ViewModelKey,
                        standardizedResourceUri,
                        tryRegisterViewModelArgs.Category,
                        tryRegisterViewModelArgs.ShouldSetFocusToEditor,
                        tryRegisterViewModelArgs.CommonService,
                        tryRegisterViewModelArgs.IdeBackgroundTaskApi);

                    return await ((IdeService)tryRegisterViewModelArgs.IdeBackgroundTaskApi).Editor_TryRegisterViewModelFunc(tryRegisterViewModelArgs);
                },
                TryShowViewModelFunc = (tryShowViewModelArgs) =>
                {
                    return ((IdeService)tryShowViewModelArgs.IdeBackgroundTaskApi).Editor_TryShowViewModelFunc(tryShowViewModelArgs);
                },
            })));
        }
        
        if (hostingInformation.WalkHostingKind == WalkHostingKind.Photino)
            services.AddScoped<IAppDataService, NativeAppDataService>();
        else
            services.AddScoped<IAppDataService, DoNothingAppDataService>();

        services
            .AddScoped<IdeService>(sp =>
            {
                return new IdeService(
                    ideConfig,
                    sp.GetRequiredService<TextEditorService>(),
                    sp);
            });

        return services;
    }
}
