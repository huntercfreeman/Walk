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
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	// public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

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
