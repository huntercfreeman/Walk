using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
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
	public GlobalCodeBlockNode()
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.GlobalCodeBlockNode++;
		#endif
	}

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
	public int Scope_StartInclusiveIndex { get; set; } = -1;
	public int Scope_EndExclusiveIndex { get; set; } = -1;
	public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
	public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
	public int Unsafe_ParentIndexKey { get; set; } = -1;
	public int Unsafe_SelfIndexKey { get; set; } = -1;
	public bool PermitCodeBlockParsing { get; set; } = true;
	public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.GlobalCodeBlockNode;

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(GlobalCodeBlockNode);

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion

	#if DEBUG	
	~GlobalCodeBlockNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.GlobalCodeBlockNode--;
	}
	#endif
}
