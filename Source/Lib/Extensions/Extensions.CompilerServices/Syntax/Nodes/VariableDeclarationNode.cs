using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class VariableDeclarationNode : IExpressionNode
{
    public VariableDeclarationNode(
        TypeReference typeReference,
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

    public TypeReference TypeReference { get; private set; }

    public SyntaxToken IdentifierToken { get; }
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

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public int ParentScopeOffset { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.VariableDeclarationNode;

    public VariableDeclarationNode SetImplicitTypeReference(TypeReference typeReference)
    {
        typeReference.IsImplicit = true;
        TypeReference = typeReference;
        return this;
    }
}
