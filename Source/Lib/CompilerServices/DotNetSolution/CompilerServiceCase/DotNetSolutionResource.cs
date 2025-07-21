using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.CompilerServices.DotNetSolution.CompilerServiceCase;

public class DotNetSolutionResource : CompilerServiceResource
{
    public DotNetSolutionResource(ResourceUri resourceUri, DotNetSolutionCompilerService dotNetSolutionCompilerService)
        : base(resourceUri, dotNetSolutionCompilerService)
    {
    }
}
