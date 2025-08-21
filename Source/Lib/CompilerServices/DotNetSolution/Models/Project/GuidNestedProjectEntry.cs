namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public record struct GuidNestedProjectEntry(
    Guid ChildProjectIdGuid,
    Guid SolutionFolderIdGuid);
