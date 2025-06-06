namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public record StringNestedProjectEntry(
    bool ChildIsSolutionFolder,
    string ChildIdentifier,
    string SolutionFolderActualName);
