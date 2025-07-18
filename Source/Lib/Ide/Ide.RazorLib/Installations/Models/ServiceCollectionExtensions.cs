using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.FileSystems.Displays;
using Walk.Ide.RazorLib.FormsGenerics.Displays;
using Walk.Ide.RazorLib.CommandBars.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
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

                    await ((IdeBackgroundTaskApi)fastParseArgs.IdeBackgroundTaskApi).Editor_FastParseFunc(fastParseArgs);
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

                    await ((IdeBackgroundTaskApi)registerModelArgs.IdeBackgroundTaskApi).Editor_RegisterModelFunc(registerModelArgs);
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

                    return await ((IdeBackgroundTaskApi)tryRegisterViewModelArgs.IdeBackgroundTaskApi).Editor_TryRegisterViewModelFunc(tryRegisterViewModelArgs);
                },
                TryShowViewModelFunc = (tryShowViewModelArgs) =>
                {
                    return ((IdeBackgroundTaskApi)tryShowViewModelArgs.IdeBackgroundTaskApi).Editor_TryShowViewModelFunc(tryShowViewModelArgs);
                },
            })));
        }
        
        if (hostingInformation.WalkHostingKind == WalkHostingKind.Photino)
        	services.AddScoped<IAppDataService, NativeAppDataService>();
        else
        	services.AddScoped<IAppDataService, DoNothingAppDataService>();

        services
            .AddScoped<IdeBackgroundTaskApi>(sp =>
            {
                return new IdeBackgroundTaskApi(
                    ideConfig,
                    _ideComponentRenderers);
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