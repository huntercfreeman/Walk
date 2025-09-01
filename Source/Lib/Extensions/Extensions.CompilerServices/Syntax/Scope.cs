using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

/// <summary>
/// Scope and codeblock indices should be initialized to -1 as that will imply "null" / that it wasn't set yet.
/// </summary>
public struct Scope
{
    public Scope()
    {
        Scope_StartInclusiveIndex = -1;
        Scope_EndExclusiveIndex = -1;
        CodeBlock_StartInclusiveIndex = -1;
        CodeBlock_EndExclusiveIndex = -1;
    }

    public Scope(
        ScopeDirectionKind scopeDirectionKind,
        int scope_StartInclusiveIndex,
        int scope_EndExclusiveIndex,
        int codeBlock_StartInclusiveIndex,
        int codeBlock_EndExclusiveIndex,
        int offsetParentScope,
        int offsetSelfScope,
        bool permitCodeBlockParsing,
        bool isImplicitOpenCodeBlockTextSpan,
        TypeReference returnTypeReference)
    {
        ScopeDirectionKind = scopeDirectionKind;
        Scope_StartInclusiveIndex = scope_StartInclusiveIndex;
        Scope_EndExclusiveIndex = scope_EndExclusiveIndex;
        CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
        CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
        OffsetParentScope = offsetParentScope;
        OffsetSelfScope = offsetSelfScope;
        PermitCodeBlockParsing = permitCodeBlockParsing;
        IsImplicitOpenCodeBlockTextSpan = isImplicitOpenCodeBlockTextSpan;
        ReturnTypeReference = returnTypeReference;
    }
    
    public ScopeDirectionKind ScopeDirectionKind { get; }
    public int Scope_StartInclusiveIndex { get; set; }
    public int Scope_EndExclusiveIndex { get; set; }
    public int CodeBlock_StartInclusiveIndex { get; set; }
    public int CodeBlock_EndExclusiveIndex { get; set; }
    public int OffsetParentScope { get; set; }
    public int OffsetSelfScope { get; set; }
    public int OffsetNode { get; set; }
    public bool PermitCodeBlockParsing { get; set; }
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }
    public TypeReference ReturnTypeReference { get; set; }
}
