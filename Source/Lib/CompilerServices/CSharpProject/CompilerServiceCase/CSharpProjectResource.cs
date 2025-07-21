using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.CSharpProject.CompilerServiceCase;

public class CSharpProjectResource : CompilerServiceResource
{
    public CSharpProjectResource(ResourceUri resourceUri, CSharpProjectCompilerService cSharpProjectCompilerService)
        : base(resourceUri, cSharpProjectCompilerService)
    {
    }
}
