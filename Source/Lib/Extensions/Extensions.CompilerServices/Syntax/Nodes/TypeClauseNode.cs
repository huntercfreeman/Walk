using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// <see cref="TypeClauseNode"/> is used anywhere a type is referenced.
/// </summary>
public sealed class TypeClauseNode : IGenericParameterNode
{
    public TypeClauseNode(
        SyntaxToken typeIdentifier,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        bool isKeywordType)
    {
        IsKeywordType = isKeywordType;
        TypeIdentifierToken = typeIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
    }
    
    /// <summary>
    /// Various UI events can result in a 'TypeReference' needing to be shown on the UI.
    ///
    /// In order to do this however, you'd have to cast 'TypeReference' as 'ISyntax',
    /// and this would cause boxing.
    ///
    /// It is presumably preferred to just eat an "object-y" cost just once by creating a 'TypeClauseNode'.
    /// Lest it get boxed, unboxed, and boxed again -- over and over.
    ///
    /// I'm going to use this constructor in the CSharpBinder expression logic temporarily so I can get it build.
    /// But any usage of this kind should probably be removed.
    /// </summary>
    public TypeClauseNode(TypeReference typeReference)
    {
        IsKeywordType = typeReference.IsKeywordType;
        TypeIdentifierToken = typeReference.TypeIdentifierToken;
        
        OpenAngleBracketToken = typeReference.OpenAngleBracketToken;
        IndexGenericParameterEntryList = typeReference.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = typeReference.CountGenericParameterEntryList;
        CloseAngleBracketToken = typeReference.CloseAngleBracketToken;
        
        ExplicitDefinitionTextSpan = typeReference.ExplicitDefinitionTextSpan;
        ExplicitDefinitionResourceUri = typeReference.ExplicitDefinitionResourceUri;
    }

    public bool _isFabricated;

    /// <summary>
    /// Given: 'int x = 2;'<br/>
    /// Then: 'int' is the <see cref="TypeIdentifierToken"/>
    /// And: <see cref="GenericParametersListingNode"/> would be null
    /// </summary>
    public SyntaxToken TypeIdentifierToken { get; set; }
    /// <summary>
    /// Given: 'int[] x = 2;'<br/>
    /// Then: 'Array&lt;T&gt;' is the <see cref="TypeIdentifierToken"/><br/>
    /// And: '&lt;int&gt;' is the <see cref="GenericParametersListingNode"/>
    /// </summary>
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }

    public bool IsKeywordType { get; set; }

    public TypeKind TypeKind { get; set; }

    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    public bool HasQuestionMark { get; set; }
    public int ArrayRank { get; set; }
    
    public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }
    public ResourceUri ExplicitDefinitionResourceUri { get; set; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated
    {
        get
        {
            return _isFabricated;
        }
        init
        {
            _isFabricated = value;
        }
    }
    
    public SyntaxKind SyntaxKind => SyntaxKind.TypeClauseNode;
    
    public bool IsParsingGenericParameters { get; set; }
}
