using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    /// <summary>
    /// The order of the entries in <see cref="_fileTemplatesList"/> is important
    /// as the .FirstOrDefault(x => ...true...) is used.
    /// </summary>
    private List<IFileTemplate> _fileTemplatesList = new()
    {
        IdeFacts.RazorCodebehind,
        IdeFacts.RazorMarkup,
        IdeFacts.CSharpClass
    };

    public List<IFileTemplate> FileTemplatesList => _fileTemplatesList;
}
