using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet;

public class DotNetWorkArgs
{
    public DotNetWorkKind WorkKind { get; set; }
    
	/* Start DotNetBackgroundTaskApiWorkArgs */
	public TreeViewCommandArgs TreeViewCommandArgs { get; set; }
	public TreeViewStringFragment TreeViewStringFragment { get; set; }
	public TreeViewProjectTestModel TreeViewProjectTestModel { get; set; }
	public string FullyQualifiedName { get; set; }
	public INugetPackageManagerQuery NugetPackageManagerQuery { get; set; }
	public Key<DotNetSolutionModel> DotNetSolutionModelKey { get; set; }
	public string ProjectTemplateShortName { get; set; }
	public string CSharpProjectName { get; set; }
	public AbsolutePath CSharpProjectAbsolutePath { get; set; }
    public AbsolutePath DotNetSolutionAbsolutePath { get; set; }
    /* End DotNetBackgroundTaskApiWorkArgs */

    /* Start Menu */
	private readonly
		Queue<(TreeViewSolution treeViewSolution, TreeViewNamespacePath projectNode, ITerminal terminal, CommonService commonService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveCSharpProjectReferenceFromSolution = new();

	private readonly
		Queue<(TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference, ITerminal terminal, CommonService commonService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveProjectToProjectReference = new();

	private readonly
		Queue<(TreeViewSolution treeViewSolution, TreeViewNamespacePath treeViewProjectToMove, string solutionFolderPath, ITerminal terminal, CommonService commonService, Func<Task> onAfterCompletion)>
		_queue_PerformMoveProjectToSolutionFolder = new();

	private readonly
		Queue<(NamespacePath modifyProjectNamespacePath, TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference, ITerminal terminal, CommonService commonService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveNuGetPackageReferenceFromProject = new();
	/* End Menu */
}
