using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.CompilerServices.DotNetSolution.Models.Project;

public class CSharpProjectToProjectReference
{
    public CSharpProjectToProjectReference(
        AbsolutePath modifyProjectAbsolutePath,
        AbsolutePath referenceProjectAbsolutePath)
    {
        ModifyProjectAbsolutePath = modifyProjectAbsolutePath;
        ReferenceProjectAbsolutePath = referenceProjectAbsolutePath;
    }

    /// <summary>The <see cref="ModifyProjectNamespacePath"/> is the <see cref="NamespacePath"/> of the Project which will have its XML data modified.</summary>
    public AbsolutePath ModifyProjectAbsolutePath { get; }
    public AbsolutePath ReferenceProjectAbsolutePath { get; }
}
