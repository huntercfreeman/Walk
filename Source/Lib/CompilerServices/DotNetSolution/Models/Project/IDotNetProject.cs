using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public interface IDotNetProject : ISolutionMember
{
    public string DisplayName { get; }
    public Guid ProjectTypeGuid { get; }
    public string RelativePathFromSolutionFileString { get; }
    public Guid ProjectIdGuid { get; }
    /// <summary>
    /// TODO: Remove the "set;" hack.
    /// </summary>
    public AbsolutePath AbsolutePath { get; set; }
    public DotNetProjectKind DotNetProjectKind { get; }
    public List<AbsolutePath>? ReferencedAbsolutePathList { get; set; }
}
