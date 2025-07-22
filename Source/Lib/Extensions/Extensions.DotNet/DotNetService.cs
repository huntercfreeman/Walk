using System.Collections.Concurrent;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.AppDatas.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService : IBackgroundTaskGroup, IDisposable
{
    private readonly HttpClient _httpClient;
    
    public DotNetService(
        IdeService ideService,
        HttpClient httpClient,
        IAppDataService appDataService,
        IServiceProvider serviceProvider)
    {
        IdeService = ideService;
        AppDataService = appDataService;
        _httpClient = httpClient;
        
        DotNetStateChanged += OnDotNetSolutionStateChanged;
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }
    public IdeService IdeService { get; }
    public TextEditorService TextEditorService => IdeService.TextEditorService;
    public CommonService CommonService => IdeService.TextEditorService.CommonService;
    public IAppDataService AppDataService { get; }

    private readonly ConcurrentQueue<DotNetWorkArgs> _workQueue = new();
    
    public void Enqueue(DotNetWorkArgs workArgs)
    {
        _workQueue.Enqueue(workArgs);
        IdeService.TextEditorService.CommonService.Continuous_Enqueue(this);
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out DotNetWorkArgs workArgs))
            return ValueTask.CompletedTask;

        switch (workArgs.WorkKind)
        {
            case DotNetWorkKind.SolutionExplorer_TreeView_MultiSelect_DeleteFiles:
                return Do_SolutionExplorer_TreeView_MultiSelect_DeleteFiles(workArgs.TreeViewCommandArgs);
            case DotNetWorkKind.WalkExtensionsDotNetInitializerOnInit:
                return Do_WalkExtensionsDotNetInitializerOnInit();
            case DotNetWorkKind.WalkExtensionsDotNetInitializerOnAfterRender:
                return Do_WalkExtensionsDotNetInitializerOnAfterRender();
            case DotNetWorkKind.SubmitNuGetQuery:
                return Do_SubmitNuGetQuery(workArgs.NugetPackageManagerQuery);
            case DotNetWorkKind.RunTestByFullyQualifiedName:
                return Do_RunTestByFullyQualifiedName(workArgs.TreeViewStringFragment, workArgs.FullyQualifiedName, workArgs.TreeViewProjectTestModel);
            case DotNetWorkKind.SetDotNetSolution:
                return Do_SetDotNetSolution(workArgs.DotNetSolutionAbsolutePath);
            case DotNetWorkKind.SetDotNetSolutionTreeView:
                return Do_SetDotNetSolutionTreeView(workArgs.DotNetSolutionModelKey);
            case DotNetWorkKind.Website_AddExistingProjectToSolution:
                return Do_Website_AddExistingProjectToSolution(
                    workArgs.DotNetSolutionModelKey,
                    workArgs.ProjectTemplateShortName,
                    workArgs.CSharpProjectName,
                    workArgs.CSharpProjectAbsolutePath);
            case DotNetWorkKind.PerformRemoveCSharpProjectReferenceFromSolution:
            {
                return Do_PerformRemoveCSharpProjectReferenceFromSolution(
                    workArgs.TreeViewSolution, workArgs.ProjectNode, workArgs.Terminal, CommonService, workArgs.OnAfterCompletion);
            }
            case DotNetWorkKind.PerformRemoveProjectToProjectReference:
            {
                return Do_PerformRemoveProjectToProjectReference(
                    workArgs.TreeViewCSharpProjectToProjectReference,
                    workArgs.Terminal,
                    CommonService,
                    workArgs.OnAfterCompletion);
            }
            case DotNetWorkKind.PerformMoveProjectToSolutionFolder:
            {
                return Do_PerformMoveProjectToSolutionFolder(
                    workArgs.TreeViewSolution,
                    workArgs.TreeViewProjectToMove,
                    workArgs.SolutionFolderPath,
                    workArgs.Terminal,
                    CommonService,
                    workArgs.OnAfterCompletion);
            }
            case DotNetWorkKind.PerformRemoveNuGetPackageReferenceFromProject:
            {
                return Do_PerformRemoveNuGetPackageReferenceFromProject(
                    workArgs.ModifyProjectNamespacePath,
                    workArgs.TreeViewCSharpProjectNugetPackageReference,
                    workArgs.Terminal,
                    CommonService,
                    workArgs.OnAfterCompletion);
            }
            case DotNetWorkKind.ConstructTreeView:
            {
                return TestExplorer_Do_ConstructTreeView();
            }
            case DotNetWorkKind.DiscoverTests:
            {
                return Do_DiscoverTests();
            }
            default:
                Console.WriteLine($"{nameof(DotNetService)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
        }
    }
    
    public void Dispose()
    {
        DotNetStateChanged -= OnDotNetSolutionStateChanged;
    }
}
