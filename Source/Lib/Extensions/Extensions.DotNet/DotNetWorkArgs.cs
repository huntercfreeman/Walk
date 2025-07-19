using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;

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
	public TreeViewSolution TreeViewSolution { get; set; }
	public TreeViewNamespacePath ProjectNode { get; set; }
	public ITerminal Terminal { get; set; }
	public Func<Task> OnAfterCompletion { get; set; }
	public TreeViewCSharpProjectToProjectReference TreeViewCSharpProjectToProjectReference { get; set; }
	public TreeViewNamespacePath TreeViewProjectToMove { get; set; }
	public string SolutionFolderPath { get; set; }
	public NamespacePath ModifyProjectNamespacePath { get; set; }
	public TreeViewCSharpProjectNugetPackageReference TreeViewCSharpProjectNugetPackageReference { get; set; }
	/* End Menu */
}
