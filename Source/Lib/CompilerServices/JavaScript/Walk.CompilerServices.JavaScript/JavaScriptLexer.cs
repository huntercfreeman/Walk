using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptLexer
{
    private readonly ResourceUri _resourceUri;
    private readonly string _text;
    private readonly List<SyntaxToken> _syntaxTokenList = new();

    public JavaScriptLexer(
        ResourceUri resourceUri,
        string text)
    {
        _resourceUri = resourceUri;
        _text = text;
    }
    
    private int _position;
    
    public IReadOnlyList<SyntaxToken> SyntaxTokenList => _syntaxTokenList;
    
    public void Lex()
    {
        while (_position < _text.Length)
        {
            var character = _text[_position];
        
            switch (character)
            {
                /* Lowercase Letters */
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                /* Uppercase Letters */
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                /* Underscore */
                case '_':
                    KeywordOrIdentifierLex();
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    NumericLiteralLex();
                    break;
                default:
                    _ = _text[_position++];
                    break;
            }
        }
    }
    
    private void KeywordOrIdentifierLex()
    {
        var positionStart = _position;
    
        while (_position < _text.Length)
        {
            if (char.IsLetterOrDigit(_text[_position]))
                _position++;
            else
                break;
        }
    
        var positionEnd = _position;
        
        var textSpan = new TextEditorTextSpan(
            StartInclusiveIndex: positionStart,
            EndExclusiveIndex: positionEnd,
            DecorationByte: (byte)GenericDecorationKind.None);
        
        /*
        public const string AWAIT_KEYWORD = "await";
        public const string BREAK_KEYWORD = "break";
        public const string CASE_KEYWORD = "case";
        public const string CATCH_KEYWORD = "catch";
        public const string CLASS_KEYWORD = "class";
        public const string CONST_KEYWORD = "const";
        public const string CONTINUE_KEYWORD = "continue";
        public const string DEBUGGER_KEYWORD = "debugger";
        public const string DEFAULT_KEYWORD = "default";
        public const string DELETE_KEYWORD = "delete";
        public const string DO_KEYWORD = "do";
        public const string ELSE_KEYWORD = "else";
        public const string ENUM_KEYWORD = "enum";
        public const string EXPORT_KEYWORD = "export";
        public const string EXTENDS_KEYWORD = "extends";
        public const string FALSE_KEYWORD = "false";
        public const string FINALLY_KEYWORD = "finally";
        public const string FOR_KEYWORD = "for";
        public const string FUNCTION_KEYWORD = "function";
        public const string IF_KEYWORD = "if";
        public const string IMPLEMENTS_KEYWORD = "implements";
        public const string IMPORT_KEYWORD = "import";
        public const string IN_KEYWORD = "in";
        public const string INSTANCEOF_KEYWORD = "instanceof";
        public const string INTERFACE_KEYWORD = "interface";
        public const string LET_KEYWORD = "let";
        public const string NEW_KEYWORD = "new";
        public const string NULL_KEYWORD = "null";
        public const string PACKAGE_KEYWORD = "package";
        public const string PRIVATE_KEYWORD = "private";
        public const string PROTECTED_KEYWORD = "protected";
        public const string PUBLIC_KEYWORD = "public";
        public const string RETURN_KEYWORD = "return";
        public const string SUPER_KEYWORD = "super";
        public const string SWITCH_KEYWORD = "switch";
        public const string STATIC_KEYWORD = "static";
        public const string THIS_KEYWORD = "this";
        public const string THROW_KEYWORD = "throw";
        public const string TRY_KEYWORD = "try";
        public const string TRUE_KEYWORD = "True";
        public const string TYPEOF_KEYWORD = "typeof";
        public const string VAR_KEYWORD = "var";
        public const string VOID_KEYWORD = "void";
        public const string WHILE_KEYWORD = "while";
        public const string WITH_KEYWORD = "with";
        public const string YIELD_KEYWORD = "yield";
        */
    
        _syntaxTokenList.Add(new SyntaxToken(
            SyntaxKind.IdentifierToken,
            textSpan));
    }
    
    private void NumericLiteralLex()
    {
        // ...
    }
}
