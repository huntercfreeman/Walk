using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ConstructorInvocationExpressionNode : IInvocationNode
{
    /// <summary>
    /// The <see cref="GenericParametersListingNode"/> is located
    /// on the <see cref="TypeClauseNode"/>.
    /// </summary>
    public ConstructorInvocationExpressionNode(
        SyntaxToken newKeywordToken,
        TypeReference typeReference,
        SyntaxToken openParenthesisToken,
        int indexFunctionParameterEntryList,
        int countFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken)
    {
        NewKeywordToken = newKeywordToken;
        ResultTypeReference = typeReference;
        
        OpenParenthesisToken = openParenthesisToken;
        IndexFunctionParameterEntryList = indexFunctionParameterEntryList;
        CountFunctionParameterEntryList = countFunctionParameterEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        
    }

    public SyntaxToken NewKeywordToken { get; set; }
    public TypeReference ResultTypeReference { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public int IdentifierStartInclusiveIndex => NewKeywordToken.TextSpan.StartInclusiveIndex;

    public ConstructorInvocationStageKind ConstructorInvocationStageKind { get; set; } = ConstructorInvocationStageKind.Unset;

    public int ParentScopeOffset { get; set; }
    
    public bool _isFabricated;
    public bool IsFabricated
    {
        get => _isFabricated;
        init => _isFabricated = value;
    }
    
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorInvocationExpressionNode;
    
    public bool IsParsingFunctionParameters { get; set; }
}
