using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

public struct PartialTypeDefinitionValue
{
    public PartialTypeDefinitionValue(
        ResourceUri resourceUri,
        int indexStartGroup,
        int scopeOffset)
    {
        ResourceUri = resourceUri;
        IndexStartGroup = indexStartGroup;
        ScopeSubIndex = scopeOffset;
    }

    public ResourceUri ResourceUri { get; set; }
    public int IndexStartGroup { get; set; }
    public int ScopeSubIndex { get; set; }
}
