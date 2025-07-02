using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalFileSystemProvider : IFileSystemProvider
{
    public LocalFileSystemProvider(
        IEnvironmentProvider environmentProvider,
        ICommonComponentRenderers commonComponentRenderers,
        ICommonUtilityService commonUtilityService)
    {
        File = new LocalFileHandler(environmentProvider, commonComponentRenderers, commonUtilityService);
        Directory = new LocalDirectoryHandler(environmentProvider, commonComponentRenderers, commonUtilityService);
    }

    public IFileHandler File { get; }
    public IDirectoryHandler Directory { get; }
}