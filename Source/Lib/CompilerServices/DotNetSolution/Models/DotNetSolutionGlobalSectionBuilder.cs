using Walk.Extensions.CompilerServices.Syntax;
using Walk.CompilerServices.DotNetSolution.Models.Associated;

namespace Walk.CompilerServices.DotNetSolution.Models;

public class DotNetSolutionGlobalSectionBuilder
{
    public SyntaxToken? GlobalSectionArgument { get; set; }
    public SyntaxToken? GlobalSectionOrder { get; set; }
    public AssociatedEntryGroup AssociatedEntryGroup { get; set; } = new(openAssociatedGroupToken: default, new List<IAssociatedEntry>(), closeAssociatedGroupToken: default);

    public DotNetSolutionGlobalSection Build()
    {
        return new DotNetSolutionGlobalSection(
            GlobalSectionArgument,
            GlobalSectionOrder,
            AssociatedEntryGroup);
    }
}
