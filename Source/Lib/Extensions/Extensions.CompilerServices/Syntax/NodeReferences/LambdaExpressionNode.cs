using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

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
    public LambdaExpressionNode(TypeReferenceValue resultTypeReference)
    {
        ResultTypeReference = resultTypeReference;
    }

    public TypeReferenceValue ResultTypeReference { get; }

    /// <summary>
    /// () => "Abc";
    ///     Then this property is true;
    ///
    /// () => { return "Abc" };
    ///     Then this property is false;
    /// </summary>
    public bool CodeBlockNodeIsExpression { get; set; } = true;
    public bool HasReadParameters { get; set; }
    public int OffsetLambdaExpressionNodeChildList { get; set; }
    public int LengthLambdaExpressionNodeChildList { get; set; }

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.LambdaExpressionNode;

    public TypeReferenceValue ReturnTypeReference { get; }

    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;
}
