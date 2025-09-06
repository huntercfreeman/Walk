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
        int offsetFunctionParameterEntryList,
        int lengthFunctionParameterEntryList,
        SyntaxToken closeParenthesisToken)
    {
        NewKeywordToken = newKeywordToken;
        ResultTypeReference = typeReference;
        
        OpenParenthesisToken = openParenthesisToken;
        OffsetFunctionParameterEntryList = offsetFunctionParameterEntryList;
        LengthFunctionParameterEntryList = lengthFunctionParameterEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        
    }

    public SyntaxToken NewKeywordToken { get; set; }
    public TypeReferenceValue ResultTypeReference { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionParameterEntryList { get; set; }
    public int LengthFunctionParameterEntryList { get; set; }
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
