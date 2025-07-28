using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

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
        List<GenericParameterEntry> genericParameterEntryList,
        SyntaxToken closeAngleBracketToken,
        
        FunctionArgumentListing primaryConstructorFunctionArgumentListing,
        TypeReference inheritedTypeReference,
        string namespaceName,
        ResourceUri resourceUri)
    {
        #if DEBUG
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeDefinitionNode++;
        #endif
    
        AccessModifierKind = accessModifierKind;
        HasPartialModifier = hasPartialModifier;
        StorageModifierKind = storageModifierKind;
        TypeIdentifierToken = typeIdentifier;
        
        OpenAngleBracketToken = openAngleBracketToken;
        GenericParameterEntryList = genericParameterEntryList;
        CloseAngleBracketToken = closeAngleBracketToken;
        
        FunctionArgumentListing = primaryConstructorFunctionArgumentListing;
        InheritedTypeReference = inheritedTypeReference;
        NamespaceName = namespaceName;
        ResourceUri = resourceUri;
    }

    private TypeClauseNode? _toTypeClauseResult;
    
    private bool _hasCalculatedToTypeReference = false;
    private TypeReference _toTypeReferenceResult;

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
    public List<GenericParameterEntry> GenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    
    public FunctionArgumentListing FunctionArgumentListing { get; set; }
    public FunctionArgumentListing PrimaryConstructorFunctionArgumentListing => FunctionArgumentListing;
    /// <summary>
    /// Given:<br/>
    /// public class Person : IPerson { ... }<br/><br/>
    /// Then: 'IPerson' is the <see cref="InheritedTypeClauseNode"/>
    /// </summary>
    public TypeReference InheritedTypeReference { get; private set; }
    public string NamespaceName { get; }
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
    
    TypeReference IExpressionNode.ResultTypeReference => TypeFacts.Pseudo.ToTypeReference();

    /// <summary>
    /// TODO: Where is this used? ('NamespaceName' already exists and seems to be the one to keep).
    /// </summary>
    public string EncompassingNamespaceIdentifierString { get; set; }
    
    /// <summary>
    /// Any files that contain a reference to this TypeDefinitionNode should
    /// have their ResourceUri in this.
    /// </summary>
    // FindAllReferences
    // public HashSet<ResourceUri>? ReferenceHashSet { get; set; }

    // ICodeBlockOwner properties.
    public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
    public int Scope_StartInclusiveIndex { get; set; } = -1;
    public int Scope_EndExclusiveIndex { get; set; } = -1;
    public int CodeBlock_StartInclusiveIndex { get; set; } = -1;
    public int CodeBlock_EndExclusiveIndex { get; set; } = -1;
    public int Unsafe_ParentIndexKey { get; set; } = -1;
    public int Unsafe_SelfIndexKey { get; set; } = -1;
    public bool PermitCodeBlockParsing { get; set; } = true;
    public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

    public bool IsKeywordType { get; init; }
    
    /// <summary>
    /// TODO: TypeDefinitionNode(s) should use the expression loop to parse the...
    /// ...generic parameters. They currently use 'ParseTypes.HandleGenericArguments(...);'
    /// </summary>
    public bool IsParsingGenericParameters { get; set; }

    public void SetFunctionArgumentListing(FunctionArgumentListing functionArgumentListing)
    {
        FunctionArgumentListing = functionArgumentListing;
    }

    public TypeClauseNode ToTypeClause()
    {
        return _toTypeClauseResult ??= new TypeClauseNode(
            TypeIdentifierToken,
            
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default, 
            
            isKeywordType: IsKeywordType)
        {
            ExplicitDefinitionTextSpan = TypeIdentifierToken.TextSpan,
            ExplicitDefinitionResourceUri = ResourceUri,
        };
    }
    
    public TypeReference ToTypeReference()
    {
        if (!_hasCalculatedToTypeReference)
            _toTypeReferenceResult = new TypeReference(ToTypeClause());
        
        return _toTypeReferenceResult;
    }

    #region ICodeBlockOwner_Methods
    public TypeReference GetReturnTypeReference()
    {
        return TypeFacts.Empty.ToTypeReference();
    }
    #endregion

    public ICodeBlockOwner SetInheritedTypeReference(TypeReference typeReference)
    {
        InheritedTypeReference = typeReference;
        return this;
    }

    #if DEBUG    
    ~TypeDefinitionNode()
    {
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.TypeDefinitionNode--;
    }
    #endif
}
