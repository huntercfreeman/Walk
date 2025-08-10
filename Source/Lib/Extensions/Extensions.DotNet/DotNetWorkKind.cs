namespace Walk.Extensions.DotNet;

public enum DotNetWorkKind
{
    None,
    
    /* Start DotNetBackgroundTaskApiWorkKind */
    SolutionExplorer_TreeView_MultiSelect_DeleteFiles,
    SubmitNuGetQuery,
    RunTestByFullyQualifiedName,
    SetDotNetSolution,
    SetDotNetSolutionTreeView,
    Website_AddExistingProjectToSolution,
    /* End DotNetBackgroundTaskApiWorkKind */
    
    /* Start DotNetMenuOptionsFactoryWorkKind */
    PerformRemoveCSharpProjectReferenceFromSolution,
    PerformRemoveProjectToProjectReference,
    PerformMoveProjectToSolutionFolder,
    PerformRemoveNuGetPackageReferenceFromProject,
    /* End DotNetMenuOptionsFactoryWorkKind */
    
    /* Start TestExplorerSchedulerWorkKind */
    ConstructTreeView,
    DiscoverTests,
    /* End TestExplorerSchedulerWorkKind */
}
