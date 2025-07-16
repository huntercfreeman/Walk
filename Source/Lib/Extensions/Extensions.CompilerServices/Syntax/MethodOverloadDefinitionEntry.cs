using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax;

public class MethodOverloadDefinitionEntry
{
	public MethodOverloadDefinitionEntry(
        int indexStartGroup,
        ResourceUri resourceUri,
        int scopeIndexKey)
    {
        IndexStartGroup = indexStartGroup;
        ResourceUri = resourceUri;
        ScopeIndexKey = scopeIndexKey;
    }

	public int IndexStartGroup { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public int ScopeIndexKey { get; set; }
}
