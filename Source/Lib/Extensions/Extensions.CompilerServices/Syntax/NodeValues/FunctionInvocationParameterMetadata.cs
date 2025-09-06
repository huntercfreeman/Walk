using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct FunctionInvocationParameterMetadata(
    int IdentifierStartInclusiveIndex,
    TypeReferenceValue TypeReference,
    ParameterModifierKind ParameterModifierKind);
