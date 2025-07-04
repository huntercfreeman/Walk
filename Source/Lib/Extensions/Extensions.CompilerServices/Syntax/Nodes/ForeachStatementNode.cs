using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ForeachStatementNode : ICodeBlockOwner
{
	public ForeachStatementNode(
		SyntaxToken foreachKeywordToken,
		SyntaxToken openParenthesisToken,
		SyntaxToken inKeywordToken,
		SyntaxToken closeParenthesisToken,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ForeachStatementNode++;
		#endif
	
		ForeachKeywordToken = foreachKeywordToken;
		OpenParenthesisToken = openParenthesisToken;
		InKeywordToken = inKeywordToken;
		CloseParenthesisToken = closeParenthesisToken;
		// CodeBlock = codeBlock;
	}

	public SyntaxToken ForeachKeywordToken { get; }
	public SyntaxToken OpenParenthesisToken { get; }
	public SyntaxToken InKeywordToken { get; }
	public SyntaxToken CloseParenthesisToken { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ForeachStatementNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~ForeachStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ForeachStatementNode--;
	}
	#endif
}
