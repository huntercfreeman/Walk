using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class BinaryExpressionNode : IExpressionNode
{
	public BinaryExpressionNode(
		TypeReference leftOperandTypeReference,
		SyntaxToken operatorToken,
		TypeReference rightOperandTypeReference,
		TypeReference resultTypeReference,
		TypeReference rightExpressionResultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.BinaryExpressionNode++;
		#endif
	
		LeftOperandTypeReference = leftOperandTypeReference;
		OperatorToken = operatorToken;
		RightOperandTypeReference = rightOperandTypeReference;
		ResultTypeReference = resultTypeReference;
		RightExpressionResultTypeReference = rightExpressionResultTypeReference;
	}

	public BinaryExpressionNode(
		TypeReference leftOperandTypeReference,
		SyntaxToken operatorToken,
		TypeReference rightOperandTypeReference,
		TypeReference resultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.BinaryExpressionNode++;
		#endif
	
		LeftOperandTypeReference = leftOperandTypeReference;
		OperatorToken = operatorToken;
		RightOperandTypeReference = rightOperandTypeReference;
		ResultTypeReference = resultTypeReference;
	}

	private TypeReference _rightExpressionResultTypeReference;

	public TypeReference LeftOperandTypeReference { get; }
	public SyntaxToken OperatorToken { get; }
	public TypeReference RightOperandTypeReference { get; }
	public TypeReference ResultTypeReference { get; }
	
	public TypeReference RightExpressionResultTypeReference
	{
		get => _rightExpressionResultTypeReference;
		set
		{
			_rightExpressionResultTypeReference = value;
			RightExpressionNodeWasSet = true;
		}
	}
	public bool RightExpressionNodeWasSet { get; set; }

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.BinaryExpressionNode;

#if DEBUG
	~BinaryExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.BinaryExpressionNode--;
	}
	#endif
}