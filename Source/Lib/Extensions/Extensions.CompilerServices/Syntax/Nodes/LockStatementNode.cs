using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class LockStatementNode : ICodeBlockOwner
{
	public LockStatementNode(
		SyntaxToken keywordToken,
		SyntaxToken openParenthesisToken,
		SyntaxToken closeParenthesisToken,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LockStatementNode++;
		#endif
	
		KeywordToken = keywordToken;
		OpenParenthesisToken = openParenthesisToken;
		CloseParenthesisToken = closeParenthesisToken;
	}

	public SyntaxToken KeywordToken { get; }
	public SyntaxToken OpenParenthesisToken { get; }
	public SyntaxToken CloseParenthesisToken { get; }

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
	public SyntaxKind SyntaxKind => SyntaxKind.LockStatementNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(LockStatementNode);

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~LockStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LockStatementNode--;
	}
	#endif
}
