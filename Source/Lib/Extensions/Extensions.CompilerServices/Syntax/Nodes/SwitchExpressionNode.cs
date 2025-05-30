using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class SwitchExpressionNode : ICodeBlockOwner
{
	public SwitchExpressionNode(
		SyntaxToken keywordToken,
		IExpressionNode expressionNode,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.SwitchExpressionNode++;
		#endif
	
		KeywordToken = keywordToken;
		ExpressionNode = expressionNode;
		CodeBlock = codeBlock;
	}

	public SyntaxToken KeywordToken { get; }
	public IExpressionNode ExpressionNode { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.SwitchExpressionNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~SwitchExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.SwitchExpressionNode--;
	}
	#endif
}
