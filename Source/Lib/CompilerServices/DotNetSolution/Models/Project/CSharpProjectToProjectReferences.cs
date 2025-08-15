using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectToProjectReferences
{
    public CSharpProjectToProjectReferences(AbsolutePath cSharpProjectAbsolutePath)
    {
        CSharpProjectAbsolutePath = cSharpProjectAbsolutePath;
    }

    public AbsolutePath CSharpProjectAbsolutePath { get; }
}
