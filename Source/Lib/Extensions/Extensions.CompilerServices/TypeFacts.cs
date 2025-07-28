using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.Extensions.CompilerServices;

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
/// The file contents: "NotApplicable empty"
/// 
/// I just got this to work.
/// It feels super hacky, so once I think of a better way to do this I'd like to change it.
/// </summary>
public static class TypeFacts
{
    private static readonly TypeReference _notApplicableTypeReference = new TypeReference(
        typeIdentifier: new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, 13, (byte)GenericDecorationKind.None)),
        openAngleBracketToken: default,
		indexGenericParameterEntryList: -1,
        countGenericParameterEntryList: 0,
		closeAngleBracketToken: default,
        isKeywordType: false,
        typeKind: TypeKind.None,
        hasQuestionMark: false,
        arrayRank: 0,
        isFabricated: false);

    /// <summary>
    /// When a type definition node does NOT inherit some other TypeReference,
    /// then this type is used.
    ///
    /// Note: this specifically implies that there was no ':'/'colon' in the text...
    /// ...contrast this with if there were a ':' but it immediately was followed by the type's
    /// code block body, them you ought to use 'Empty', because they wrote the syntax,
    /// but some part of it was quite literally "empty".
    /// </summary>
    public static readonly TypeDefinitionNode NotApplicable = new(
        AccessModifierKind.Public,
        false,
        StorageModifierKind.Class,
        new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, 13, (byte)GenericDecorationKind.None)),
        openAngleBracketToken: default,
		indexGenericParameterEntryList: -1,
        countGenericParameterEntryList: 0,
		closeAngleBracketToken: default,
        openParenthesisToken: default,
        functionArgumentEntryList: null,
        closeParenthesisToken: default,
        inheritedTypeReference: _notApplicableTypeReference,
        string.Empty,
        ResourceUri.Empty
        // FindAllReferences
        // , referenceHashSet: new()
        );

    /// <summary>
    /// If a <see cref="ISyntaxNode"/> has a <see cref="TypeClauseNode"/>,
    /// but is constructed during parsing process, prior to having found the
    /// <see cref="TypeClauseNode"/>, then this will be used as the
    /// <see cref="TypeClauseNode"/> for the time being until the actual is parsed.
    /// </summary>
    public static readonly TypeDefinitionNode Empty = new(
        AccessModifierKind.Public,
        false,
        StorageModifierKind.Class,
        new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(14, 19, (byte)GenericDecorationKind.None)),
        openAngleBracketToken: default,
		indexGenericParameterEntryList: -1,
        countGenericParameterEntryList: 0,
		closeAngleBracketToken: default,
        openParenthesisToken: default,
        functionArgumentEntryList: null,
        closeParenthesisToken: default,
        inheritedTypeReference: _notApplicableTypeReference,
        string.Empty,
        ResourceUri.Empty
        // FindAllReferences
        // , referenceHashSet: new()
        );

    /// <summary>
    /// When parsing an expression, there may be a <see cref="FunctionInvocationNode"/>
    /// with a <see cref="GenericParametersListingNode"/>.
    ///
    /// The expression code works however by passing around an 'IExpression expressionPrimary'
    /// and an 'IExpression expressionSeconday'.
    ///
    /// This means we cannot add common logic for parsing a <see cref="GenericParametersListingNode"/>
    /// unless the expression parsing code were modified, or maybe a call to a separate
    /// function to parse <see cref="GenericParametersListingNode"/> could be done.
    ///
    /// But, for now an experiment with making <see cref="GenericParametersListingNode"/>
    /// into a <see cref="IExpressionNode"/> is being tried out.
    ///
    /// Any <see cref="IExpressionNode"/> that is part of a complete expression,
    /// will have the <see cref="IExpressionNode.ResultTypeClauseNode"/> of <see cref="Pseudo"/>
    /// in order to indicate that the node is part of a expression, but not itself an expression.
    ///
    /// Use 'TypeClauseNode GenericParameterEntryNode.ResultTypeClauseNode => TypeClauseNodeFacts.Pseudo;'
    /// so that some confusion can be avoided since one has to cast it explicitly as an IExpressionNode
    /// in order to access the property. (i.e.: <see cref="GenericParameterEntryNode.TypeClauseNode"/>
    /// is not equal to GenericParameterEntryNode.ResultTypeClauseNode).
    /// </summary>
    public static readonly TypeDefinitionNode Pseudo = new(
        AccessModifierKind.Public,
        false,
        StorageModifierKind.Class,
        new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(14, 19, (byte)GenericDecorationKind.None)),
        openAngleBracketToken: default,
		indexGenericParameterEntryList: -1,
        countGenericParameterEntryList: 0,
		closeAngleBracketToken: default,
        openParenthesisToken: default,
        functionArgumentEntryList: null,
        closeParenthesisToken: default,
        inheritedTypeReference: _notApplicableTypeReference,
        string.Empty,
        ResourceUri.Empty
        // FindAllReferences
        // , referenceHashSet: new()
        );
}
