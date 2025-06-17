using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when defining a function.
/// </summary>
public struct FunctionArgumentEntry
{
	public FunctionArgumentEntry(
		VariableDeclarationNode variableDeclarationNode,
		SyntaxToken optionalCompileTimeConstantToken,
		ArgumentModifierKind argumentModifierKind)
	{
		VariableDeclarationNode = variableDeclarationNode;
		OptionalCompileTimeConstantToken = optionalCompileTimeConstantToken;
		ArgumentModifierKind = argumentModifierKind;
	}

    /// <summary>
    /// TODO: Don't store the VariableDeclarationNode here. Bring any properties needed here directly inline. (avoids struct containing a reference to reference type.
    /// <summary/>
	public VariableDeclarationNode VariableDeclarationNode { get; }
	public SyntaxToken OptionalCompileTimeConstantToken { get; }
	public ArgumentModifierKind ArgumentModifierKind { get; }
}
