using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalFileSystemProvider : IFileSystemProvider
{
    public LocalFileSystemProvider(CommonService commonService)
    {
        File = new LocalFileHandler(commonService);
        Directory = new LocalDirectoryHandler(commonService);
    }

    public IFileHandler File { get; }
    public IDirectoryHandler Directory { get; }
}