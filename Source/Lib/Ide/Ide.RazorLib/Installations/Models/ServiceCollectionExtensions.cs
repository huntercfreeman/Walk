using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Ide.RazorLib.StartupControls.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.FileSystems.Displays;
using Walk.Ide.RazorLib.FormsGenerics.Displays;
using Walk.Ide.RazorLib.Commands;
using Walk.Ide.RazorLib.CommandBars.Models;
// FindAllReferences
// using Walk.Ide.RazorLib.FindAllReferences.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.Namespaces.Displays;
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
            services.AddWalkTextEditor(hostingInformation, inTextEditorOptions => inTextEditorOptions with
            {
                CustomThemeRecordList = WalkTextEditorCustomThemeFacts.AllCustomThemesList,
                InitialThemeKey = ThemeFacts.VisualStudioDarkThemeClone.Key,
                AbsolutePathStandardizeFunc = AbsolutePathStandardizeFunc,
                FastParseFunc = async (fastParseArgs) =>
                {
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		fastParseArgs.ResourceUri.Value, fastParseArgs.ServiceProvider);
                		
                	var standardizedResourceUri = new ResourceUri(standardizedAbsolutePathString);
                
                    fastParseArgs = new FastParseArgs(
                        standardizedResourceUri,
                        fastParseArgs.ExtensionNoPeriod,
                        fastParseArgs.ServiceProvider)
                    {
                    	ShouldBlockUntilBackgroundTaskIsCompleted = fastParseArgs.ShouldBlockUntilBackgroundTaskIsCompleted
                    };

                    var ideBackgroundTaskApi = fastParseArgs.ServiceProvider.GetRequiredService<IdeBackgroundTaskApi>();
                    await ideBackgroundTaskApi.Editor_FastParseFunc(fastParseArgs);
                },
                RegisterModelFunc = async (registerModelArgs) =>
                {
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		registerModelArgs.ResourceUri.Value, registerModelArgs.ServiceProvider);
                		
                	var standardizedResourceUri = new ResourceUri(standardizedAbsolutePathString);
                
                    registerModelArgs = new RegisterModelArgs(
                    	registerModelArgs.EditContext,
                        standardizedResourceUri,
                        registerModelArgs.ServiceProvider)
                    {
                    	ShouldBlockUntilBackgroundTaskIsCompleted = registerModelArgs.ShouldBlockUntilBackgroundTaskIsCompleted
                    };

                    var ideBackgroundTaskApi = registerModelArgs.ServiceProvider.GetRequiredService<IdeBackgroundTaskApi>();
                    await ideBackgroundTaskApi.Editor_RegisterModelFunc(registerModelArgs);
                },
                TryRegisterViewModelFunc = async (tryRegisterViewModelArgs) =>
                {
                	var standardizedAbsolutePathString = await AbsolutePathStandardizeFunc(
                		tryRegisterViewModelArgs.ResourceUri.Value, tryRegisterViewModelArgs.ServiceProvider);
                		
                	var standardizedResourceUri = new ResourceUri(standardizedAbsolutePathString);
                	
                    tryRegisterViewModelArgs = new TryRegisterViewModelArgs(
                    	tryRegisterViewModelArgs.EditContext,
                        tryRegisterViewModelArgs.ViewModelKey,
                        standardizedResourceUri,
                        tryRegisterViewModelArgs.Category,
                        tryRegisterViewModelArgs.ShouldSetFocusToEditor,
                        tryRegisterViewModelArgs.ServiceProvider);

                    var ideBackgroundTaskApi = tryRegisterViewModelArgs.ServiceProvider.GetRequiredService<IdeBackgroundTaskApi>();
                    return await ideBackgroundTaskApi.Editor_TryRegisterViewModelFunc(tryRegisterViewModelArgs);
                },
                TryShowViewModelFunc = (tryShowViewModelArgs) =>
                {
                    var ideBackgroundTaskApi = tryShowViewModelArgs.ServiceProvider.GetRequiredService<IdeBackgroundTaskApi>();
                    return ideBackgroundTaskApi.Editor_TryShowViewModelFunc(tryShowViewModelArgs);
                },
            });
        }
        
        if (hostingInformation.WalkHostingKind == WalkHostingKind.Photino)
        	services.AddScoped<IAppDataService, NativeAppDataService>();
        else
        	services.AddScoped<IAppDataService, DoNothingAppDataService>();

        services
            .AddSingleton(ideConfig)
            .AddSingleton<IIdeComponentRenderers>(_ideComponentRenderers)
            .AddScoped<IdeBackgroundTaskApi>()
            .AddScoped<ICommandFactory, CommandFactory>()
            .AddScoped<IMenuOptionsFactory, MenuOptionsFactory>()
            .AddScoped<IFileTemplateProvider, FileTemplateProvider>()
            .AddScoped<ICodeSearchService, CodeSearchService>()
            .AddScoped<IInputFileService, InputFileService>()
            .AddScoped<IStartupControlService, StartupControlService>()
            .AddScoped<ITerminalService, TerminalService>()
            .AddScoped<ITerminalGroupService, TerminalGroupService>()
            .AddScoped<IFolderExplorerService, FolderExplorerService>()
            .AddScoped<IIdeMainLayoutService, IdeMainLayoutService>()
            .AddScoped<IIdeHeaderService, IdeHeaderService>()
            .AddScoped<ICommandBarService, CommandBarService>();
            // FindAllReferences
            // .AddScoped<IFindAllReferencesService, FindAllReferencesService>();

        return services;
    }
    
    public static Task<string> AbsolutePathStandardizeFunc(string absolutePathString, IServiceProvider serviceProvider)
    {
        var environmentProvider = serviceProvider.GetRequiredService<IEnvironmentProvider>();

        if (absolutePathString.StartsWith(environmentProvider.DriveExecutingFromNoDirectorySeparator))
        {
            var removeDriveFromResourceUriValue = absolutePathString[
                environmentProvider.DriveExecutingFromNoDirectorySeparator.Length..];

            return Task.FromResult(removeDriveFromResourceUriValue);
        }

        return Task.FromResult(absolutePathString);
    }

    private static readonly IdeTreeViews _ideTreeViews = new(
        typeof(TreeViewNamespacePathDisplay),
        typeof(TreeViewAbsolutePathDisplay));

    private static readonly IdeComponentRenderers _ideComponentRenderers = new(
        typeof(BooleanPromptOrCancelDisplay),
        typeof(FileFormDisplay),
        typeof(DeleteFileFormDisplay),
        typeof(InputFileDisplay),
        _ideTreeViews);
}