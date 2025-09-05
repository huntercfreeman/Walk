using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class AmbiguousIdentifierNode : IGenericParameterNode
{
    public AmbiguousIdentifierNode(
        SyntaxToken token,
        SyntaxToken openAngleBracketToken,
        int offsetGenericParameterEntryList,
        int lengthGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        TypeReferenceValue resultTypeReference)
    {
        Token = token;
        
        OpenAngleBracketToken = openAngleBracketToken;
        OffsetGenericParameterEntryList = offsetGenericParameterEntryList;
        LengthGenericParameterEntryList = lengthGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        ResultTypeReference = resultTypeReference;
    }
    
    public SyntaxToken Token { get; set; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
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
            OffsetGenericParameterEntryList,
            LengthGenericParameterEntryList,
            CloseAngleBracketToken,
            ResultTypeReference)
        {
            IsParsingGenericParameters = IsParsingGenericParameters,
            FollowsMemberAccessToken = FollowsMemberAccessToken,
            HasQuestionMark = HasQuestionMark,
        };
    }
}

