using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalFileSystemProvider : IFileSystemProvider
{
    public LocalFileSystemProvider(
        IEnvironmentProvider environmentProvider,
        ICommonComponentRenderers commonComponentRenderers,
        ICommonUiService commonUiService)
    {
        File = new LocalFileHandler(environmentProvider, commonComponentRenderers, commonUiService);
        Directory = new LocalDirectoryHandler(environmentProvider, commonComponentRenderers, commonUiService);
    }

    public IFileHandler File { get; }
    public IDirectoryHandler Directory { get; }
}