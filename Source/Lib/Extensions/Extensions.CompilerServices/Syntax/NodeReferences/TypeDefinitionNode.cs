using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.Extensions.CompilerServices.Syntax.NodeReferences;

/// <summary>
/// <see cref="TypeDefinitionNode"/> is used anywhere a type is defined.
/// </summary>
public sealed class TypeDefinitionNode : ICodeBlockOwner, IFunctionDefinitionNode, IGenericParameterNode
{
    public TypeDefinitionNode(
        AccessModifierKind accessModifierKind,
        bool hasPartialModifier,
        StorageModifierKind storageModifierKind,
        SyntaxToken typeIdentifier,
        SyntaxToken openAngleBracketToken,
        int offsetGenericParameterEntryList,
        int lengthGenericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        SyntaxToken openParenthesisToken,
        int offsetFunctionArgumentEntryList,
        int lengthFunctionArgumentEntryList,
        SyntaxToken closeParenthesisToken,
        TypeReferenceValue inheritedTypeReference,
        ResourceUri resourceUri)
    {
        AccessModifierKind = accessModifierKind;
        HasPartialModifier = hasPartialModifier;
        StorageModifierKind = storageModifierKind;
        TypeIdentifierToken = typeIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        OffsetGenericParameterEntryList = offsetGenericParameterEntryList;
        LengthGenericParameterEntryList = lengthGenericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        OpenParenthesisToken = openParenthesisToken;
        OffsetFunctionArgumentEntryList = offsetFunctionArgumentEntryList;
        LengthFunctionArgumentEntryList = lengthFunctionArgumentEntryList;
        CloseParenthesisToken = closeParenthesisToken;
        InheritedTypeReference = inheritedTypeReference;
        ResourceUri = resourceUri;
    }

    public AccessModifierKind AccessModifierKind { get; }
    /// <summary>TODO: Use 'IndexPartialTypeDefinition != -1' to signify this bool.</summary>
    public bool HasPartialModifier { get; }
    public int IndexPartialTypeDefinition { get; set; } = -1;
    public StorageModifierKind StorageModifierKind { get; }
    /// <summary>
    /// Given: 'public class Person { /* class definition here */ }'<br/>
    /// Then: 'Person' is the <see cref="TypeIdentifierToken"/><br/>
    /// And: <see cref="GenericArgumentsListingNode"/> would be null
    /// </summary>
    public SyntaxToken TypeIdentifierToken { get; }
    /// <summary>
    /// Given: 'public struct Array&lt;T&gt; { /* struct definition here */ }'<br/>
    /// Then: 'Array&lt;T&gt;' is the <see cref="TypeIdentifierToken"/><br/>
    /// And: '&lt;T&gt;' is the <see cref="GenericArgumentsListingNode"/>
    /// </summary>
    public SyntaxToken OpenAngleBracketToken { get; set; }
    
    public int OffsetGenericParameterEntryList { get; set; }
    public int LengthGenericParameterEntryList { get; set; }
    
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int OffsetFunctionArgumentEntryList { get; set; }
    public int LengthFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    /// <summary>
    /// Given:<br/>
    /// public class Person : IPerson { ... }<br/><br/>
    /// Then: 'IPerson' is the <see cref="InheritedTypeClauseNode"/>
    /// </summary>
    public TypeReferenceValue InheritedTypeReference { get; private set; }
    /// <summary>
    /// 'string.Empty' is used as a special case to store language primitives,
    /// since 'string.Empty' is not a valid 'ResourceUri' for the 'TextEditorService'.
    ///
    /// Perhaps this is odd to do, but the TextEditorTextSpan requires "source text"
    /// to read from.
    ///
    /// So doing this means any special case handling of the language primitives
    /// will "just work" regardless of who tries to read them.
    ///
    /// go-to definition won't do anything since string.Empty isn't a valid file path.
    ///
    /// In particular, this 'string.Empty' file only exists in the CSharpCompilerService's resources.
    /// It never actually gets added to the TextEditorService as a TextEditorModel, only a CSharpResource.
    /// 
    /// The file contents:
    ///     "NotApplicable empty"
    /// 
    /// I just got this to work.
    /// It feels super hacky, so once I think of a better way to do this I'd like to change it.
    /// </summary>
    public ResourceUri ResourceUri { get; }
    public bool IsInterface => StorageModifierKind == StorageModifierKind.Interface;

    public bool IsFabricated { get; init; }
    public SyntaxKind SyntaxKind => SyntaxKind.TypeDefinitionNode;
    
    TypeReferenceValue IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();
    
    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
    public int ParentScopeSubIndex { get; set; } = -1;
    public int SelfScopeSubIndex { get; set; } = -1;

    public bool IsKeywordType { get; init; }
    
    /// <summary>
    /// TODO: TypeDefinitionNode(s) should use the expression loop to parse the...
    /// ...generic parameters. They currently use 'ParseTypes.HandleGenericArguments(...);'
    /// </summary>
    public bool IsParsingGenericParameters { get; set; }

    public ICodeBlockOwner SetInheritedTypeReference(TypeReferenceValue typeReference)
    {
        InheritedTypeReference = typeReference;
        return this;
    }
    
    public TypeReferenceValue ToTypeReference()
    {
        return new TypeReferenceValue(this);
    }
}
