namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when defining a function.
/// </summary>
public struct FunctionArgumentEntry
{
	public FunctionArgumentEntry(
		VariableDeclarationNode variableDeclarationNode,
		SyntaxToken? optionalCompileTimeConstantToken,
		bool isOptional,
		bool hasParamsKeyword,
		bool hasOutKeyword,
		bool hasInKeyword,
		bool hasRefKeyword)
	{	
		VariableDeclarationNode = variableDeclarationNode;
		OptionalCompileTimeConstantToken = optionalCompileTimeConstantToken;
		IsOptional = isOptional;
		HasParamsKeyword = hasParamsKeyword;
		HasOutKeyword = hasOutKeyword;
		HasInKeyword = hasInKeyword;
		HasRefKeyword = hasRefKeyword;
	}

    /// <summary>
    /// TODO: Don't store the VariableDeclarationNode here. Bring any properties needed here directly inline. (avoids struct containing a reference to reference type.
    /// <summary/>
	public VariableDeclarationNode VariableDeclarationNode { get; }
	public SyntaxToken? OptionalCompileTimeConstantToken { get; }
	public bool IsOptional { get; }
	public bool HasThisKeyword { get; }
	public bool HasParamsKeyword { get; }
	public bool HasOutKeyword { get; }
	public bool HasInKeyword { get; }
	public bool HasRefKeyword { get; }
}
