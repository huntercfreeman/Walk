using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Xml;

public sealed class XmlResource : CompilerServiceResource
{
    public XmlResource(ResourceUri resourceUri, XmlCompilerService xmlCompilerService)
        : base(resourceUri, xmlCompilerService)
    {
    }
}
