using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectDependencies
{
    public CSharpProjectDependencies(AbsolutePath cSharpProjectAbsolutePath)
    {
        CSharpProjectAbsolutePath = cSharpProjectAbsolutePath;
    }

    public AbsolutePath CSharpProjectAbsolutePath { get; }
}
