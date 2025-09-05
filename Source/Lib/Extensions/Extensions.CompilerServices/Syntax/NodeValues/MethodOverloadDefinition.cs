using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

public class MethodOverloadDefinition
{
    public MethodOverloadDefinition(
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
