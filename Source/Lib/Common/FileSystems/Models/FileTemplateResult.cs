namespace Walk.Common.RazorLib.FileSystems.Models;

public class FileTemplateResult
{
    public FileTemplateResult(AbsolutePath fileAbsolutePath, string fileAbsolutePathNamespace, string contents)
    {
        FileAbsolutePath = fileAbsolutePath;
        FileAbsolutePathNamespace = fileAbsolutePathNamespace;
        Contents = contents;
    }

    public AbsolutePath FileAbsolutePath { get; }
    public string FileAbsolutePathNamespace { get; }
    public string Contents { get; }
}
