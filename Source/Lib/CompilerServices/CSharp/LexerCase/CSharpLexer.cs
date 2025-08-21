using Walk.CompilerServices.CSharp.BinderCase;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.CSharp.LexerCase;

public static class CSharpLexer
{
    /// <summary>
    /// Initialize the CSharpLexerOutput here, then start the while loop with 'Lex_Frame(...)'.
    /// </summary>
    public static CSharpLexerOutput Lex(CSharpBinder binder, ResourceUri resourceUri, StreamReaderWrap streamReaderWrap, bool shouldUseSharedStringWalker)
    {
        var lexerOutput = new CSharpLexerOutput(resourceUri);
        
        var previousEscapeCharacterTextSpan = new TextEditorTextSpan(
            0,
            0,
            (byte)GenericDecorationKind.None);
            
        var interpolatedExpressionUnmatchedBraceCount = -1;

        Lex_Frame(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, ref interpolatedExpressionUnmatchedBraceCount);
        
        var endOfFileTextSpan = new TextEditorTextSpan(
            streamReaderWrap.PositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None,
            streamReaderWrap.ByteIndex);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, endOfFileTextSpan));
        return lexerOutput;
    }
    
    /// <summary>
    /// Isolate the while loop within its own function in order to permit recursion without allocating new state.
    /// </summary>
    public static void Lex_Frame(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        ref int interpolatedExpressionUnmatchedBraceCount)
    {
        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
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
                    LexIdentifierOrKeywordOrKeywordContextual(binder, ref lexerOutput, streamReaderWrap);
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
                    LexNumericLiteralToken(binder, ref lexerOutput, streamReaderWrap);
                    break;
                case '\'':
                    LexCharLiteralToken(binder, ref lexerOutput, streamReaderWrap);
                    break;
                case '"':
                    LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 0, useVerbatim: false);
                    break;
                case '/':
                    if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        LexCommentSingleLineToken(ref lexerOutput, streamReaderWrap);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '*')
                    {
                        LexCommentMultiLineToken(ref lexerOutput, streamReaderWrap);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DivisionToken, textSpan));
                    }
                    break;
                case '+':
                    if (streamReaderWrap.PeekCharacter(1) == '+')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PlusPlusToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PlusToken, textSpan));
                    }
                    break;
                case '-':
                    if (streamReaderWrap.PeekCharacter(1) == '-')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MinusMinusToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MinusToken, textSpan));
                    }
                    break;
                case '=':
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsEqualsToken, textSpan));
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsCloseAngleBracketToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsToken, textSpan));
                    }
                    break;
                case '?':
                    if (streamReaderWrap.PeekCharacter(1) == '?')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.QuestionMarkQuestionMarkToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.QuestionMarkToken, textSpan));
                    }
                    break;
                case '|':
                    if (streamReaderWrap.PeekCharacter(1) == '|')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PipePipeToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PipeToken, textSpan));
                    }
                    break;
                case '&':
                    if (streamReaderWrap.PeekCharacter(1) == '&')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AmpersandToken, textSpan));
                    }
                    break;
                case '*':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StarToken, textSpan));
                    break;
                }
                case '!':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BangEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BangToken, textSpan));
                    }
                    break;
                }
                case ';':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StatementDelimiterToken, textSpan));
                    break;
                }
                case '(':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenParenthesisToken, textSpan));
                    break;
                }
                case ')':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseParenthesisToken, textSpan));
                    break;
                }
                case '{':
                {
                    if (interpolatedExpressionUnmatchedBraceCount != -1)
                        ++interpolatedExpressionUnmatchedBraceCount;
                
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenBraceToken, textSpan));
                    break;
                }
                case '}':
                {
                    if (interpolatedExpressionUnmatchedBraceCount != -1)
                    {
                        if (--interpolatedExpressionUnmatchedBraceCount <= 0)
                            goto forceExit;
                    }
                
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseBraceToken, textSpan));
                    break;
                }
                case '<':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenAngleBracketEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenAngleBracketToken, textSpan));
                    }
                    break;
                }
                case '>':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseAngleBracketEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseAngleBracketToken, textSpan));
                    }
                    break;
                }
                case '[':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenSquareBracketToken, textSpan));
                    break;
                }
                case ']':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseSquareBracketToken, textSpan));
                    break;
                }
                case '$':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: false);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '@' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: true);
                    }
                    else if (streamReaderWrap.NextCharacter == '$')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;

                        // The while loop starts counting from and including the first dollar sign.
                        var countDollarSign = 0;
                    
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter != '$')
                                break;
                            
                            ++countDollarSign;
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        // Only the last '$' (dollar sign character) will be syntax highlighted
                        // if this code is NOT included.
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.StringLiteral, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringLiteralToken, textSpan));
                        
                        // From the LexString(...) method:
                        //     "awkwardly even if there are many of these it is expected
                        //      that the last one will not have been consumed."
                        streamReaderWrap.BacktrackCharacterNoReturnValue();
                        
                        if (streamReaderWrap.NextCharacter == '"')
                            LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: countDollarSign, useVerbatim: false);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DollarSignToken, textSpan));
                        break;
                    }
                    break;
                case '@':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 0, useVerbatim: true);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '$' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: true);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AtToken, textSpan));
                        break;
                    }
                    break;
                case ':':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ColonToken, textSpan));
                    break;
                }
                case '.':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MemberAccessToken, textSpan));
                    break;
                }
                case ',':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    var byteEntryIndex = streamReaderWrap.ByteIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None, byteEntryIndex);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CommaToken, textSpan));
                    break;
                }
                case '#':
                    LexPreprocessorDirectiveToken(ref lexerOutput, streamReaderWrap);
                    break;
                default:
                    _ = streamReaderWrap.ReadCharacter();
                    break;
            }
        }

        forceExit:
        return;
    }
    
    /// <summary>
    /// The invoker of this method is expected to count the amount of '$' (dollar sign characters).
    /// When it comes to raw strings however, this logic will counted inside of this method.
    ///
    /// The reason being: you don't know if it is a string until you've read all of the '$' (dollar sign characters).
    /// So in order to invoke this method the invoker had to have counted them.
    /// </summary>
    private static void LexString(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        int countDollarSign,
        bool useVerbatim)
    {
        // Interpolated expressions will be done recursively and added to this 'SyntaxTokenList'
        var syntaxTokenListIndex = lexerOutput.SyntaxTokenList.Count;
    
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        var useInterpolation = countDollarSign > 0;
        
        if (useInterpolation)
            _ = streamReaderWrap.ReadCharacter(); // Move past the '$' (dollar sign character); awkwardly even if there are many of these it is expected that the last one will not have been consumed.
        if (useVerbatim)
            _ = streamReaderWrap.ReadCharacter(); // Move past the '@' (at character)
        
        var useRaw = false;
        int countDoubleQuotes = 0;
        
        if (!useVerbatim && streamReaderWrap.PeekCharacter(1) == '\"' && streamReaderWrap.PeekCharacter(2) == '\"')
        {
            useRaw = true;
            
            // Count the amount of double quotes to be used as the delimiter.
            while (!streamReaderWrap.IsEof)
            {
                if (streamReaderWrap.CurrentCharacter != '\"')
                    break;
    
                ++countDoubleQuotes;
                _ = streamReaderWrap.ReadCharacter();
            }
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter(); // Move past the '"' (double quote character)
        }

        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == '\"')
            {
                if (useRaw)
                {
                    var matchDoubleQuotes = 0;
                    
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter != '\"')
                            break;
                        
                        _ = streamReaderWrap.ReadCharacter();
                        if (++matchDoubleQuotes == countDoubleQuotes)
                            goto foundEndDelimiter;
                    }
                    
                    continue;
                }
                else if (useVerbatim && streamReaderWrap.NextCharacter == '\"')
                {
                    EscapeCharacterListAdd(ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, new TextEditorTextSpan(
                        streamReaderWrap.PositionIndex,
                        streamReaderWrap.PositionIndex + 2,
                        (byte)GenericDecorationKind.EscapeCharacterPrimary,
                        streamReaderWrap.ByteIndex));
    
                    _ = streamReaderWrap.ReadCharacter();
                }
                else
                {
                    _ = streamReaderWrap.ReadCharacter();
                    break;
                }
            }
            else if (!useVerbatim && streamReaderWrap.CurrentCharacter == '\\')
            {
                EscapeCharacterListAdd(ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex + 2,
                    (byte)GenericDecorationKind.EscapeCharacterPrimary,
                    streamReaderWrap.ByteIndex));

                // Presuming the escaped text is 2 characters, then read an extra character.
                _ = streamReaderWrap.ReadCharacter();
            }
            else if (useInterpolation && streamReaderWrap.CurrentCharacter == '{')
            {
                // With raw, one is escaping by way of typing less.
                // With normal interpolation, one is escaping by way of typing more.
                //
                // Thus, these are two separate cases written as an if-else.
                if (useRaw)
                {
                    var interpolationTemporaryPositionIndex = streamReaderWrap.PositionIndex;
                    var matchBrace = 0;
                        
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter != '{')
                            break;
                        
                        _ = streamReaderWrap.ReadCharacter();
                        if (++matchBrace >= countDollarSign)
                        {
                            // Found yet another '{' match beyond what was needed.
                            // So, this logic will match from the inside to the outside.
                            if (streamReaderWrap.CurrentCharacter == '{')
                            {
                                ++interpolationTemporaryPositionIndex;
                            }
                            else
                            {
                                // 'LexInterpolatedExpression' is expected to consume one more after it is finished.
                                // Thus, if this while loop were to consume, it would skip the
                                // closing double quotes if the expression were the last thing in the string.
                                //
                                // So, a backtrack is done.
                                LexInterpolatedExpression(
                                    binder,
                                    ref lexerOutput,
                                    streamReaderWrap,
                                    ref previousEscapeCharacterTextSpan,
                                    startInclusiveOpenDelimiter: interpolationTemporaryPositionIndex,
                                    countDollarSign: countDollarSign,
                                    useRaw);
                                streamReaderWrap.BacktrackCharacterNoReturnValue();
                            }
                        }
                    }
                }
                else
                {
                    if (streamReaderWrap.NextCharacter == '{')
                    {
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    else
                    {
                        // 'LexInterpolatedExpression' is expected to consume one more after it is finished.
                        // Thus, if this while loop were to consume, it would skip the
                        // closing double quotes if the expression were the last thing in the string.
                        LexInterpolatedExpression(
                            binder,
                            ref lexerOutput,
                            streamReaderWrap,
                            ref previousEscapeCharacterTextSpan,
                            startInclusiveOpenDelimiter: streamReaderWrap.PositionIndex,
                            countDollarSign: countDollarSign,
                            useRaw);
                        continue;
                    }
                }
            }

            _ = streamReaderWrap.ReadCharacter();
        }

        foundEndDelimiter:

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.StringLiteral,
            byteEntryIndex);

        if (useInterpolation)
        {
            lexerOutput.SyntaxTokenList.Insert(
                syntaxTokenListIndex,
                new SyntaxToken(SyntaxKind.StringInterpolatedStartToken, textSpan));
                
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedEndToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None,
                    streamReaderWrap.ByteIndex)));
        }
        else
        {
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringLiteralToken, textSpan));
        }
    }
    
    /// <summary>
    /// 'startInclusiveFirstOpenDelimiter' refers to:
    ///     $"Hello, {name}"
    ///
    /// how there is a '{' that deliminates the start of the interpolated expression.
    /// at what position index does it lie at?
    ///
    /// In the case of raw strings, the start-inclusive of the multi-character open delimiter is what needs to be provided.
    /// </summary>
    private static void LexInterpolatedExpression(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        int startInclusiveOpenDelimiter,
        int countDollarSign,
        bool useRaw)
    {
        int unmatchedBraceCounter;
        
        if (useRaw)
        {
            // Starts inside the expression

            // TODO: streamReaderWrap.ByteIndex - countDollarSign
            lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                streamReaderWrap.PositionIndex - countDollarSign,
                streamReaderWrap.PositionIndex,
                (byte)GenericDecorationKind.None,
                streamReaderWrap.ByteIndex));
        
            /*var readOpenDelimiterCount = streamReaderWrap.PositionIndex - startInclusiveOpenDelimiter;
        
            for (; readOpenDelimiterCount < countDollarSign; readOpenDelimiterCount++)
            {
                _ = streamReaderWrap.ReadCharacter();
            }*/
            
            unmatchedBraceCounter = countDollarSign;
        }
        else
        {
            // Starts at the OpenBraceToken
        
            lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                streamReaderWrap.PositionIndex,
                streamReaderWrap.PositionIndex + 1,
                (byte)GenericDecorationKind.None,
                streamReaderWrap.ByteIndex));
                
            var readOpenDelimiterCount = streamReaderWrap.PositionIndex - startInclusiveOpenDelimiter;
        
            for (; readOpenDelimiterCount < countDollarSign; readOpenDelimiterCount++)
            {
                _ = streamReaderWrap.ReadCharacter();
            }
            
            unmatchedBraceCounter = countDollarSign;
        }
    
        // Recursive solution that lexes the interpolated expression only, (not including the '{' or '}').
        Lex_Frame(
            binder,
            ref lexerOutput,
            streamReaderWrap,
            ref previousEscapeCharacterTextSpan,
            ref unmatchedBraceCounter);
        
        if (useRaw)
        {
            _ = streamReaderWrap.ReadCharacter(); // This consumes the final '}'.

            // TODO: streamReaderWrap.ByteIndex - 1
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedContinueToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex - 1,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None,
                    streamReaderWrap.ByteIndex)));
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter(); // This consumes the final '}'.

            // TODO: streamReaderWrap.ByteIndex - 1
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedContinueToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex - 1,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None,
                    streamReaderWrap.ByteIndex)));
        }
    }
    
    private static void EscapeCharacterListAdd(
        ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap, ref TextEditorTextSpan previousEscapeCharacterTextSpan, TextEditorTextSpan textSpan)
    {
        if (lexerOutput.MiscTextSpanList.Count > 0)
        {
            if (previousEscapeCharacterTextSpan.EndExclusiveIndex == textSpan.StartInclusiveIndex &&
                previousEscapeCharacterTextSpan.DecorationByte == (byte)GenericDecorationKind.EscapeCharacterPrimary)
            {
                textSpan = textSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.EscapeCharacterSecondary,
                };
            }
        }
        
        previousEscapeCharacterTextSpan = textSpan;
        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexIdentifierOrKeywordOrKeywordContextual(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        // To detect whether a word is an identifier or a keyword:
        // -------------------------------------------------------
        // A buffer of 10 characters is used as the word is read ('stackalloc' is the longest keyword at 10 characters).
        // And every character in the word is casted as an int and summed.
        //
        // The sum of each char is used as a heuristic for whether the word might be a keyword.
        // The value isn't unique, but the collisions are minimal enough for this step to be useful.
        // A switch statement checks whether the word's char sum is equal to that of a known keyword.
        // 
        // If the char sum is equal to a keyword, then:
        // the word's length and the keyword's length are compared.
        //
        // If they both have the same length:
        // a for loop over the buffer is performed to determine
        // whether every character in the word is truly equal
        // to the keyword.
        //
        // The check is only performed for the length of the word, so the indices are always initialized in time.
        // 
        
    
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        var characterIntSum = 0;
        
        int bufferIndex = 0;
        
        while (!streamReaderWrap.IsEof)
        {
            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                streamReaderWrap.CurrentCharacter != '_')
            {
                break;
            }

            characterIntSum += (int)streamReaderWrap.CurrentCharacter;
            if (bufferIndex < 10)
                binder.KeywordCheckBuffer[bufferIndex++] = streamReaderWrap.CurrentCharacter;
            
            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None,
            byteEntryIndex,
            characterIntSum);
        
        switch (characterIntSum)
        {
            // NonContextualKeywords-NonControl
            // ================================
            case 852: // abstract
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 'b' &&
                    binder.KeywordCheckBuffer[2] == 's' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'c' &&
                    binder.KeywordCheckBuffer[7] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AbstractTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 212: // as
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
            
                goto default;
            case 411: // base
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'b' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 's' &&
                    binder.KeywordCheckBuffer[3] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BaseTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
            
                goto default;
            case 428: // bool
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'b' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BoolTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 436: // !! DUPLICATES !!
            
                if (textSpan.Length != 4)
                    goto default;
            
                if (binder.KeywordCheckBuffer[0] == 'b' &&
                    binder.KeywordCheckBuffer[1] == 'y' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'e')
                {
                    // byte
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByteTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'f' &&
                         binder.KeywordCheckBuffer[1] == 'r' &&
                         binder.KeywordCheckBuffer[2] == 'o' &&
                         binder.KeywordCheckBuffer[3] == 'm')
                {
                    // from
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FromTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'i' &&
                         binder.KeywordCheckBuffer[1] == 'n' &&
                         binder.KeywordCheckBuffer[2] == 'i' &&
                         binder.KeywordCheckBuffer[3] == 't')
                {
                    // init
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InitTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 515: // catch
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'c' &&
                    binder.KeywordCheckBuffer[4] == 'h')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CatchTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 414: // char
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 'r')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 711: // checked
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'c' &&
                    binder.KeywordCheckBuffer[4] == 'k' &&
                    binder.KeywordCheckBuffer[5] == 'e' &&
                    binder.KeywordCheckBuffer[6] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CheckedTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 534: // !! DUPLICATES !!
                
                if (textSpan.Length != 5)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'l' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 's' &&
                    binder.KeywordCheckBuffer[4] == 's')
                {
                    // class
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ClassTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'f' &&
                         binder.KeywordCheckBuffer[1] == 'l' &&
                         binder.KeywordCheckBuffer[2] == 'o' &&
                         binder.KeywordCheckBuffer[3] == 'a' &&
                         binder.KeywordCheckBuffer[4] == 't')
                {
                    // float
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FloatTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'a' &&
                         binder.KeywordCheckBuffer[1] == 'w' &&
                         binder.KeywordCheckBuffer[2] == 'a' &&
                         binder.KeywordCheckBuffer[3] == 'i' &&
                         binder.KeywordCheckBuffer[4] == 't')
                {
                    // await
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AwaitTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 551: // !! DUPLICATES !!
                
                if (textSpan.Length != 5)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'n' &&
                    binder.KeywordCheckBuffer[3] == 's' &&
                    binder.KeywordCheckBuffer[4] == 't')
                {
                    // const
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ConstTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 's' &&
                         binder.KeywordCheckBuffer[1] == 'b' &&
                         binder.KeywordCheckBuffer[2] == 'y' &&
                         binder.KeywordCheckBuffer[3] == 't' &&
                         binder.KeywordCheckBuffer[4] == 'e')
                {
                    // sbyte
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SbyteTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 719: // decimal
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'c' &&
                    binder.KeywordCheckBuffer[3] == 'i' &&
                    binder.KeywordCheckBuffer[4] == 'm' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DecimalTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 741: // !! DUPLICATES !!
                
                if (textSpan.Length != 7)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'f' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'u' &&
                    binder.KeywordCheckBuffer[5] == 'l' &&
                    binder.KeywordCheckBuffer[6] == 't')
                {
                    // default
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DefaultTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'd' &&
                         binder.KeywordCheckBuffer[1] == 'y' &&
                         binder.KeywordCheckBuffer[2] == 'n' &&
                         binder.KeywordCheckBuffer[3] == 'a' &&
                         binder.KeywordCheckBuffer[4] == 'm' &&
                         binder.KeywordCheckBuffer[5] == 'i' &&
                         binder.KeywordCheckBuffer[6] == 'c')
                {
                    // dynamic
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DynamicTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 827: // delegate
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'g' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 't' &&
                    binder.KeywordCheckBuffer[7] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DelegateTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 635: // double
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'u' &&
                    binder.KeywordCheckBuffer[3] == 'b' &&
                    binder.KeywordCheckBuffer[4] == 'l' &&
                    binder.KeywordCheckBuffer[5] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoubleTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 437: // enum
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'e' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 'u' &&
                    binder.KeywordCheckBuffer[3] == 'm')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EnumTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 546: // event
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'e' &&
                    binder.KeywordCheckBuffer[1] == 'v' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EventTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 866: // explicit
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'e' &&
                    binder.KeywordCheckBuffer[1] == 'x' &&
                    binder.KeywordCheckBuffer[2] == 'p' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'c' &&
                    binder.KeywordCheckBuffer[6] == 'i' &&
                    binder.KeywordCheckBuffer[7] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExplicitTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 662: // extern
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'e' &&
                    binder.KeywordCheckBuffer[1] == 'x' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'n')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExternTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 523: // false
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 's' &&
                    binder.KeywordCheckBuffer[4] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FalseTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 751: // finally
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'n' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'l' &&
                    binder.KeywordCheckBuffer[5] == 'l' &&
                    binder.KeywordCheckBuffer[6] == 'y')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FinallyTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 528: // fixed
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'x' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FixedTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 859: // implicit
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'm' &&
                    binder.KeywordCheckBuffer[2] == 'p' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'c' &&
                    binder.KeywordCheckBuffer[6] == 'i' &&
                    binder.KeywordCheckBuffer[7] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ImplicitTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 215: // in
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'n')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 331: // int
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 945: // interface
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'f' &&
                    binder.KeywordCheckBuffer[6] == 'a' &&
                    binder.KeywordCheckBuffer[7] == 'c' &&
                    binder.KeywordCheckBuffer[8] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InterfaceTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 861: // internal
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'n' &&
                    binder.KeywordCheckBuffer[6] == 'a' &&
                    binder.KeywordCheckBuffer[7] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InternalTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 220: // is
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IsTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 425: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'l' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'c' &&
                    binder.KeywordCheckBuffer[3] == 'k')
                {
                    // lock
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LockTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'e' &&
                         binder.KeywordCheckBuffer[1] == 'l' &&
                         binder.KeywordCheckBuffer[2] == 's' &&
                         binder.KeywordCheckBuffer[3] == 'e')
                {
                    // else
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ElseTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }
                
                goto default;
            case 432: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'l' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'n' &&
                    binder.KeywordCheckBuffer[3] == 'g')
                {
                    // long
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LongTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'j' &&
                         binder.KeywordCheckBuffer[1] == 'o' &&
                         binder.KeywordCheckBuffer[2] == 'i' &&
                         binder.KeywordCheckBuffer[3] == 'n')
                {
                    // join
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.JoinTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 941: // namespace
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'm' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 's' &&
                    binder.KeywordCheckBuffer[5] == 'p' &&
                    binder.KeywordCheckBuffer[6] == 'a' &&
                    binder.KeywordCheckBuffer[7] == 'c' &&
                    binder.KeywordCheckBuffer[8] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NamespaceTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 330: // new
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'w')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NewTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 443: // null
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'u' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NullTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 631: // object
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'b' &&
                    binder.KeywordCheckBuffer[2] == 'j' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'c' &&
                    binder.KeywordCheckBuffer[5] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ObjectTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 876: // operator
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'p' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'r' &&
                    binder.KeywordCheckBuffer[4] == 'a' &&
                    binder.KeywordCheckBuffer[5] == 't' &&
                    binder.KeywordCheckBuffer[6] == 'o' &&
                    binder.KeywordCheckBuffer[7] == 'r')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OperatorTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 344: // out
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'u' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OutTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 864: // !! DUPLICATES !!
                
                if (textSpan.Length != 8)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'v' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'r' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'i' &&
                    binder.KeywordCheckBuffer[6] == 'd' &&
                    binder.KeywordCheckBuffer[7] == 'e')
                {
                    // override
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OverrideTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'v' &&
                         binder.KeywordCheckBuffer[1] == 'o' &&
                         binder.KeywordCheckBuffer[2] == 'l' &&
                         binder.KeywordCheckBuffer[3] == 'a' &&
                         binder.KeywordCheckBuffer[4] == 't' &&
                         binder.KeywordCheckBuffer[5] == 'i' &&
                         binder.KeywordCheckBuffer[6] == 'l' &&
                         binder.KeywordCheckBuffer[7] == 'e')
                {
                    // volatile
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VolatileTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                    
                goto default;
            case 644: // params
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'p' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'm' &&
                    binder.KeywordCheckBuffer[5] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ParamsTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 763: // private
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'p' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'v' &&
                    binder.KeywordCheckBuffer[4] == 'a' &&
                    binder.KeywordCheckBuffer[5] == 't' &&
                    binder.KeywordCheckBuffer[6] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PrivateTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 970: // protected
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'p' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'e' &&
                    binder.KeywordCheckBuffer[5] == 'c' &&
                    binder.KeywordCheckBuffer[6] == 't' &&
                    binder.KeywordCheckBuffer[7] == 'e' &&
                    binder.KeywordCheckBuffer[8] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ProtectedTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 639: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'p' &&
                    binder.KeywordCheckBuffer[1] == 'u' &&
                    binder.KeywordCheckBuffer[2] == 'b' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'c')
                {
                    // public
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PublicTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'r' &&
                         binder.KeywordCheckBuffer[1] == 'e' &&
                         binder.KeywordCheckBuffer[2] == 'c' &&
                         binder.KeywordCheckBuffer[3] == 'o' &&
                         binder.KeywordCheckBuffer[4] == 'r' &&
                         binder.KeywordCheckBuffer[5] == 'd')
                {
                    // record
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RecordTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 862: // readonly
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'r' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 'd' &&
                    binder.KeywordCheckBuffer[4] == 'o' &&
                    binder.KeywordCheckBuffer[5] == 'n' &&
                    binder.KeywordCheckBuffer[6] == 'l' &&
                    binder.KeywordCheckBuffer[7] == 'y')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReadonlyTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 317: // ref
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'r' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'f')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RefTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 622: // sealed
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'e' &&
                    binder.KeywordCheckBuffer[5] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SealedTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 560: // short
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'r' &&
                    binder.KeywordCheckBuffer[4] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ShortTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 656: // sizeof
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'z' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'o' &&
                    binder.KeywordCheckBuffer[5] == 'f')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SizeofTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 1057: // stackalloc
                if (textSpan.Length == 10 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 't' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 'c' &&
                    binder.KeywordCheckBuffer[4] == 'k' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'l' &&
                    binder.KeywordCheckBuffer[7] == 'l' &&
                    binder.KeywordCheckBuffer[8] == 'o' &&
                    binder.KeywordCheckBuffer[9] == 'c')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StackallocTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 648: // static
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 't' &&
                    binder.KeywordCheckBuffer[2] == 'a' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'c')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StaticTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 663: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 't' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 'i' &&
                    binder.KeywordCheckBuffer[4] == 'n' &&
                    binder.KeywordCheckBuffer[5] == 'g')
                {
                    // string
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 't' &&
                         binder.KeywordCheckBuffer[1] == 'y' &&
                         binder.KeywordCheckBuffer[2] == 'p' &&
                         binder.KeywordCheckBuffer[3] == 'e' &&
                         binder.KeywordCheckBuffer[4] == 'o' &&
                         binder.KeywordCheckBuffer[5] == 'f')
                {
                    // typeof
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TypeofTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 677: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 't' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 'u' &&
                    binder.KeywordCheckBuffer[4] == 'c' &&
                    binder.KeywordCheckBuffer[5] == 't')
                {
                    // struct
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StructTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'u' &&
                         binder.KeywordCheckBuffer[1] == 's' &&
                         binder.KeywordCheckBuffer[2] == 'h' &&
                         binder.KeywordCheckBuffer[3] == 'o' &&
                         binder.KeywordCheckBuffer[4] == 'r' &&
                         binder.KeywordCheckBuffer[5] == 't')
                {
                    // ushort
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UshortTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 440: // this
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 't' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThisTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 448: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 't' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'u' &&
                    binder.KeywordCheckBuffer[3] == 'e')
                {
                    // true
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TrueTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'u' &&
                         binder.KeywordCheckBuffer[1] == 'i' &&
                         binder.KeywordCheckBuffer[2] == 'n' &&
                         binder.KeywordCheckBuffer[3] == 't')
                {
                    // uint
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UintTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 351: // try
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 't' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'y')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TryTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 549: // ulong
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'u' &&
                    binder.KeywordCheckBuffer[1] == 'l' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 'g')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UlongTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 938: // unchecked
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'u' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 'c' &&
                    binder.KeywordCheckBuffer[3] == 'h' &&
                    binder.KeywordCheckBuffer[4] == 'e' &&
                    binder.KeywordCheckBuffer[5] == 'c' &&
                    binder.KeywordCheckBuffer[6] == 'k' &&
                    binder.KeywordCheckBuffer[7] == 'e' &&
                    binder.KeywordCheckBuffer[8] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UncheckedTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 642: // unsafe
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'u' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 's' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'f' &&
                    binder.KeywordCheckBuffer[5] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnsafeTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 550: // using
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'u' &&
                    binder.KeywordCheckBuffer[1] == 's' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 'g')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UsingTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 775: // virtual
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'v' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'u' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VirtualTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 434: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'v' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'd')
                {
                    // void
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VoidTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'w' &&
                         binder.KeywordCheckBuffer[1] == 'h' &&
                         binder.KeywordCheckBuffer[2] == 'e' &&
                         binder.KeywordCheckBuffer[3] == 'n')
                {
                    // when
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhenTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 517: // break
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'b' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'k')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BreakTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 412: // case
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 's' &&
                    binder.KeywordCheckBuffer[3] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CaseTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 869: // continue
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'c' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'n' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'n' &&
                    binder.KeywordCheckBuffer[6] == 'u' &&
                    binder.KeywordCheckBuffer[7] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ContinueTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 211: // do
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'o')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 327: // for
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'r')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 728: // foreach
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'a' &&
                    binder.KeywordCheckBuffer[5] == 'c' &&
                    binder.KeywordCheckBuffer[6] == 'h')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForeachTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 441: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (binder.KeywordCheckBuffer[0] == 'g' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'o')
                {
                    // goto
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GotoTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }
                else if (binder.KeywordCheckBuffer[0] == 'n' &&
                         binder.KeywordCheckBuffer[1] == 'i' &&
                         binder.KeywordCheckBuffer[2] == 'n' &&
                         binder.KeywordCheckBuffer[3] == 't')
                {
                    // nint
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NintTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }
                
                goto default;
            case 207: // if
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'f')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IfTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 672: // return
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'r' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'u' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'n')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReturnTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 658: // switch
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'w' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'c' &&
                    binder.KeywordCheckBuffer[5] == 'h')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SwitchTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 564: // throw
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 't' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 'o' &&
                    binder.KeywordCheckBuffer[4] == 'w')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThrowTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 537: // while
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'w' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhileTokenKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            case 297: // add
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 'd' &&
                    binder.KeywordCheckBuffer[2] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AddTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 307: // and
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AndTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 522: // alias
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 'l' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AliasTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 940: // ascending
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 's' &&
                    binder.KeywordCheckBuffer[2] == 'c' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'n' &&
                    binder.KeywordCheckBuffer[5] == 'd' &&
                    binder.KeywordCheckBuffer[6] == 'i' &&
                    binder.KeywordCheckBuffer[7] == 'n' &&
                    binder.KeywordCheckBuffer[8] == 'g')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AscendingTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 429: // args
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'g' &&
                    binder.KeywordCheckBuffer[3] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ArgsTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 542: // async
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'a' &&
                    binder.KeywordCheckBuffer[1] == 's' &&
                    binder.KeywordCheckBuffer[2] == 'y' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 'c')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsyncTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 219: // by
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'b' &&
                    binder.KeywordCheckBuffer[1] == 'y')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 1044: // descending
                if (textSpan.Length == 10 &&
                    binder.KeywordCheckBuffer[0] == 'd' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 's' &&
                    binder.KeywordCheckBuffer[3] == 'c' &&
                    binder.KeywordCheckBuffer[4] == 'e' &&
                    binder.KeywordCheckBuffer[5] == 'n' &&
                    binder.KeywordCheckBuffer[6] == 'd' &&
                    binder.KeywordCheckBuffer[7] == 'i' &&
                    binder.KeywordCheckBuffer[8] == 'n' &&
                    binder.KeywordCheckBuffer[9] == 'g')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DescendingTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 651: // equals
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'e' &&
                    binder.KeywordCheckBuffer[1] == 'q' &&
                    binder.KeywordCheckBuffer[2] == 'u' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'l' &&
                    binder.KeywordCheckBuffer[5] == 's')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 416: // file
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'f' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FileTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 320: // get
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'g' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GetTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 625: // global
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'g' &&
                    binder.KeywordCheckBuffer[1] == 'l' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'b' &&
                    binder.KeywordCheckBuffer[4] == 'a' &&
                    binder.KeywordCheckBuffer[5] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GlobalTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 557: // group
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'g' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'u' &&
                    binder.KeywordCheckBuffer[4] == 'p')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GroupTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 442: // into
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'i' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'o')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntoTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 325: // let
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'l' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LetTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 717: // managed
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'm' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'n' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'g' &&
                    binder.KeywordCheckBuffer[5] == 'e' &&
                    binder.KeywordCheckBuffer[6] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ManagedTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 630: // nameof
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'm' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'o' &&
                    binder.KeywordCheckBuffer[5] == 'f')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NameofTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 337: // not
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 780: // notnull
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'o' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 'u' &&
                    binder.KeywordCheckBuffer[5] == 'l' &&
                    binder.KeywordCheckBuffer[6] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotnullTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 558: // nuint
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'n' &&
                    binder.KeywordCheckBuffer[1] == 'u' &&
                    binder.KeywordCheckBuffer[2] == 'i' &&
                    binder.KeywordCheckBuffer[3] == 'n' &&
                    binder.KeywordCheckBuffer[4] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NuintTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 221: // on
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'n')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OnTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 225: // or
                if (textSpan.Length == 2 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'r')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 759: // orderby
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'o' &&
                    binder.KeywordCheckBuffer[1] == 'r' &&
                    binder.KeywordCheckBuffer[2] == 'd' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'r' &&
                    binder.KeywordCheckBuffer[5] == 'b' &&
                    binder.KeywordCheckBuffer[6] == 'y')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrderbyTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 749: // partial
                if (textSpan.Length == 7 &&
                    binder.KeywordCheckBuffer[0] == 'p' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'r' &&
                    binder.KeywordCheckBuffer[3] == 't' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'l')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PartialTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 654: // remove
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 'r' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'm' &&
                    binder.KeywordCheckBuffer[3] == 'o' &&
                    binder.KeywordCheckBuffer[4] == 'v' &&
                    binder.KeywordCheckBuffer[5] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RemoveTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 865: // required
                if (textSpan.Length == 8 &&
                    binder.KeywordCheckBuffer[0] == 'r' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'q' &&
                    binder.KeywordCheckBuffer[3] == 'u' &&
                    binder.KeywordCheckBuffer[4] == 'i' &&
                    binder.KeywordCheckBuffer[5] == 'r' &&
                    binder.KeywordCheckBuffer[6] == 'e' &&
                    binder.KeywordCheckBuffer[7] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RequiredTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 638: // scoped
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'c' &&
                    binder.KeywordCheckBuffer[2] == 'o' &&
                    binder.KeywordCheckBuffer[3] == 'p' &&
                    binder.KeywordCheckBuffer[4] == 'e' &&
                    binder.KeywordCheckBuffer[5] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ScopedTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 640: // select
                if (textSpan.Length == 6 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 'e' &&
                    binder.KeywordCheckBuffer[4] == 'c' &&
                    binder.KeywordCheckBuffer[5] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SelectTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 332: // set
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 's' &&
                    binder.KeywordCheckBuffer[1] == 'e' &&
                    binder.KeywordCheckBuffer[2] == 't')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SetTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 944: // unmanaged
                if (textSpan.Length == 9 &&
                    binder.KeywordCheckBuffer[0] == 'u' &&
                    binder.KeywordCheckBuffer[1] == 'n' &&
                    binder.KeywordCheckBuffer[2] == 'm' &&
                    binder.KeywordCheckBuffer[3] == 'a' &&
                    binder.KeywordCheckBuffer[4] == 'n' &&
                    binder.KeywordCheckBuffer[5] == 'a' &&
                    binder.KeywordCheckBuffer[6] == 'g' &&
                    binder.KeywordCheckBuffer[7] == 'e' &&
                    binder.KeywordCheckBuffer[8] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnmanagedTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 541: // value
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'v' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'l' &&
                    binder.KeywordCheckBuffer[3] == 'u' &&
                    binder.KeywordCheckBuffer[4] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ValueTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 329: // var
                if (textSpan.Length == 3 &&
                    binder.KeywordCheckBuffer[0] == 'v' &&
                    binder.KeywordCheckBuffer[1] == 'a' &&
                    binder.KeywordCheckBuffer[2] == 'r')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VarTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 539: // where
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'w' &&
                    binder.KeywordCheckBuffer[1] == 'h' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'r' &&
                    binder.KeywordCheckBuffer[4] == 'e')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhereTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 444: // with
                if (textSpan.Length == 4 &&
                    binder.KeywordCheckBuffer[0] == 'w' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 't' &&
                    binder.KeywordCheckBuffer[3] == 'h')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WithTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword }));
                    return;
                }

                goto default;
            case 535: // yield
                if (textSpan.Length == 5 &&
                    binder.KeywordCheckBuffer[0] == 'y' &&
                    binder.KeywordCheckBuffer[1] == 'i' &&
                    binder.KeywordCheckBuffer[2] == 'e' &&
                    binder.KeywordCheckBuffer[3] == 'l' &&
                    binder.KeywordCheckBuffer[4] == 'd')
                {
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.YieldTokenContextualKeyword, textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl }));
                    return;
                }

                goto default;
            default:
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
                return;
        }
    }
    
    public static void LexNumericLiteralToken(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNotANumber = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
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
                    break;
                default:
                    isNotANumber = true;
                    break;
            }

            if (isNotANumber)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None,
            byteEntryIndex);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NumericLiteralToken, textSpan));
    }
    
    public static void LexCharLiteralToken(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var delimiter = '\'';
        var escapeCharacter = '\\';
        
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        // Move past the opening delimiter
        _ = streamReaderWrap.ReadCharacter();

        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == delimiter)
            {
                _ = streamReaderWrap.ReadCharacter();
                break;
            }
            else if (streamReaderWrap.CurrentCharacter == escapeCharacter)
            {
                lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex + 2,
                    (byte)GenericDecorationKind.EscapeCharacterPrimary,
                    streamReaderWrap.ByteIndex));

                // Presuming the escaped text is 2 characters,
                // then read an extra character.
                _ = streamReaderWrap.ReadCharacter();
            }

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.StringLiteral,
            byteEntryIndex);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharLiteralToken, textSpan));
    }
    
    public static void LexCommentSingleLineToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNewLineCharacter = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '\r':
                case '\n':
                    isNewLineCharacter = true;
                    break;
                default:
                    break;
            }

            if (isNewLineCharacter)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.CommentSingleLine,
            byteEntryIndex);

        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexCommentMultiLineToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        // Move past the initial "/*"
        streamReaderWrap.SkipRange(2);

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var possibleClosingText = false;
        var sawClosingText = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '*':
                    possibleClosingText = true;
                    break;
                case '/':
                    if (possibleClosingText)
                        sawClosingText = true;
                    break;
                default:
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();

            if (sawClosingText)
                break;
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.CommentMultiLine,
            byteEntryIndex);

        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexPreprocessorDirectiveToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;
        var byteEntryIndex = streamReaderWrap.ByteIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNewLineCharacter = false;
        var firstWhitespaceCharacterPositionIndex = -1;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '\r':
                case '\n':
                    isNewLineCharacter = true;
                    break;
                case '\t':
                case ' ':
                    if (firstWhitespaceCharacterPositionIndex == -1)
                        firstWhitespaceCharacterPositionIndex = streamReaderWrap.PositionIndex;
                    break;
                default:
                    break;
            }

            if (isNewLineCharacter)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }
        
        if (firstWhitespaceCharacterPositionIndex == -1)
            firstWhitespaceCharacterPositionIndex = streamReaderWrap.PositionIndex;

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            firstWhitespaceCharacterPositionIndex,
            (byte)GenericDecorationKind.PreprocessorDirective,
            byteEntryIndex);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PreprocessorDirectiveToken, textSpan));
    }
}
