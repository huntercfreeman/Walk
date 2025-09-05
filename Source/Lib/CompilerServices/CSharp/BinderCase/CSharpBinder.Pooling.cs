using Walk.CompilerServices.CSharp.ParserCase;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;
using Walk.Extensions.CompilerServices.Syntax.Utility;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.CSharp.BinderCase;

public partial class CSharpBinder
{
    public Stack<(int ScopeSubIndex, CSharpDeferredChildScope DeferredChildScope)> CSharpParserModel_ParseChildScopeStack { get; } = new();
    public List<(SyntaxKind DelimiterSyntaxKind, IExpressionNode? ExpressionNode)> CSharpParserModel_ExpressionList { get; set; } = new();
    public List<SyntaxKind> CSharpParserModel_TryParseExpressionSyntaxKindList { get; } = new();
    public HashSet<string> CSharpParserModel_ClearedPartialDefinitionHashSet { get; } = new();
    public List<SyntaxNodeValue> CSharpParserModel_ExternalTypeDefinitionList { get; } = new();
    public CSharpStatementBuilder CSharpParserModel_StatementBuilder { get; } = new();

    public TokenWalker CSharpParserModel_TokenWalker { get; } = new(Array.Empty<SyntaxToken>(), useDeferredParsing: true);

    /// <summary>
    /// This is cleared at the start of a new parse, inside the CSharpParserModel constructor.
    /// </summary>
    public List<TextEditorTextSpan> CSharpParserModel_AddedNamespaceList { get; } = new();

    public List<ISyntaxNode> AmbiguousParenthesizedExpressionNodeChildList { get; } = new();
    public List<VariableDeclarationNode> LambdaExpressionNodeChildList { get; } = new();

    /// <summary>
    /// This list is used within TextEditorEditContext and for the lexers to re-use by clearing it prior to starting the lexing.
    /// </summary>
    internal readonly List<SyntaxToken> LEXER_syntaxTokenList = new();
    /// <summary>
    /// Used by lexer
    /// </summary>
    public char[] KeywordCheckBuffer { get; } = new char[10];

    internal const int POOL_TEMPORARY_LOCAL_VARIABLE_DECLARATION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    public readonly Queue<VariableDeclarationNode> Pool_TemporaryLocalVariableDeclarationNode_Queue = new();

    internal const int POOL_BINARY_EXPRESSION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<BinaryExpressionNode> Pool_BinaryExpressionNode_Queue = new();

    internal const int POOL_TYPE_CLAUSE_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<TypeClauseNode> Pool_TypeClauseNode_Queue = new();

    internal const int POOL_VARIABLE_REFERENCE_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<VariableReferenceNode> Pool_VariableReferenceNode_Queue = new();

    // When parsing Walk.sln solution wide:
    //
    // Count of 3 => Pool_NamespaceClauseNode_%: 81.5%
    // Count of 4 => Pool_NamespaceClauseNode_%: 89.9%
    // Count of 5 => Pool_NamespaceClauseNode_%: 97.9%
    // Count of 6 => Pool_NamespaceClauseNode_%: 98.0%
    // Count of 7 => Pool_NamespaceClauseNode_%: 98.1%
    // Count of 8 => Pool_NamespaceClauseNode_%: 98.2%
    // Count of 9 => Pool_NamespaceClauseNode_%: 98.2%
    // Count of 18 => Pool_NamespaceClauseNode_%: 98.8%
    // Count of 50 => Pool_NamespaceClauseNode_%: 100.0%
    // Count of 25 => Pool_NamespaceClauseNode_%: 99.3%
    //
    // Likely you don't actually have 25 namespace clause nodes one after another.
    //
    // Probably just have an edge case that results in no return, but it is occurring so minimally
    // that you can just increase the count in the pool and observe a % hit increase that is oddly high.
    //
    // I don't commonly use explicit namespace qualification.
    // Thus I would probably pick '5' since that is where the value slows down greatly.
    //
    // But, I also might not have a fair measurement of what the impact of '6' would be,
    // due to me not commonly using explicit namespace qualification.
    //
    // Therefore, I'll go 1 higher than what I'd pick.
    // So, I'd pick '5' thus go 1 higher and pick '6'.
    // 
    internal const int POOL_NAMESPACE_CLAUSE_NODE_MAX_COUNT = 6;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<NamespaceClauseNode> Pool_NamespaceClauseNode_Queue = new();

    internal const int POOL_AMBIGUOUS_IDENTIFIER_EXPRESSION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<AmbiguousIdentifierNode> Pool_AmbiguousIdentifierExpressionNode_Queue = new();

    internal const int POOL_CONSTRUCTOR_INVOCATION_EXPRESSION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<ConstructorInvocationNode> Pool_ConstructorInvocationExpressionNode_Queue = new();

    internal const int POOL_FUNCTION_INVOCATION_NODE_MAX_COUNT = 3;
    /// <summary>This is only safe to use while parsing</summary>
    internal readonly Queue<FunctionInvocationNode> Pool_FunctionInvocationNode_Queue = new();

    public BadExpressionNode Shared_BadExpressionNode { get; } = new BadExpressionNode(
        Facts.CSharpFacts.Types.Void.ToTypeReference(),
        EmptyExpressionNode.Empty,
        EmptyExpressionNode.Empty);
}
