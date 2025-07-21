using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct PartialTypeDefinitionEntry
{
    public PartialTypeDefinitionEntry(
        ResourceUri resourceUri,
        int indexStartGroup,
        int scopeIndexKey)
    {
        ResourceUri = resourceUri;
        IndexStartGroup = indexStartGroup;
        ScopeIndexKey = scopeIndexKey;
    }

    public ResourceUri ResourceUri { get; set; }
    public int IndexStartGroup { get; set; }
    public int ScopeIndexKey { get; set; }
}
