namespace Walk.Common.RazorLib.FileSystems.Models;

public class FileTemplate : IFileTemplate
{
    public FileTemplate(
        string displayName,
        FileTemplateKind fileTemplateKind,
        Func<string, bool> isExactTemplate,
        Func<string, List<IFileTemplate>> relatedFileTemplatesFunc,
        bool initialCheckedStateWhenIsRelatedFile,
        Func<FileTemplateParameter, FileTemplateResult> constructFileContents)
    {
        DisplayName = displayName;
        FileTemplateKind = fileTemplateKind;
        IsExactTemplate = isExactTemplate;
        RelatedFileTemplatesFunc = relatedFileTemplatesFunc;
        InitialCheckedStateWhenIsRelatedFile = initialCheckedStateWhenIsRelatedFile;
        ConstructFileContents = constructFileContents;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string DisplayName { get; }
    public FileTemplateKind FileTemplateKind { get; }
    public Func<string, bool> IsExactTemplate { get; }
    public Func<string, List<IFileTemplate>> RelatedFileTemplatesFunc { get; }
    public bool InitialCheckedStateWhenIsRelatedFile { get; }
    public Func<FileTemplateParameter, FileTemplateResult> ConstructFileContents { get; }
}
