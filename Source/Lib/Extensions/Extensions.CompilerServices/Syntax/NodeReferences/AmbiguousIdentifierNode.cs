using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class AmbiguousIdentifierNode : IGenericParameterNode
{
    public AmbiguousIdentifierNode(
        SyntaxToken token,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        TypeReferenceValue resultTypeReference)
    {
        Token = token;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        ResultTypeReference = resultTypeReference;
    }
    
    public SyntaxToken Token { get; set; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public TypeReferenceValue ResultTypeReference { get; set; }
    public bool FollowsMemberAccessToken { get; set; }
    public bool HasQuestionMark { get; set; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.AmbiguousIdentifierNode;
    
    public bool IsParsingGenericParameters { get; set; }
    
    public AmbiguousIdentifierNode GetClone()
    {
        return new AmbiguousIdentifierNode(
            Token,
            OpenAngleBracketToken,
            IndexGenericParameterEntryList,
            CountGenericParameterEntryList,
            CloseAngleBracketToken,
            ResultTypeReference)
        {
            IsParsingGenericParameters = IsParsingGenericParameters,
            FollowsMemberAccessToken = FollowsMemberAccessToken,
            HasQuestionMark = HasQuestionMark,
        };
    }
}

