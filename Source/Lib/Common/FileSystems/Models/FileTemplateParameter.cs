namespace Walk.Common.RazorLib.FileSystems.Models;

public class FileTemplateParameter
{
    public FileTemplateParameter(
        string filename,
        AbsolutePath parentDirectory,
        string parentDirectoryNamespace,
        IEnvironmentProvider environmentProvider)
    {
        Filename = filename;
        ParentDirectory = parentDirectory;
        ParentDirectoryNamespace = parentDirectoryNamespace;
        EnvironmentProvider = environmentProvider;
    }

    public string Filename { get; }
    public AbsolutePath ParentDirectory { get; }
    public string ParentDirectoryNamespace { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }
}
