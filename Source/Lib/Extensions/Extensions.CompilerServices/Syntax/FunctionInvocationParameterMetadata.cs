using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct FunctionInvocationParameterMetadata(
    int IdentifierStartInclusiveIndex,
    TypeReference TypeReference,
    ParameterModifierKind ParameterModifierKind);
