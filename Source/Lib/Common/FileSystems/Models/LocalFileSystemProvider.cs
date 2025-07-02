using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalFileSystemProvider : IFileSystemProvider
{
    public LocalFileSystemProvider(ICommonUtilityService commonUtilityService)
    {
        File = new LocalFileHandler(commonUtilityService);
        Directory = new LocalDirectoryHandler(commonUtilityService);
    }

    public IFileHandler File { get; }
    public IDirectoryHandler Directory { get; }
}