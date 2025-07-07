using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class SwitchExpressionNode : IExpressionNode
{
	public SwitchExpressionNode()
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.SwitchExpressionNode++;
		#endif
	}

	public int ParentScopeIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.SwitchExpressionNode;
	
	public TypeReference ResultTypeReference { get; }

	public string IdentifierText => nameof(SwitchExpressionNode);

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
