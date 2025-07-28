using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax;

public record struct TypeReference
{
    public TypeReference(
        SyntaxToken typeIdentifier,
        
        SyntaxToken openAngleBracketToken,
        List<GenericParameterEntry> genericParameterEntryList,
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
        GenericParameterEntryList = genericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        TypeKind = typeKind;
        HasQuestionMark = hasQuestionMark;
        ArrayRank = arrayRank;
        IsFabricated = isFabricated;
    }
    
    public TypeReference(TypeClauseNode typeClauseNode)
    {
        typeClauseNode.IsBeingUsed = false;
    
        IsKeywordType = typeClauseNode.IsKeywordType;
        TypeIdentifierToken = typeClauseNode.TypeIdentifierToken;
        
        OpenAngleBracketToken = typeClauseNode.OpenAngleBracketToken;
        GenericParameterEntryList = typeClauseNode.GenericParameterEntryList;
        CloseAngleBracketToken = typeClauseNode.CloseAngleBracketToken;
        
        TypeKind = typeClauseNode.TypeKind;
        HasQuestionMark = typeClauseNode.HasQuestionMark;
        ArrayRank = typeClauseNode.ArrayRank;
        IsFabricated = typeClauseNode.IsFabricated;
        ExplicitDefinitionTextSpan = typeClauseNode.ExplicitDefinitionTextSpan;
        ExplicitDefinitionResourceUri = typeClauseNode.ExplicitDefinitionResourceUri;
    }

    public SyntaxToken TypeIdentifierToken { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; }
    public List<GenericParameterEntry> GenericParameterEntryList { get; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public bool IsKeywordType { get; }
    public TypeKind TypeKind { get; }
    public bool HasQuestionMark { get; }
    public int ArrayRank { get; }
    public bool IsFabricated { get; }
    
    public TextEditorTextSpan ExplicitDefinitionTextSpan { get; set; }
    public ResourceUri ExplicitDefinitionResourceUri { get; set; }
    
    public bool IsImplicit { get; set; }
}
