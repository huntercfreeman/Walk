using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct SyntaxNodeValue
{
    public SyntaxNodeValue(
        SyntaxToken identifierToken,
        ResourceUri resourceUri,
        bool isFabricated,
        SyntaxKind syntaxKind,
        int parentScopeSubIndex,
        int selfScopeSubIndex)
    {
        IdentifierToken = identifierToken;
        ResourceUri = resourceUri;
        IsFabricated = isFabricated;
        SyntaxKind = syntaxKind;
        ParentScopeSubIndex = parentScopeSubIndex;
        SelfScopeSubIndex = selfScopeSubIndex;
    }
    
    public SyntaxNodeValue(
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
