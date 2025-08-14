using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Common.RazorLib.FileSystems.Displays;

public partial class FileTemplatesDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public string FileName { get; set; } = null!;

    private List<FileTemplatesFormWrapper> _fileTemplatesFormWrappersList = new();
    private List<FileTemplatesFormWrapper> _relatedMatchWrappersList = new();
    private FileTemplatesFormWrapper? _exactMatchWrapper;
    public IFileTemplate? ExactMatchFileTemplate => _exactMatchWrapper?.FileTemplate;

    public List<IFileTemplate>? RelatedMatchFileTemplates => _relatedMatchWrappersList
        .Where(x => x.IsChecked)
        .Select(x => x.FileTemplate)
        .ToList();

    protected override void OnInitialized()
    {
        _fileTemplatesFormWrappersList = CommonService.FileTemplatesList
            .Select(x => new FileTemplatesFormWrapper(x, true))
            .ToList();
    }

    private class FileTemplatesFormWrapper
    {
        public FileTemplatesFormWrapper(IFileTemplate fileTemplate, bool isChecked)
        {
            FileTemplate = fileTemplate;
            IsChecked = isChecked;
        }

        public IFileTemplate FileTemplate { get; }
        public bool IsChecked { get; set; }
    }

    private void GetRelatedFileTemplates()
    {
        if (_exactMatchWrapper is null)
        {
            _relatedMatchWrappersList = new();
            return;
        }

        var relatedMatches = _exactMatchWrapper.FileTemplate.RelatedFileTemplatesFunc.Invoke(FileName);

        _relatedMatchWrappersList = relatedMatches
            .Select(rel => _fileTemplatesFormWrappersList.First(wrap => rel.Id == wrap.FileTemplate.Id))
            .ToList();
    }
}
