using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.Extensions.DotNet.ComponentRenderers.Models;

public interface ITreeViewSolutionFolderRendererType
{
	public SolutionFolder DotNetSolutionFolder { get; set; }
}