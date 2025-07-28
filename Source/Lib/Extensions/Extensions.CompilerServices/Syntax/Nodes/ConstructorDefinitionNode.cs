using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class ConstructorDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode
{
    public ConstructorDefinitionNode(
        TypeReference returnTypeReference,
        SyntaxToken functionIdentifier,
        
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        SyntaxToken openParenthesisToken,
        List<FunctionArgumentEntry> functionArgumentEntryList,
        SyntaxToken closeParenthesisToken,
        CodeBlock codeBlock,
        ResourceUri resourceUri)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorDefinitionNode++;
        #endif
    
        ReturnTypeReference = returnTypeReference;
        FunctionIdentifier = functionIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        FunctionArgumentEntryList = functionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        ResourceUri = resourceUri;
    }

    public TypeReference ReturnTypeReference { get; }
    public SyntaxToken FunctionIdentifier { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; private set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public List<FunctionArgumentEntry> FunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    public ResourceUri ResourceUri { get; set; }

    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Down;
    public int Scope_StartInclusiveIndex { get; set; } = -1;
    public int Scope_EndExclusiveIndex { get; set; } = -1;
    public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
    public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
    public int Unsafe_ParentIndexKey { get; set; } = -1;
    public int Unsafe_SelfIndexKey { get; set; } = -1;
    public bool PermitCodeBlockParsing { get; set; } = true;
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.ConstructorDefinitionNode;
    
    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    #region ICodeBlockOwner_Methods
    public TypeReference GetReturnTypeReference()
    {
        return ReturnTypeReference;
    }
    #endregion

    #if DEBUG    
    ~ConstructorDefinitionNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.ConstructorDefinitionNode--;
    }
    #endif
}
