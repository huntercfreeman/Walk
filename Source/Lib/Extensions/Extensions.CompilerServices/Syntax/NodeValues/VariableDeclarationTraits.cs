using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct VariableDeclarationTraits
{
    public VariableDeclarationTraits(VariableDeclarationNode variableDeclarationNode)
    {
        TypeReference = variableDeclarationNode.TypeReference;
        VariableKind = variableDeclarationNode.VariableKind;
        IsInitialized = variableDeclarationNode.IsInitialized;
        ResourceUri = variableDeclarationNode.ResourceUri;
        HasGetter = variableDeclarationNode.HasGetter;
        HasSetter = variableDeclarationNode.HasSetter;
    }

    /*public VariableDeclarationTraits(
        TypeReferenceValue typeReference,
        VariableKind variableKind,
        bool isInitialized,
        ResourceUri resourceUri,
        bool hasGetter,
        bool hasSetter)
    {
        TypeReference = typeReference;
        VariableKind = variableKind;
        IsInitialized = isInitialized;
        ResourceUri = resourceUri;
        HasGetter = hasGetter;
        HasSetter = hasSetter;
    }*/

    public TypeReferenceValue TypeReference { get; set; }

    public VariableKind VariableKind { get; set; }
    public bool IsInitialized { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }

    public bool IsDefault()
    {
        return TypeReference.IsDefault();
    }
}
