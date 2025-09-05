using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

public sealed class ConstructorInvocationNode : IInvocationNode
{
    /// <summary>
    /// The <see cref="GenericParametersListingNode"/> is located
    /// on the <see cref="TypeClauseNode"/>.
    /// </summary>
    public ConstructorInvocationNode(
        SyntaxToken newKeywordToken,
        TypeReferenceValue typeReference,
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
    public TypeReferenceValue ResultTypeReference { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public int IdentifierStartInclusiveIndex => NewKeywordToken.TextSpan.StartInclusiveIndex;

    public ConstructorInvocationStageKind ConstructorInvocationStageKind { get; set; } = ConstructorInvocationStageKind.Unset;

    public int ParentScopeSubIndex { get; set; }
    
    public bool _isFabricated;
    public bool IsFabricated
    {
        get => _isFabricated;
        init => _isFabricated = value;
    }
    
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorInvocationNode;
    
    public bool IsParsingFunctionParameters { get; set; }
}
