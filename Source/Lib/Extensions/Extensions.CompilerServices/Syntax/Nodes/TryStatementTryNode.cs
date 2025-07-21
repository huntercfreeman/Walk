using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class TryStatementTryNode : ICodeBlockOwner
{
    public TryStatementTryNode(
        SyntaxToken keywordToken,
        CodeBlock codeBlock)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TryStatementTryNode++;
        #endif

        KeywordToken = keywordToken;
    }

    public SyntaxToken KeywordToken { get; }

    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
    public int Scope_StartInclusiveIndex { get; set; } = -1;
    public int Scope_EndExclusiveIndex { get; set; } = -1;
    public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
    public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
    public int Unsafe_ParentIndexKey { get; set; } = -1;
    public int Unsafe_SelfIndexKey { get; set; } = -1;
    public bool PermitCodeBlockParsing { get; set; } = true;
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.TryStatementTryNode;

    #region ICodeBlockOwner_Methods
    public TypeReference GetReturnTypeReference()
    {
        return TypeFacts.Empty.ToTypeReference();
    }
    #endregion

    #if DEBUG    
    ~TryStatementTryNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TryStatementTryNode--;
    }
    #endif
}
