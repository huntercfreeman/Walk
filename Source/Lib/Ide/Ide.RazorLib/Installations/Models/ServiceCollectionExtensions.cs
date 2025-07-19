using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.FileSystems.Displays;
using Walk.Ide.RazorLib.FormsGenerics.Displays;
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
                CustomThemeRecordList = WalkTextEditorCustomThemeFacts.AllCustomThemesList,
                InitialThemeKey = ThemeFacts.VisualStudioDarkThemeClone.Key,
                AbsolutePathStandardizeFunc = ServiceCollectionExtensions.AbsolutePathStandardizeFunc,
                FastParseFunc = async (fastParseArgs) =>
                {    
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		fastParseArgs.ResourceUri.Value,
                		fastParseArgs.CommonUtilityService);
                		
                	var standardizedResourceUri = new ResourceUri((string)standardizedAbsolutePathString);
                
                    fastParseArgs = new FastParseArgs(
                        standardizedResourceUri,
                        fastParseArgs.ExtensionNoPeriod,
                        fastParseArgs.CommonUtilityService,
                        fastParseArgs.IdeBackgroundTaskApi)
                    {
                    	ShouldBlockUntilBackgroundTaskIsCompleted = fastParseArgs.ShouldBlockUntilBackgroundTaskIsCompleted
                    };

                    await ((IdeService)fastParseArgs.IdeBackgroundTaskApi).Editor_FastParseFunc(fastParseArgs);
                },
                RegisterModelFunc = async (registerModelArgs) =>
                {
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		registerModelArgs.ResourceUri.Value,
                		registerModelArgs.CommonUtilityService);
                		
                	var standardizedResourceUri = new ResourceUri((string)standardizedAbsolutePathString);
                
                    registerModelArgs = new RegisterModelArgs(
                    	registerModelArgs.EditContext,
                        standardizedResourceUri,
                        registerModelArgs.CommonUtilityService,
                        registerModelArgs.IdeBackgroundTaskApi)
                    {
                    	ShouldBlockUntilBackgroundTaskIsCompleted = registerModelArgs.ShouldBlockUntilBackgroundTaskIsCompleted
                    };

                    await ((IdeService)registerModelArgs.IdeBackgroundTaskApi).Editor_RegisterModelFunc(registerModelArgs);
                },
                TryRegisterViewModelFunc = async (tryRegisterViewModelArgs) =>
                {
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		tryRegisterViewModelArgs.ResourceUri.Value,
                		tryRegisterViewModelArgs.CommonUtilityService);
                		
                	var standardizedResourceUri = new ResourceUri((string)standardizedAbsolutePathString);
                	
                    tryRegisterViewModelArgs = new TryRegisterViewModelArgs(
                    	tryRegisterViewModelArgs.EditContext,
                        tryRegisterViewModelArgs.ViewModelKey,
                        standardizedResourceUri,
                        tryRegisterViewModelArgs.Category,
                        tryRegisterViewModelArgs.ShouldSetFocusToEditor,
                        tryRegisterViewModelArgs.CommonUtilityService,
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
                    _ideComponentRenderers,
                    sp.GetRequiredService<TextEditorService>(),
                    sp);
            });

        return services;
    }
    
    public static Task<string> AbsolutePathStandardizeFunc(string absolutePathString, CommonUtilityService commonUtilityService)
    {
        if (absolutePathString.StartsWith(commonUtilityService.EnvironmentProvider.DriveExecutingFromNoDirectorySeparator))
        {
            var removeDriveFromResourceUriValue = absolutePathString[
                commonUtilityService.EnvironmentProvider.DriveExecutingFromNoDirectorySeparator.Length..];

            return Task.FromResult(removeDriveFromResourceUriValue);
        }

        return Task.FromResult(absolutePathString);
    }

    private static readonly IdeComponentRenderers _ideComponentRenderers = new(
        typeof(BooleanPromptOrCancelDisplay),
        typeof(FileFormDisplay),
        typeof(DeleteFileFormDisplay),
        typeof(InputFileDisplay));
}