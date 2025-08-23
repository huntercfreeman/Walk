namespace Walk.Common.RazorLib.FileSystems.Models;

public class FileTemplate : IFileTemplate
{
    public FileTemplate(
        string displayName,
        FileTemplateKind fileTemplateKind,
        string fileExtensionNoPeriod,
        Func<string, List<IFileTemplate>> relatedFileTemplatesFunc,
        bool initialCheckedStateWhenIsRelatedFile,
        Func<FileTemplateParameter, FileTemplateResult> constructFileContents)
    {
        DisplayName = displayName;
        FileTemplateKind = fileTemplateKind;
        FileExtensionNoPeriod = fileExtensionNoPeriod;
        RelatedFileTemplatesFunc = relatedFileTemplatesFunc;
        InitialCheckedStateWhenIsRelatedFile = initialCheckedStateWhenIsRelatedFile;
        ConstructFileContents = constructFileContents;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string DisplayName { get; }
    public FileTemplateKind FileTemplateKind { get; }
    public string FileExtensionNoPeriod { get; }
    public Func<string, List<IFileTemplate>> RelatedFileTemplatesFunc { get; }
    public bool InitialCheckedStateWhenIsRelatedFile { get; }
    public Func<FileTemplateParameter, FileTemplateResult> ConstructFileContents { get; }
}
