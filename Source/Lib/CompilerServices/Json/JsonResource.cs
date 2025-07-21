using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Json;

public class JsonResource : CompilerServiceResource
{
    public JsonResource(ResourceUri resourceUri, JsonCompilerService jsonCompilerService)
        : base(resourceUri, jsonCompilerService)
    {
    }
}
