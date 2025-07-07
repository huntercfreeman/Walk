using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.CompilerServices.CSharp.Facts;

public partial class CSharpFacts
{
    public class Types
    {
        public static readonly TypeDefinitionNode Void = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "void".Length, (byte)GenericDecorationKind.None, "void")),
            typeof(void),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
            // FindAllReferences
            // , referenceHashSet: new()
            )
            {
            	IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Int = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "int".Length, (byte)GenericDecorationKind.None, "int")),
            typeof(int),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
            // FindAllReferences
            // , referenceHashSet: new()
            )
            {
            	IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Char = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "char".Length, (byte)GenericDecorationKind.None, "char")),
            typeof(char),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
            // FindAllReferences
            // , referenceHashSet: new()
			)
            {
            	IsKeywordType = true
            };

        public static readonly TypeDefinitionNode String = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "string".Length, (byte)GenericDecorationKind.None, "string")),
            typeof(string),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
			// FindAllReferences
			// ,referenceHashSet: new()
			)
            {
            	IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Bool = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "bool".Length, (byte)GenericDecorationKind.None, "bool")),
            typeof(bool),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
			// FindAllReferences
			// ,referenceHashSet: new()
			)
            {
            	IsKeywordType = true
            };

        public static readonly TypeDefinitionNode Var = new(
            AccessModifierKind.Public,
            false,
            StorageModifierKind.Class,
            new SyntaxToken(SyntaxKind.IdentifierToken, new TextEditorTextSpan(0, "var".Length, (byte)GenericDecorationKind.None, "var")),
            typeof(void),
            default,
            primaryConstructorFunctionArgumentListing: default,
            TypeFacts.NotApplicable.ToTypeReference(),
            string.Empty,
		    ResourceUri.Empty
            // FindAllReferences
            // ,referenceHashSet: new()
            )
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