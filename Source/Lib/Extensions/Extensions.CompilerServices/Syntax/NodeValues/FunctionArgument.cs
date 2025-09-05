using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when defining a function.
/// </summary>
public struct FunctionArgument
{
    public FunctionArgument(
        VariableDeclarationNode variableDeclarationNode,
        SyntaxToken optionalCompileTimeConstantToken,
        ArgumentModifierKind argumentModifierKind)
    {
        TypeReference = variableDeclarationNode.TypeReference;
        IdentifierToken = variableDeclarationNode.IdentifierToken;
        VariableKind = variableDeclarationNode.VariableKind;
        
        OptionalCompileTimeConstantToken = optionalCompileTimeConstantToken;
        ArgumentModifierKind = argumentModifierKind;
    }

    public TypeReferenceValue TypeReference { get; }
    public SyntaxToken IdentifierToken { get; }
    public VariableKind VariableKind { get; }
    public SyntaxToken OptionalCompileTimeConstantToken { get; }
    public ArgumentModifierKind ArgumentModifierKind { get; }
}
