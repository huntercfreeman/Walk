namespace Walk.Extensions.DotNet.ComponentRenderers.Models;

public class DotNetComponentRenderers : IDotNetComponentRenderers
{
	public DotNetComponentRenderers(
		Type nuGetPackageManagerRendererType,
		Type removeCSharpProjectFromSolutionRendererType)
	{
		NuGetPackageManagerRendererType = nuGetPackageManagerRendererType;
		RemoveCSharpProjectFromSolutionRendererType = removeCSharpProjectFromSolutionRendererType;
	}

	public Type NuGetPackageManagerRendererType { get; }
	public Type RemoveCSharpProjectFromSolutionRendererType { get; }
}
