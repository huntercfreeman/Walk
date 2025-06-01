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
	        DecorationByte: (byte)GenericDecorationKind.None,
	        ResourceUri: _resourceUri,
	        SourceText: _text);
	    
	    if (JavaScriptKeywords.ALL_KEYWORDS.Contains(textSpan.Text))
	    {
	    	textSpan = textSpan with
	    	{
	    		DecorationByte = (byte)GenericDecorationKind.Keyword
	    	};
	    }
	
	    _syntaxTokenList.Add(new SyntaxToken(
	    	SyntaxKind.IdentifierToken,
	    	textSpan));
	}
	
	private void NumericLiteralLex()
	{
	    // ...
	}
}
