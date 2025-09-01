using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct PartialTypeDefinitionEntry
{
    public PartialTypeDefinitionEntry(
        ResourceUri resourceUri,
        int indexStartGroup,
        int scopeOffset)
    {
        ResourceUri = resourceUri;
        IndexStartGroup = indexStartGroup;
        ScopeOffset = scopeOffset;
    }

    public ResourceUri ResourceUri { get; set; }
    public int IndexStartGroup { get; set; }
    public int ScopeOffset { get; set; }
}
