using Walk.Extensions.CompilerServices.Syntax.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct VariableDeclarationTraits
{
    public VariableDeclarationTraits(
        VariableKind variableKind,
        TypeReferenceValue resultTypeReference,
        bool hasSetter,
        bool hasGetter)
    {
        VariableKind = variableKind;
        ResultTypeReference = resultTypeReference;
        HasSetter = hasSetter;
        HasGetter = hasGetter;
    }

    public VariableKind VariableKind { get; set; }
    public TypeReferenceValue ResultTypeReference { get; set; }
    public bool HasSetter { get; set; }
    public bool HasGetter { get; set; }
    
    public bool IsDefault()
    {
        return ResultTypeReference.IsDefault();
    }
}
