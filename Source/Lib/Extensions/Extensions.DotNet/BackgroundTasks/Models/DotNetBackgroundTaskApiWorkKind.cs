namespace Walk.Extensions.DotNet.BackgroundTasks.Models;

public enum DotNetBackgroundTaskApiWorkKind
{
	None,
    SolutionExplorer_TreeView_MultiSelect_DeleteFiles,
    WalkExtensionsDotNetInitializerOnInit,
    WalkExtensionsDotNetInitializerOnAfterRender,
    SubmitNuGetQuery,
    RunTestByFullyQualifiedName,
    SetDotNetSolution,
	SetDotNetSolutionTreeView,
	Website_AddExistingProjectToSolution,
}
