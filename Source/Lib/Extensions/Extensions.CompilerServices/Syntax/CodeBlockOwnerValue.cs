using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct CodeBlockOwnerValue
{
    public CodeBlockOwnerValue(
        ScopeDirectionKind scopeDirectionKind,
        int scope_StartInclusiveIndex,
        int scope_EndExclusiveIndex,
        int codeBlock_StartInclusiveIndex,
        int codeBlock_EndExclusiveIndex,
        int unsafe_ParentIndexKey,
        int unsafe_SelfIndexKey,
        bool permitCodeBlockParsing,
        bool isImplicitOpenCodeBlockTextSpan,
        TypeReference returnTypeReference)
    {
        ScopeDirectionKind = scopeDirectionKind;
        Scope_StartInclusiveIndex = scope_StartInclusiveIndex;
        Scope_EndExclusiveIndex = scope_EndExclusiveIndex;
        CodeBlock_StartInclusiveIndex = codeBlock_StartInclusiveIndex;
        CodeBlock_EndExclusiveIndex = codeBlock_EndExclusiveIndex;
        Unsafe_ParentIndexKey = unsafe_ParentIndexKey;
        Unsafe_SelfIndexKey = unsafe_SelfIndexKey;
        PermitCodeBlockParsing = permitCodeBlockParsing;
        IsImplicitOpenCodeBlockTextSpan = isImplicitOpenCodeBlockTextSpan;
        ReturnTypeReference = returnTypeReference;
    }
    
    public ScopeDirectionKind ScopeDirectionKind { get; }
    public int Scope_StartInclusiveIndex { get; set; }
    public int Scope_EndExclusiveIndex { get; set; }
    public int CodeBlock_StartInclusiveIndex { get; set; }
    public int CodeBlock_EndExclusiveIndex { get; set; }
    public int Unsafe_ParentIndexKey { get; set; }
    public int Unsafe_SelfIndexKey { get; set; }
    public bool PermitCodeBlockParsing { get; set; }
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }
    public TypeReference ReturnTypeReference { get; set; }
}
