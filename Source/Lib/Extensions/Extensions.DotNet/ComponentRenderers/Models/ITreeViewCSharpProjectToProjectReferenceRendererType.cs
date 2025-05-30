using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.Extensions.DotNet.ComponentRenderers.Models;

public interface ITreeViewCSharpProjectToProjectReferenceRendererType
{
	public CSharpProjectToProjectReference CSharpProjectToProjectReference { get; set; }
}