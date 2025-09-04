using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public struct VariableDeclarationValue
{
    public VariableDeclarationValue(
        VariableKind variableKind,
        TypeReference resultTypeReference)
    {
        VariableKind = variableKind;
    }

    public VariableKind VariableKind { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    
    public bool IsDefault()
    {
        return ResultTypeReference.IsDefault();
    }
}
