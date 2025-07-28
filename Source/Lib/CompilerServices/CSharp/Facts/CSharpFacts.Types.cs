using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.CompilerServices.CSharp.Facts;

public partial class CSharpFacts
{
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
    ///     "NotApplicable empty" + " void int char string bool var"
    /// 
    /// 'Walk.Extensions.CompilerServices.TypeFacts' contains some types as well and those are the first to appear in the text.
    ///
    /// I just got this to work.
    /// It feels super hacky, so once I think of a better way to do this I'd like to change it.
    /// </summary>
    public class Types
    {
        public static readonly TypeDefinitionNode Void = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(20, 24, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Int = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(25, 28, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Char = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(29, 33, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly TypeDefinitionNode String = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(34, 40, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Bool = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(41, 45, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Var = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(46, 49, (byte)GenericDecorationKind.None)),
            openAngleBracketToken: default,
    		genericParameterEntryList: null,
    		closeAngleBracketToken: default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
            ResourceUri.Empty)
            {
                IsKeywordType = true
            };

        public static readonly IReadOnlyList<TypeDefinitionNode> TypeDefinitionNodes = new[]
        {
            Void,
            Int,
            String,
            Bool,
            Var
        }.ToList();
    }
}
