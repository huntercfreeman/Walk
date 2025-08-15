using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.CompilerServices.DotNetSolution.Models;

public interface IDotNetSolution
{
    public Key<DotNetSolutionModel> Key { get; init; }
    public AbsolutePath AbsolutePath { get; init; }
    public DotNetSolutionHeader DotNetSolutionHeader { get; init; }
    public List<IDotNetProject> DotNetProjectList { get; }
    public List<SolutionFolder> SolutionFolderList { get; init; }
    /// <summary>Use when the solution is '.sln'</summary>
    public List<GuidNestedProjectEntry>? GuidNestedProjectEntryList { get; init; }
    /// <summary>Use when the solution is '.slnx'</summary>
    public List<StringNestedProjectEntry>? StringNestedProjectEntryList { get; init; }
    public DotNetSolutionGlobal DotNetSolutionGlobal { get; init; }
    public string SolutionFileContents { get; }

    public string NamespaceString => string.Empty;
}
