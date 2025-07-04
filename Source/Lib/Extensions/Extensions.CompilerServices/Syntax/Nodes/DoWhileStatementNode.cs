using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class DoWhileStatementNode : ICodeBlockOwner
{
	public DoWhileStatementNode(
		SyntaxToken doKeywordToken,
		SyntaxToken whileKeywordToken,
		SyntaxToken openParenthesisToken,
		SyntaxToken closeParenthesisToken)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.DoWhileStatementNode++;
		#endif
	
		DoKeywordToken = doKeywordToken;
		WhileKeywordToken = whileKeywordToken;
		OpenParenthesisToken = openParenthesisToken;
		CloseParenthesisToken = closeParenthesisToken;
	}

	public SyntaxToken DoKeywordToken { get; }
	public SyntaxToken WhileKeywordToken { get; set; }
	public SyntaxToken OpenParenthesisToken { get; set; }
	public SyntaxToken CloseParenthesisToken { get; set; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.DoWhileStatementNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~DoWhileStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.DoWhileStatementNode--;
	}
	#endif
}
