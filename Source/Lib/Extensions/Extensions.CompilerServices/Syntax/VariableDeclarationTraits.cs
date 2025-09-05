using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct VariableDeclarationTraits
{
    public VariableDeclarationTraits(
        VariableKind variableKind,
        TypeReference resultTypeReference,
        bool hasSetter,
        bool hasGetter)
    {
        VariableKind = variableKind;
        ResultTypeReference = resultTypeReference;
        HasSetter = hasSetter;
        HasGetter = hasGetter;
    }

    public VariableKind VariableKind { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    public bool HasSetter { get; set; }
    public bool HasGetter { get; set; }
    
    public bool IsDefault()
    {
        return ResultTypeReference.IsDefault();
    }
}
