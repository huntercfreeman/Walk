using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    /// <summary>
    /// The order of the entries in <see cref="_fileTemplatesList"/> is important
    /// as the .FirstOrDefault(x => ...true...) is used.
    /// </summary>
    private List<IFileTemplate> _fileTemplatesList = new()
    {
        CommonFacts.RazorCodebehind,
        CommonFacts.RazorMarkup,
        CommonFacts.CSharpClass
    };

    public List<IFileTemplate> FileTemplatesList => _fileTemplatesList;
}
