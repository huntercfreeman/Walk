using Walk.Common.RazorLib.Namespaces.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectDependencies
{
    public CSharpProjectDependencies(NamespacePath cSharpProjectNamespacePath)
    {
        CSharpProjectNamespacePath = cSharpProjectNamespacePath;
    }

    public NamespacePath CSharpProjectNamespacePath { get; }
}
