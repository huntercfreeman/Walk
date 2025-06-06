using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when invoking a function.
/// </summary>
public struct FunctionParameterEntry
{
	public FunctionParameterEntry(
		bool hasOutKeyword,
		bool hasInKeyword,
		bool hasRefKeyword)
	{
		HasOutKeyword = hasOutKeyword;
		HasInKeyword = hasInKeyword;
		HasRefKeyword = hasRefKeyword;
	}

	public bool HasOutKeyword { get; }
	public bool HasInKeyword { get; }
	public bool HasRefKeyword { get; }
}
