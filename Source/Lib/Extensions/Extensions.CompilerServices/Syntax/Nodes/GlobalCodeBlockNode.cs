using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// I just realized as I made this type,
/// is it "global" or "top level statements"?
///
/// Now that I think about it I think
/// this should be named top-level-statements.
///
/// But I'm in the middle of a lot of changes
/// and cannot mess with the name at the moment.
///
/// --------------------------------------------
///
/// When invoking 'GetChildList()'
/// this will return 'CodeBlockNode.GetChildList();'
/// if 'CodeBlockNode' is not null.
/// </summary>
public sealed class GlobalCodeBlockNode : ICodeBlockOwner
{
    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
    /// <summary>
    /// GlobalCodeBlockNode should have 'Scope_StartInclusiveIndex' be initialized to 0.
    /// This is contrary to the pattern in every other ICodeBlockOwner.
    /// For that reason initialization syntax is used here even though 0 is the default value.
    /// </summary>
    public int Scope_StartInclusiveIndex { get; set; } = 0;
    public int Scope_EndExclusiveIndex { get; set; } = -1;
    public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
    public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
    public int Unsafe_ParentIndexKey { get; set; } = -1;
    /// <summary>
    /// GlobalCodeBlockNode should have 'Unsafe_SelfIndexKey' be initialized to 0.
    /// This is contrary to the pattern in every other ICodeBlockOwner.
    /// For that reason initialization syntax is used here even though 0 is the default value.
    /// </summary>
    public int Unsafe_SelfIndexKey { get; set; } = 0;
    public bool PermitCodeBlockParsing { get; set; } = true;
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; } = true;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.GlobalCodeBlockNode;

    #region ICodeBlockOwner_Methods
    public TypeReference GetReturnTypeReference()
    {
        return TypeFacts.Empty.ToTypeReference();
    }
    #endregion
}
