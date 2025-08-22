using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptResource : CompilerServiceResource
{
    public JavaScriptResource(ResourceUri resourceUri, JavaScriptCompilerService javaScriptCompilerService)
        : base(resourceUri, javaScriptCompilerService)
    {
    }
}
