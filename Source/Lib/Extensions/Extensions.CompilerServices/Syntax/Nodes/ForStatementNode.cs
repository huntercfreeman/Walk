using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ForStatementNode : ICodeBlockOwner
{
	public ForStatementNode(
		SyntaxToken keywordToken,
		SyntaxToken openParenthesisToken,
		SyntaxToken initializationStatementDelimiterToken,
		SyntaxToken conditionStatementDelimiterToken,
		SyntaxToken closeParenthesisToken,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ForStatementNode++;
		#endif
	
		KeywordToken = keywordToken;
		OpenParenthesisToken = openParenthesisToken;
		InitializationStatementDelimiterToken = initializationStatementDelimiterToken;
		ConditionStatementDelimiterToken = conditionStatementDelimiterToken;
		CloseParenthesisToken = closeParenthesisToken;
		// CodeBlock = codeBlock;
	}

	public SyntaxToken KeywordToken { get; }
	public SyntaxToken OpenParenthesisToken { get; }
	public SyntaxToken InitializationStatementDelimiterToken { get; }
	public SyntaxToken ConditionStatementDelimiterToken { get; }
	public SyntaxToken CloseParenthesisToken { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ForStatementNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~ForStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ForStatementNode--;
	}
	#endif
}
