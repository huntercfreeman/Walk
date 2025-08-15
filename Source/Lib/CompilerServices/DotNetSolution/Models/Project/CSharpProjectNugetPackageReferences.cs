using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectNugetPackageReferences
{
    public CSharpProjectNugetPackageReferences(AbsolutePath cSharpProjectAbsolutePath)
    {
        CSharpProjectAbsolutePath = cSharpProjectAbsolutePath;
    }

    public AbsolutePath CSharpProjectAbsolutePath { get; }
}
