namespace Walk.Common.RazorLib.FileSystems.Models;

public partial class InMemoryFileSystemProvider : IFileSystemProvider
{
    private readonly IEnvironmentProvider _environmentProvider;
    /// <summary>
    /// I want the website demo to focus on the text editor.
    /// In order to create the files for the demo I run some very unoptimized code.
    /// I'm going to just manually add the 3 demo files by explicit casting the IFileSystemProvider.
    /// Then I can expose '__Files' so that the website can quickly add these in.
    /// </summary>
    private readonly List<InMemoryFile> _files = new();
    private readonly SemaphoreSlim _modificationSemaphore = new(1, 1);
    private readonly InMemoryFileHandler _file;
    private readonly InMemoryDirectoryHandler _directory;

    public InMemoryFileSystemProvider(CommonService commonService)
    {
        _environmentProvider = commonService.EnvironmentProvider;

        _file = new InMemoryFileHandler(this, commonService);
        _directory = new InMemoryDirectoryHandler(this, commonService);

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
    
    /// <summary>
    /// I want the website demo to focus on the text editor.
    /// In order to create the files for the demo I run some very unoptimized code.
    /// I'm going to just manually add the 3 demo files here by explicit casting the IFileSystemProvider.
    /// </summary>
    public List<InMemoryFile> __Files => _files;
}
