using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Namespaces.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class FileTemplateParameter
{
    public FileTemplateParameter(
        string filename,
        NamespacePath parentDirectory,
        IEnvironmentProvider environmentProvider)
    {
        Filename = filename;
        ParentDirectory = parentDirectory;
        EnvironmentProvider = environmentProvider;
    }

    public string Filename { get; }
    public NamespacePath ParentDirectory { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }
}
