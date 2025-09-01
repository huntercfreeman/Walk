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
        int parentScopeOffset,
        int selfScopeOffset,
        int nodeOffset,
        bool permitCodeBlockParsing,
        bool isImplicitOpenCodeBlockTextSpan,
        TypeReference returnTypeReference)
    {
        ScopeDirectionKind = scopeDirectionKind;
        Scope_StartInclusiveIndex = scope_StartInclusiveIndex;
        Scope_EndExclusiveIndex = scope_EndExclusiveIndex;
        CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
        CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
        ParentScopeOffset = parentScopeOffset;
        SelfScopeOffset = selfScopeOffset;
        PermitCodeBlockParsing = permitCodeBlockParsing;
        IsImplicitOpenCodeBlockTextSpan = isImplicitOpenCodeBlockTextSpan;
        ReturnTypeReference = returnTypeReference;
    }
    
    public ScopeDirectionKind ScopeDirectionKind { get; }
    public int Scope_StartInclusiveIndex { get; set; }
    public int Scope_EndExclusiveIndex { get; set; }
    public int CodeBlock_StartInclusiveIndex { get; set; }
    public int CodeBlock_EndExclusiveIndex { get; set; }
    public int ParentScopeOffset { get; set; }
    public int SelfScopeOffset { get; set; }
    public int NodeOffset { get; set; }
    public bool PermitCodeBlockParsing { get; set; }
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }
    public TypeReference ReturnTypeReference { get; set; }
    public SyntaxKind SyntaxKind { get; set; }
    
    public bool IsDefault()
    {
        return SyntaxKind == SyntaxKind.NotApplicable;
    }
}
