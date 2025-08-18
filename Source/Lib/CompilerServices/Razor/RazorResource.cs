using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Razor;

public sealed class RazorResource : CompilerServiceResource
{
    public RazorResource(ResourceUri resourceUri, RazorCompilerService razorCompilerService)
        : base(resourceUri, razorCompilerService)
    {
    }
}
