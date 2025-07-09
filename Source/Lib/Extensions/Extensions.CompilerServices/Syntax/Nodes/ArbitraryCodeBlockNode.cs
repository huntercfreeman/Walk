using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ArbitraryCodeBlockNode : ICodeBlockOwner
{
	public ArbitraryCodeBlockNode(ICodeBlockOwner parentCodeBlockOwner)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ArbitraryCodeBlockNode++;
		#endif
	
		ParentCodeBlockOwner = parentCodeBlockOwner;
	}

	public ICodeBlockOwner ParentCodeBlockOwner { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ParentCodeBlockOwner.ScopeDirectionKind;
	public int Scope_StartInclusiveIndex { get; set; } = -1;
	public int Scope_EndExclusiveIndex { get; set; } = -1;
	public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
	public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
	public int Unsafe_ParentIndexKey { get; set; } = -1;
	public int Unsafe_SelfIndexKey { get; set; } = -1;
	public bool PermitCodeBlockParsing { get; set; } = true;
	public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.ArbitraryCodeBlockNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		if (ParentCodeBlockOwner is null)
			return TypeFacts.Empty.ToTypeReference();
		
		return ParentCodeBlockOwner.GetReturnTypeReference();
	}
	#endregion

	#if DEBUG	
	~ArbitraryCodeBlockNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ArbitraryCodeBlockNode--;
	}
	#endif
}
