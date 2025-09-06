using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct MethodOverloadDefinition
{
    public MethodOverloadDefinition(
        ResourceUri resourceUri,
        int offsetGroup,
        int scopeIndexKey)
    {
        ResourceUri = resourceUri;
        OffsetGroup = offsetGroup;
        ScopeIndexKey = scopeIndexKey;
    }

    public ResourceUri ResourceUri { get; set; }
    public int OffsetGroup { get; set; }
    public int ScopeIndexKey { get; set; }
}
