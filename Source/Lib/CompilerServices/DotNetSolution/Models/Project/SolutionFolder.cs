using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class SolutionFolder : ISolutionMember
{
    public static readonly Guid SolutionFolderProjectTypeGuid = Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

    public SolutionFolder(
        string displayName,
        string actualName)
    {
        DisplayName = displayName;
        ProjectTypeGuid = Guid.Empty;
        ActualName = actualName;
        ProjectIdGuid = Guid.Empty;
        IsSlnx = true;
    }
    
    public SolutionFolder(
        string displayName,
        Guid projectTypeGuid,
        string actualName,
        Guid projectIdGuid)
    {
        DisplayName = displayName;
        ProjectTypeGuid = projectTypeGuid;
        ActualName = actualName;
        ProjectIdGuid = projectIdGuid;
        IsSlnx = false;
    }

    public string DisplayName { get; }
    public Guid ProjectTypeGuid { get; }
    public Guid ProjectIdGuid { get; }
    public string ActualName { get; }
    
    public bool IsSlnx { get; set; }
    
    public SolutionMemberKind SolutionMemberKind => SolutionMemberKind.SolutionFolder;
}
