using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// While parsing expression, it is necessary that there exists a node
/// that indicates a lambda expression is being parsed.
///
/// This type might be more along the lines of a "builder" type.
/// It is meant to be made when starting lambda expression,
/// then the primary expression can be equal to an instae of this type.
///
/// This then directs the parser accordingly until the lambda expression
/// is fully parsed.
///
/// At this point, it is planned that a FunctionDefinitionNode will be
/// made, and a 'MethodGroupExpressionNode' (this type does not yet exist) will be returned as the
/// primary expression.
/// </summary>
public sealed class LambdaExpressionNode : IExpressionNode, ICodeBlockOwner
{
	public LambdaExpressionNode(TypeReference resultTypeReference)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LambdaExpressionNode++;
		#endif
	
		ResultTypeReference = resultTypeReference;
	}

	public TypeReference ResultTypeReference { get; }

	/// <summary>
	/// () => "Abc";
	///     Then this property is true;
	///
	/// () => { return "Abc" };
	///     Then this property is false;
	/// </summary>
	public bool CodeBlockNodeIsExpression { get; set; } = true;
	public bool HasReadParameters { get; set; }
	public List<VariableDeclarationNode> VariableDeclarationNodeList { get; } = new();

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.LambdaExpressionNode;

	public TypeReference ReturnTypeReference { get; }

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return ReturnTypeReference;
	}
	#endregion

	#if DEBUG	
	~LambdaExpressionNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.LambdaExpressionNode--;
	}
	#endif
}
