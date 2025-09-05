using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

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

    public TypeReference TypeReference { get; }
    public SyntaxToken IdentifierToken { get; }
    public VariableKind VariableKind { get; }
    public SyntaxToken OptionalCompileTimeConstantToken { get; }
    public ArgumentModifierKind ArgumentModifierKind { get; }
}
