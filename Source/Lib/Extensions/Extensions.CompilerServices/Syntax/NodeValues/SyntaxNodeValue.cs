using Walk.Extensions.CompilerServices.Syntax.NodeReferences;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct SyntaxNodeValue
{
    /*public SyntaxNodeValue(
        SyntaxToken identifierToken,
        ResourceUri resourceUri,
        bool isFabricated,
        SyntaxKind syntaxKind,
        int parentScopeSubIndex,
        int selfScopeSubIndex,
        int traitsIndex)
    {
        IdentifierToken = identifierToken;
        ResourceUri = resourceUri;
        IsFabricated = isFabricated;
        SyntaxKind = syntaxKind;
        ParentScopeSubIndex = parentScopeSubIndex;
        SelfScopeSubIndex = selfScopeSubIndex;
        TraitsIndex = traitsIndex;
    }*/

    public SyntaxNodeValue(
        TypeDefinitionNode typeDefinitionNode,
        int traitsIndex)
    {
        IdentifierToken = typeDefinitionNode.TypeIdentifierToken;
        ResourceUri = typeDefinitionNode.ResourceUri;
        IsFabricated = typeDefinitionNode.IsFabricated;
        SyntaxKind = typeDefinitionNode.SyntaxKind;
        ParentScopeSubIndex = typeDefinitionNode.ParentScopeSubIndex;
        SelfScopeSubIndex = typeDefinitionNode.SelfScopeSubIndex;
        TraitsIndex = traitsIndex;
    }
    
    public SyntaxNodeValue(
        NamespaceStatementNode namespaceStatementNode,
        int traitsIndex)
    {
        IdentifierToken = namespaceStatementNode.IdentifierToken;
        ResourceUri = namespaceStatementNode.ResourceUri;
        IsFabricated = namespaceStatementNode.IsFabricated;
        SyntaxKind = namespaceStatementNode.SyntaxKind;
        ParentScopeSubIndex = namespaceStatementNode.ParentScopeSubIndex;
        SelfScopeSubIndex = namespaceStatementNode.SelfScopeSubIndex;
        TraitsIndex = traitsIndex;
    }

    public SyntaxNodeValue(
        FunctionDefinitionNode functionDefinitionNode,
        int traitsIndex)
    {
        IdentifierToken = functionDefinitionNode.FunctionIdentifierToken;
        ResourceUri = functionDefinitionNode.ResourceUri;
        IsFabricated = functionDefinitionNode.IsFabricated;
        SyntaxKind = functionDefinitionNode.SyntaxKind;
        ParentScopeSubIndex = functionDefinitionNode.ParentScopeSubIndex;
        SelfScopeSubIndex = functionDefinitionNode.SelfScopeSubIndex;
        TraitsIndex = traitsIndex;
    }

    public SyntaxNodeValue(
        ConstructorDefinitionNode constructorDefinitionNode,
        int traitsIndex)
    {
        IdentifierToken = constructorDefinitionNode.FunctionIdentifier;
        ResourceUri = constructorDefinitionNode.ResourceUri;
        IsFabricated = constructorDefinitionNode.IsFabricated;
        SyntaxKind = constructorDefinitionNode.SyntaxKind;
        ParentScopeSubIndex = constructorDefinitionNode.ParentScopeSubIndex;
        SelfScopeSubIndex = constructorDefinitionNode.SelfScopeSubIndex;
        TraitsIndex = traitsIndex;
    }

    public SyntaxNodeValue(
        VariableDeclarationNode variableDeclarationNode,
        int traitsIndex)
    {
        IdentifierToken = variableDeclarationNode.IdentifierToken;
        ResourceUri = variableDeclarationNode.ResourceUri;
        IsFabricated = variableDeclarationNode.IsFabricated;
        SyntaxKind = variableDeclarationNode.SyntaxKind;
        ParentScopeSubIndex = variableDeclarationNode.ParentScopeSubIndex;
        SelfScopeSubIndex = -1;
        TraitsIndex = traitsIndex;
    }

    public SyntaxNodeValue(
        LabelDeclarationNode labelDeclarationNode,
        ResourceUri resourceUri,
        int traitsIndex)
    {
        IdentifierToken = labelDeclarationNode.IdentifierToken;
        ResourceUri = resourceUri;
        IsFabricated = labelDeclarationNode.IsFabricated;
        SyntaxKind = labelDeclarationNode.SyntaxKind;
        ParentScopeSubIndex = labelDeclarationNode.ParentScopeSubIndex;
        SelfScopeSubIndex = -1;
        TraitsIndex = traitsIndex;
    }

    public SyntaxToken IdentifierToken { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public bool IsFabricated { get; set; }
    public SyntaxKind SyntaxKind { get; set; }
    public int ParentScopeSubIndex { get; set; }
    public int SelfScopeSubIndex { get; set; }
    public int TraitsIndex { get; set; }
    
    public bool IsDefault()
    {
        return SyntaxKind == SyntaxKind.NotApplicable;
    }
}
