using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class VariableDeclarationNode : IExpressionNode
{
    public VariableDeclarationNode(
        TypeReferenceValue typeReference,
        SyntaxToken identifierToken,
        VariableKind variableKind,
        bool isInitialized,
        ResourceUri resourceUri)
    {
        TypeReference = typeReference;
        IdentifierToken = identifierToken;
        VariableKind = variableKind;
        IsInitialized = isInitialized;
        ResourceUri = resourceUri;
    }

    public TypeReferenceValue TypeReference { get; set; }

    public SyntaxToken IdentifierToken { get; set; }
    /// <summary>
    /// TODO: Remove the 'set;' on this property
    /// </summary>
    public VariableKind VariableKind { get; set; }
    public bool IsInitialized { get; set; }
    public ResourceUri ResourceUri { get; set; }
    /// <summary>
    /// TODO: Remove the 'set;' on this property
    /// </summary>
    public bool HasGetter { get; set; }
    /// <summary>
    /// TODO: Remove the 'set;' on this property
    /// </summary>
    public bool GetterIsAutoImplemented { get; set; }
    /// <summary>
    /// TODO: Remove the 'set;' on this property
    /// </summary>
    public bool HasSetter { get; set; }
    /// <summary>
    /// TODO: Remove the 'set;' on this property
    /// </summary>
    public bool SetterIsAutoImplemented { get; set; }

    TypeReferenceValue IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int ParentScopeSubIndex { get; set; }
    public bool _isFabricated;
    public bool IsFabricated
    {
        get => _isFabricated;
        init => _isFabricated = value;
    }
    public SyntaxKind SyntaxKind => SyntaxKind.VariableDeclarationNode;

    public VariableDeclarationNode SetImplicitTypeReference(TypeReferenceValue typeReference)
    {
        typeReference.IsImplicit = true;
        TypeReference = typeReference;
        return this;
    }
}
