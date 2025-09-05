using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

/// <summary>
/// Used when invoking a function.
/// </summary>
public struct FunctionParameter
{
    public FunctionParameter(ParameterModifierKind parameterModifierKind)
    {
        ParameterModifierKind = parameterModifierKind;
    }

    public ParameterModifierKind ParameterModifierKind { get; }
}
