using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

/// <summary>
/// Scope and codeblock indices should be initialized to -1 as that will imply "null" / that it wasn't set yet.
/// </summary>
public struct Scope
{
    public Scope(
        ScopeDirectionKind scopeDirectionKind,
        int scope_StartInclusiveIndex,
        int scope_EndExclusiveIndex,
        int codeBlock_StartInclusiveIndex,
        int codeBlock_EndExclusiveIndex,
        int parentScopeSubIndex,
        int selfScopeSubIndex,
        int nodeSubIndex,
        bool permitCodeBlockParsing,
        bool isImplicitOpenCodeBlockTextSpan,
        SyntaxKind ownerSyntaxKind)
    {
        ScopeDirectionKind = scopeDirectionKind;
        Scope_StartInclusiveIndex = scope_StartInclusiveIndex;
        Scope_EndExclusiveIndex = scope_EndExclusiveIndex;
        CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
        CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
        ParentScopeSubIndex = parentScopeSubIndex;
        SelfScopeSubIndex = selfScopeSubIndex;
        NodeSubIndex = nodeSubIndex;
        PermitCodeBlockParsing = permitCodeBlockParsing;
        IsImplicitOpenCodeBlockTextSpan = isImplicitOpenCodeBlockTextSpan;
        OwnerSyntaxKind = ownerSyntaxKind;
    }
    
    public ScopeDirectionKind ScopeDirectionKind { get; }
    public int Scope_StartInclusiveIndex { get; set; }
    public int Scope_EndExclusiveIndex { get; set; }
    public int CodeBlock_StartInclusiveIndex { get; set; }
    public int CodeBlock_EndExclusiveIndex { get; set; }
    public int ParentScopeSubIndex { get; set; }
    public int SelfScopeSubIndex { get; set; }
    /// <summary>
    /// When this is '-1', there is no tracked ICodeBlockOwner instance.
    /// </summary>
    public int NodeSubIndex { get; set; }
    public bool PermitCodeBlockParsing { get; set; }
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }
    public SyntaxKind OwnerSyntaxKind { get; set; }
    
    public bool IsDefault()
    {
        return OwnerSyntaxKind == SyntaxKind.NotApplicable;
    }
}
