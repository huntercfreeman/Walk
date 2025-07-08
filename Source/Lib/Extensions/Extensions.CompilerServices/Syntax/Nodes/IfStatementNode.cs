using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class IfStatementNode : ICodeBlockOwner
{
	public IfStatementNode(
		SyntaxToken keywordToken,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.IfStatementNode++;
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
	public SyntaxKind SyntaxKind => SyntaxKind.IfStatementNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(IfStatementNode);

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~IfStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.IfStatementNode--;
	}
	#endif
}