using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Values;

public record struct FunctionInvocationParameterMetadata(
    int IdentifierStartInclusiveIndex,
    TypeReference TypeReference,
    ParameterModifierKind ParameterModifierKind);
