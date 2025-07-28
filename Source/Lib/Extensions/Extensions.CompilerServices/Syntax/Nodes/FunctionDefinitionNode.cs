using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

/// <summary>
/// TODO: Track the open and close braces for the function body.
/// </summary>
public sealed class FunctionDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode
{
    public FunctionDefinitionNode(
        AccessModifierKind accessModifierKind,
        TypeReference returnTypeReference,
        SyntaxToken functionIdentifierToken,
        
        SyntaxToken openAngleBracketToken,
        int indexGenericParameterEntryList,
        int countGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        FunctionArgumentListing functionArgumentListing,
        CodeBlock codeBlock,
        ResourceUri resourceUri)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionDefinitionNode++;
        #endif
    
        AccessModifierKind = accessModifierKind;
        ReturnTypeReference = returnTypeReference;
        FunctionIdentifierToken = functionIdentifierToken;
        
        OpenAngleBracketToken = openAngleBracketToken;
        IndexGenericParameterEntryList = indexGenericParameterEntryList;
        CountGenericParameterEntryList = countGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        FunctionArgumentListing = functionArgumentListing;
        ResourceUri = resourceUri;
    }

    public AccessModifierKind AccessModifierKind { get; }
    public TypeReference ReturnTypeReference { get; }
    public SyntaxToken FunctionIdentifierToken { get; }
    
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public FunctionArgumentListing FunctionArgumentListing { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public int IndexMethodOverloadDefinition { get; set; } = -1;

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
    public SyntaxKind SyntaxKind => SyntaxKind.FunctionDefinitionNode;
    
    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
    
    public bool IsParsingGenericParameters { get; set; }
    
    #region ICodeBlockOwner_Methods
    public TypeReference GetReturnTypeReference()
    {
        return ReturnTypeReference;
    }
    #endregion

    #if DEBUG    
    ~FunctionDefinitionNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.FunctionDefinitionNode--;
    }
    #endif
}
