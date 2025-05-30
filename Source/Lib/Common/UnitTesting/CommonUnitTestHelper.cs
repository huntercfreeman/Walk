using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Exceptions;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Storages.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Walk.Common.RazorLib.UnitTesting;

public class CommonUnitTestHelper
{
    /// <summary>
    /// To create an instance of <see cref="CommonUnitTestHelper"/>,
    /// one should invoke <see cref="AddWalkCommonServicesUnitTesting(IServiceCollection, WalkHostingInformation)"/>,
    /// then build the <see cref="IServiceProvider"/>, and provide the built serviceProvider to this constructor.
    /// </summary>
    public CommonUnitTestHelper(IServiceProvider serviceProvider)
    {
        EnvironmentProvider = serviceProvider.GetRequiredService<IEnvironmentProvider>();
        FileSystemProvider = serviceProvider.GetRequiredService<IFileSystemProvider>();
        AppOptionsService = serviceProvider.GetRequiredService<IAppOptionsService>();
        DialogService = serviceProvider.GetRequiredService<IDialogService>();
        StorageService = serviceProvider.GetRequiredService<IStorageService>();
        DragService = serviceProvider.GetRequiredService<IDragService>();
        DropdownService = serviceProvider.GetRequiredService<IDropdownService>();
        ClipboardService = serviceProvider.GetRequiredService<IClipboardService>();
        NotificationService = serviceProvider.GetRequiredService<INotificationService>();
        ThemeService = serviceProvider.GetRequiredService<IThemeService>();
        TreeViewService = serviceProvider.GetRequiredService<ITreeViewService>();

        ValidateDependencies();
    }

    /// <summary>
    /// Implementation is intended to be <see cref="InMemoryEnvironmentProvider"/>
    /// </summary>
    public IEnvironmentProvider EnvironmentProvider { get; }
    /// <summary>
    /// Implementation is intended to be <see cref="InMemoryFileSystemProvider"/>
    /// </summary>
    public IFileSystemProvider FileSystemProvider { get; }
    public IAppOptionsService AppOptionsService { get; }
    public IDialogService DialogService { get; }
    public IStorageService StorageService { get; }
    public IDragService DragService { get; }
    public IDropdownService DropdownService { get; }
    /// <summary>
    /// Implementation is intended to be <see cref="InMemoryClipboardService"/>
    /// </summary>
    public IClipboardService ClipboardService { get; }
    public INotificationService NotificationService { get; }
    public IThemeService ThemeService { get; }
    public ITreeViewService TreeViewService { get; }

    /// <summary>This method is not an extension method due to its niche nature.</summary>
    public static IServiceCollection AddWalkCommonServicesUnitTesting(
        IServiceCollection services,
        WalkHostingInformation hostingInformation)
    {
        services.AddWalkCommonServices(hostingInformation);
        
        return services
        	.AddScoped<IClipboardService, InMemoryClipboardService>()
        	.AddScoped<IStorageService, DoNothingStorageService>();
    }

    private void ValidateDependencies()
    {
        if (EnvironmentProvider is not InMemoryEnvironmentProvider)
            ThrowInvalidInterfaceException(typeof(IEnvironmentProvider), typeof(InMemoryEnvironmentProvider));

        if (FileSystemProvider is not InMemoryFileSystemProvider)
            ThrowInvalidInterfaceException(typeof(IFileSystemProvider), typeof(InMemoryFileSystemProvider));

        if (ClipboardService is not InMemoryClipboardService)
            ThrowInvalidInterfaceException(typeof(IClipboardService), typeof(InMemoryClipboardService));

        if (StorageService is not DoNothingStorageService)
            ThrowInvalidInterfaceException(typeof(IStorageService), typeof(DoNothingStorageService));
    }

    private void ThrowInvalidInterfaceException(Type interfaceType, Type concreteType)
    {
        throw new WalkCommonException(
            $"The current implementation of {nameof(IStorageService)}" +
            $" is NOT {nameof(DoNothingStorageService)}." +
            $" To avoid side effects in unit tests," +
            $" change the implementation of {nameof(IStorageService)}" +
            $" to {nameof(DoNothingStorageService)}");
    }
}
