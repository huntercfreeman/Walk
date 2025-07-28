using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class AmbiguousIdentifierExpressionNode : IGenericParameterNode
{
    public AmbiguousIdentifierExpressionNode(
        SyntaxToken token,
        
        SyntaxToken openAngleBracketToken,
        List<GenericParameterEntry> genericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        TypeReference resultTypeReference)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AmbiguousIdentifierExpressionNode++;
        #endif
    
        Token = token;
        
        OpenAngleBracketToken = openAngleBracketToken;
        GenericParameterEntryList = genericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        ResultTypeReference = resultTypeReference;
    }
    
    public SyntaxToken Token { get; set; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public List<GenericParameterEntry> GenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public TypeReference ResultTypeReference { get; set; }
    public bool FollowsMemberAccessToken { get; set; }
    public bool HasQuestionMark { get; set; }

    public int Unsafe_ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.AmbiguousIdentifierExpressionNode;
    
    public void SetSharedInstance(
        SyntaxToken token,
        
        SyntaxToken openAngleBracketToken,
        List<GenericParameterEntry> genericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        TypeReference resultTypeReference,
        bool followsMemberAccessToken)
    {
        Token = token;
        
        OpenAngleBracketToken = openAngleBracketToken;
        GenericParameterEntryList = genericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        ResultTypeReference = resultTypeReference;
        FollowsMemberAccessToken = followsMemberAccessToken;
        HasQuestionMark = false;
    }
    
    public bool IsParsingGenericParameters { get; set; }

#if DEBUG
    ~AmbiguousIdentifierExpressionNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.AmbiguousIdentifierExpressionNode--;
        // Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.Asdfg;
        // Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.CreateText();
    }
    #endif
}

