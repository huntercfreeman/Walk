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
		// CodeBlock = codeBlock;
	}

	public SyntaxToken KeywordToken { get; }
	public SyntaxToken OpenParenthesisToken { get; }
	public SyntaxToken CloseParenthesisToken { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.LockStatementNode;

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
