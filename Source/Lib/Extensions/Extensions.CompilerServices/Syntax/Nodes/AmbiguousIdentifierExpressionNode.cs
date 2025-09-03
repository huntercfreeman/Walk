using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class AmbiguousIdentifierExpressionNode : IGenericParameterNode
{
    public AmbiguousIdentifierExpressionNode(
        SyntaxToken token,
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        TypeReference resultTypeReference)
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
    
    public TypeReference ResultTypeReference { get; set; }
    public bool FollowsMemberAccessToken { get; set; }
    public bool HasQuestionMark { get; set; }

    public int ParentScopeSubIndex { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.AmbiguousIdentifierExpressionNode;
    
    public bool IsParsingGenericParameters { get; set; }
    
    public AmbiguousIdentifierExpressionNode GetClone()
    {
        return new AmbiguousIdentifierExpressionNode(
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

