using Walk.Common.RazorLib.Namespaces.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectNugetPackageReferences
{
    public CSharpProjectNugetPackageReferences(NamespacePath cSharpProjectNamespacePath)
    {
        CSharpProjectNamespacePath = cSharpProjectNamespacePath;
    }

    public NamespacePath CSharpProjectNamespacePath { get; }
}