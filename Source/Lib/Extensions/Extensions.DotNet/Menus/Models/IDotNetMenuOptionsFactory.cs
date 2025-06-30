using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.Menus.Models;

public interface IDotNetMenuOptionsFactory
{
	public MenuOptionRecord RemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution solutionNode,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		ICommonUiService commonUiService,
		Func<Task> onAfterCompletion);

	public MenuOptionRecord AddProjectToProjectReference(
		TreeViewNamespacePath projectReceivingReference,
		ITerminal terminal,
		ICommonUiService commonUiService,
		IdeBackgroundTaskApi ideBackgroundTaskApi,
		Func<Task> onAfterCompletion);

	public MenuOptionRecord RemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		ICommonUiService commonUiService,
		Func<Task> onAfterCompletion);

	public MenuOptionRecord MoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		ITerminal terminal,
		ICommonUiService commonUiService,
		Func<Task> onAfterCompletion);

	public MenuOptionRecord RemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		ICommonUiService commonUiService,
		Func<Task> onAfterCompletion);
}
