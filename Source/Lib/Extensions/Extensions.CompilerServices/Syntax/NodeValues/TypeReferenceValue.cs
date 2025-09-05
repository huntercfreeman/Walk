using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct TypeReferenceValue
{
    public TypeReferenceValue(
        SyntaxToken typeIdentifier,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        bool isKeywordType,
        TypeKind typeKind,
        bool hasQuestionMark,
        int arrayRank,
        bool isFabricated)
    {
        IsKeywordType = isKeywordType;
        TypeIdentifierToken = typeIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        TypeKind = typeKind;
        HasQuestionMark = hasQuestionMark;
        ArrayRank = arrayRank;
        IsFabricated = isFabricated;
    }
    
    public TypeReferenceValue(TypeClauseNode typeClauseNode)
    {
        IsKeywordType = typeClauseNode.IsKeywordType;
        TypeIdentifierToken = typeClauseNode.TypeIdentifierToken;
        
        OpenAngleBracketToken = typeClauseNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = typeClauseNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = typeClauseNode.CountGenericParameterEntryList;
        CloseAngleBracketToken = typeClauseNode.CloseAngleBracketToken;
        
        TypeKind = typeClauseNode.TypeKind;
        HasQuestionMark = typeClauseNode.HasQuestionMark;
        ArrayRank = typeClauseNode.ArrayRank;
        IsFabricated = typeClauseNode.IsFabricated;
        ExplicitDefinitionTextSpan = typeClauseNode.ExplicitDefinitionTextSpan;
        ExplicitDefinitionResourceUri = typeClauseNode.ExplicitDefinitionResourceUri;
    }
    
    public TypeReferenceValue(TypeDefinitionNode typeDefinitionNode)
    {
        IsKeywordType = typeDefinitionNode.IsKeywordType;
        TypeIdentifierToken = typeDefinitionNode.TypeIdentifierToken;
        
        OpenAngleBracketToken = typeDefinitionNode.OpenAngleBracketToken;
        IndexGenericParameterEntryList = typeDefinitionNode.IndexGenericParameterEntryList;
        CountGenericParameterEntryList = typeDefinitionNode.CountGenericParameterEntryList;
        CloseAngleBracketToken = typeDefinitionNode.CloseAngleBracketToken;
        
        IsFabricated = typeDefinitionNode.IsFabricated;
        ExplicitDefinitionTextSpan = typeDefinitionNode.TypeIdentifierToken.TextSpan;
        ExplicitDefinitionResourceUri = typeDefinitionNode.ResourceUri;
    }

    public SyntaxToken TypeIdentifierToken { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public bool IsKeywordType { get; }
    public TypeKind TypeKind { get; }
    public bool HasQuestionMark { get; }
    public int ArrayRank { get; }
    public bool IsFabricated { get; }
    
    public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }
    public ResourceUri ExplicitDefinitionResourceUri { get; set; }
    
    public bool IsImplicit { get; set; }

    public readonly bool IsDefault()
    {
        return
            TypeIdentifierToken.TextSpan.StartInclusiveIndex == 0 &&
            TypeIdentifierToken.TextSpan.EndExclusiveIndex == 0;
    }
}
