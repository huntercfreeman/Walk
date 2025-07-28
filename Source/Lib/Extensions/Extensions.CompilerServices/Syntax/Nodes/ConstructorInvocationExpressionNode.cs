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
        List<FunctionParameterEntry> functionParameterEntryList,
        SyntaxToken closeParenthesisToken
        
        )
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorInvocationExpressionNode++;
        #endif
    
        NewKeywordToken = newKeywordToken;
        ResultTypeReference = typeReference;
        
        OpenParenthesisToken = openParenthesisToken;
        FunctionParameterEntryList = functionParameterEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        
    }

    public SyntaxToken NewKeywordToken { get; }
    public TypeReference ResultTypeReference { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public List<FunctionParameterEntry> FunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public int IdentifierStartInclusiveIndex => NewKeywordToken.TextSpan.StartInclusiveIndex;

    public ConstructorInvocationStageKind ConstructorInvocationStageKind { get; set; } = ConstructorInvocationStageKind.Unset;

    public int Unsafe_ParentIndexKey { get; set; }
    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorInvocationExpressionNode;
    
    public bool IsParsingFunctionParameters { get; set; }

#if DEBUG
    ~ConstructorInvocationExpressionNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorInvocationExpressionNode--;
    }
    #endif
}
