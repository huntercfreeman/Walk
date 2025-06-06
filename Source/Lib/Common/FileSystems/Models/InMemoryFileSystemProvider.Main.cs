using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public partial class InMemoryFileSystemProvider : IFileSystemProvider
{
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly List<InMemoryFile> _files = new();
    private readonly SemaphoreSlim _modificationSemaphore = new(1, 1);
    private readonly InMemoryFileHandler _file;
    private readonly InMemoryDirectoryHandler _directory;

    public InMemoryFileSystemProvider(
        IEnvironmentProvider environmentProvider,
        ICommonComponentRenderers commonComponentRenderers,
        INotificationService notificationService)
    {
        _environmentProvider = environmentProvider;

        _file = new InMemoryFileHandler(this, _environmentProvider, commonComponentRenderers, notificationService);
        _directory = new InMemoryDirectoryHandler(this, _environmentProvider, commonComponentRenderers, notificationService);

        Directory
            .CreateDirectoryAsync(_environmentProvider.RootDirectoryAbsolutePath.Value)
            .Wait();

        Directory
            .CreateDirectoryAsync(_environmentProvider.HomeDirectoryAbsolutePath.Value)
            .Wait();
    }

    public IReadOnlyList<InMemoryFile> Files => _files;

    public IFileHandler File => _file;
    public IDirectoryHandler Directory => _directory;
}