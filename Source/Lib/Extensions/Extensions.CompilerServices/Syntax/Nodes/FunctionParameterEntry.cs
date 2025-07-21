using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// Used when invoking a function.
/// </summary>
public struct FunctionParameterEntry
{
    public FunctionParameterEntry(ParameterModifierKind parameterModifierKind)
    {
        ParameterModifierKind = parameterModifierKind;
    }

    public ParameterModifierKind ParameterModifierKind { get; }
}
