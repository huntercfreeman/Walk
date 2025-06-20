using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Notifications.Displays;
using Walk.Common.RazorLib.TreeViews.Displays.Utils;
using Walk.Common.RazorLib.WatchWindows.Displays;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Outlines.Models;
using Walk.Common.RazorLib.Reflectives.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.Storages.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Tooltips.Models;

namespace Walk.Common.RazorLib.Installations.Models;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The <see cref="configure"/> parameter provides an instance of a record type.
    /// Use the 'with' keyword to change properties and then return the new instance.
    /// </summary>
    public static IServiceCollection AddWalkCommonServices(
        this IServiceCollection services,
        WalkHostingInformation hostingInformation,
        Func<WalkCommonConfig, WalkCommonConfig>? configure = null)
    {
        var commonConfig = new WalkCommonConfig();

        if (configure is not null)
            commonConfig = configure.Invoke(commonConfig);

		var continuousQueue = new BackgroundTaskQueue(
            BackgroundTaskFacts.ContinuousQueueKey,
            "Continuous");
            
        var indefiniteQueue = new BackgroundTaskQueue(
            BackgroundTaskFacts.IndefiniteQueueKey,
            "Blocking");
            
        hostingInformation.BackgroundTaskService.SetContinuousQueue(continuousQueue);
        hostingInformation.BackgroundTaskService.SetIndefiniteQueue(indefiniteQueue);
            
        services
            .AddSingleton(commonConfig)
            .AddSingleton(hostingInformation)
            .AddSingleton<ICommonComponentRenderers>(_ => _commonRendererTypes)
			.AddScoped<BackgroundTaskService>(sp => 
            {
				hostingInformation.BackgroundTaskService.SetContinuousWorker(new ContinuousBackgroundTaskWorker(
				    continuousQueue,
					hostingInformation.BackgroundTaskService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				hostingInformation.BackgroundTaskService.SetIndefiniteWorker(new IndefiniteBackgroundTaskWorker(
				    indefiniteQueue,
					hostingInformation.BackgroundTaskService,
				    sp.GetRequiredService<ILoggerFactory>(),
				    hostingInformation.WalkHostingKind));

				return hostingInformation.BackgroundTaskService;
			})
            .AddScoped<CommonBackgroundTaskApi>()
            .AddScoped<BrowserResizeInterop>()
            .AddScoped<IContextService, ContextService>()
            .AddScoped<IOutlineService, OutlineService>()
            .AddScoped<IPanelService, PanelService>()
            .AddScoped<IAppDimensionService, AppDimensionService>()
            .AddScoped<IKeymapService, KeymapService>()
            .AddScoped<IWidgetService, WidgetService>()
            .AddScoped<IReflectiveService, ReflectiveService>()
            .AddScoped<IClipboardService, JavaScriptInteropClipboardService>()
            .AddScoped<IDialogService, DialogService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddScoped<IDragService, DragService>()
            .AddScoped<IDropdownService, DropdownService>()
            .AddScoped<IAppOptionsService, AppOptionsService>()
            .AddScoped<IStorageService, LocalStorageService>()
            .AddScoped<IThemeService, ThemeService>()
            .AddScoped<ITreeViewService, TreeViewService>()
            .AddScoped<ITooltipService, TooltipService>();

        switch (hostingInformation.WalkHostingKind)
        {
            case WalkHostingKind.Photino:
                services.AddScoped<IEnvironmentProvider, LocalEnvironmentProvider>();
                services.AddScoped<IFileSystemProvider, LocalFileSystemProvider>();
                break;
            default:
                services.AddScoped<IEnvironmentProvider, InMemoryEnvironmentProvider>();
                services.AddScoped<IFileSystemProvider, InMemoryFileSystemProvider>();
                break;
        }
        
        return services;
    }

    private static readonly CommonTreeViews _commonTreeViews = new(
        typeof(TreeViewExceptionDisplay),
        typeof(TreeViewMissingRendererFallbackDisplay),
        typeof(TreeViewTextDisplay),
        typeof(TreeViewReflectionDisplay),
        typeof(TreeViewPropertiesDisplay),
        typeof(TreeViewInterfaceImplementationDisplay),
        typeof(TreeViewFieldsDisplay),
        typeof(TreeViewExceptionDisplay),
        typeof(TreeViewEnumerableDisplay));

    private static readonly CommonComponentRenderers _commonRendererTypes = new(
        typeof(CommonErrorNotificationDisplay),
        typeof(CommonInformativeNotificationDisplay),
        typeof(CommonProgressNotificationDisplay),
        _commonTreeViews);
}