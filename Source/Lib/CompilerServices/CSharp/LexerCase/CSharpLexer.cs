using Walk.CompilerServices.CSharp.BinderCase;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Exceptions;
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
        Span<int> keywordCheckBuffer = stackalloc int[10];
    
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
                keywordCheckBuffer[bufferIndex++] = streamReaderWrap.ReadCharacter();
            else
                _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None,
            byteEntryIndex);
        
        switch (characterIntSum)
        {
            // NonContextualKeywords-NonControl
            // ================================
            case 852: // abstract
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 'b' &&
                    keywordCheckBuffer[2] == 's' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'c' &&
                    keywordCheckBuffer[7] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AbstractTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 212: // as
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsTokenKeyword, textSpan));
                    return;
                }
            
                goto default;
            case 411: // base
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'b' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 's' &&
                    keywordCheckBuffer[3] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BaseTokenKeyword, textSpan));
                    return;
                }
            
                goto default;
            case 428: // bool
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'b' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BoolTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 436: // !! DUPLICATES !!
            
                if (textSpan.Length != 4)
                    goto default;
            
                if (keywordCheckBuffer[0] == 'b' &&
                    keywordCheckBuffer[1] == 'y' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'e')
                {
                    // byte
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByteTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'f' &&
                         keywordCheckBuffer[1] == 'r' &&
                         keywordCheckBuffer[2] == 'o' &&
                         keywordCheckBuffer[3] == 'm')
                {
                    // from
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FromTokenContextualKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'i' &&
                         keywordCheckBuffer[1] == 'n' &&
                         keywordCheckBuffer[2] == 'i' &&
                         keywordCheckBuffer[3] == 't')
                {
                    // init
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InitTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 515: // catch
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'c' &&
                    keywordCheckBuffer[4] == 'h' &&)
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CatchTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 414: // char
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 'r')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 711: // checked
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'c' &&
                    keywordCheckBuffer[4] == 'k' &&
                    keywordCheckBuffer[5] == 'e' &&
                    keywordCheckBuffer[6] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CheckedTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 534: // !! DUPLICATES !!
                
                if (textSpan.Length != 5)
                    goto default;

                if (keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'l' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 's' &&
                    keywordCheckBuffer[4] == 's')
                {
                    // class
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ClassTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'f' &&
                         keywordCheckBuffer[1] == 'l' &&
                         keywordCheckBuffer[2] == 'o' &&
                         keywordCheckBuffer[3] == 'a' &&
                         keywordCheckBuffer[4] == 't')
                {
                    // float
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FloatTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'a' &&
                         keywordCheckBuffer[1] == 'w' &&
                         keywordCheckBuffer[2] == 'a' &&
                         keywordCheckBuffer[3] == 'i' &&
                         keywordCheckBuffer[4] == 't')
                {
                    // await
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AwaitTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 551: // !! DUPLICATES !!
                
                if (textSpan.Length != 5
                    goto default;

                if (keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'n' &&
                    keywordCheckBuffer[3] == 's' &&
                    keywordCheckBuffer[4] == 't')
                {
                    // const
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ConstTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 's' &&
                         keywordCheckBuffer[1] == 'b' &&
                         keywordCheckBuffer[2] == 'y' &&
                         keywordCheckBuffer[3] == 't' &&
                         keywordCheckBuffer[4] == 'e')
                {
                    // sbyte
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SbyteTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 719: // decimal
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'c' &&
                    keywordCheckBuffer[3] == 'i' &&
                    keywordCheckBuffer[4] == 'm' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DecimalTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 741: // !! DUPLICATES !!
                
                if (textSpan.Length != 7)
                    goto default;

                if (keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'f' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'u' &&
                    keywordCheckBuffer[5] == 'l' &&
                    keywordCheckBuffer[6] == 't')
                {
                    // default
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DefaultTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'd' &&
                         keywordCheckBuffer[1] == 'y' &&
                         keywordCheckBuffer[2] == 'n' &&
                         keywordCheckBuffer[3] == 'a' &&
                         keywordCheckBuffer[4] == 'm' &&
                         keywordCheckBuffer[5] == 'i' &&
                         keywordCheckBuffer[6] == 'c')
                {
                    // dynamic
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DynamicTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 827: // delegate
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'g' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 't' &&
                    keywordCheckBuffer[7] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DelegateTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 635: // double
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'u' &&
                    keywordCheckBuffer[3] == 'b' &&
                    keywordCheckBuffer[4] == 'l' &&
                    keywordCheckBuffer[5] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoubleTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 437: // enum
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'e' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 'u' &&
                    keywordCheckBuffer[3] == 'm')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EnumTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 546: // event
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'e' &&
                    keywordCheckBuffer[1] == 'v' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EventTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 866: // explicit
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'e' &&
                    keywordCheckBuffer[1] == 'x' &&
                    keywordCheckBuffer[2] == 'p' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'c' &&
                    keywordCheckBuffer[6] == 'i' &&
                    keywordCheckBuffer[7] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExplicitTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 662: // extern
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'e' &&
                    keywordCheckBuffer[1] == 'x' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'n')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExternTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 523: // false
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 's' &&
                    keywordCheckBuffer[4] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FalseTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 751: // finally
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'n' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'l' &&
                    keywordCheckBuffer[5] == 'l' &&
                    keywordCheckBuffer[6] == 'y')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FinallyTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 528: // fixed
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'x' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FixedTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 859: // implicit
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'm' &&
                    keywordCheckBuffer[2] == 'p' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'c' &&
                    keywordCheckBuffer[6] == 'i' &&
                    keywordCheckBuffer[7] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ImplicitTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 215: // in
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'n')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 331: // int
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 945: // interface
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'f' &&
                    keywordCheckBuffer[6] == 'a' &&
                    keywordCheckBuffer[7] == 'c' &&
                    keywordCheckBuffer[8] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InterfaceTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 861: // internal
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'n' &&
                    keywordCheckBuffer[6] == 'a' &&
                    keywordCheckBuffer[7] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InternalTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 220: // is
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IsTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 425: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (keywordCheckBuffer[0] == 'l' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'c' &&
                    keywordCheckBuffer[3] == 'k')
                {
                    // lock
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LockTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'e' &&
                         keywordCheckBuffer[1] == 'l' &&
                         keywordCheckBuffer[2] == 's' &&
                         keywordCheckBuffer[3] == 'e')
                {
                    // else
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ElseTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 432: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (keywordCheckBuffer[0] == 'l' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'n' &&
                    keywordCheckBuffer[3] == 'g')
                {
                    // long
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LongTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'j' &&
                         keywordCheckBuffer[1] == 'o' &&
                         keywordCheckBuffer[2] == 'i' &&
                         keywordCheckBuffer[3] == 'n')
                {
                    // join
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.JoinTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 941: // namespace
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'm' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 's' &&
                    keywordCheckBuffer[5] == 'p' &&
                    keywordCheckBuffer[6] == 'a' &&
                    keywordCheckBuffer[7] == 'c' &&
                    keywordCheckBuffer[8] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NamespaceTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 330: // new
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'w')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NewTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 443: // null
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'u' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NullTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 631: // object
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'b' &&
                    keywordCheckBuffer[2] == 'j' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'c' &&
                    keywordCheckBuffer[5] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ObjectTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 876: // operator
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'p' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'r' &&
                    keywordCheckBuffer[4] == 'a' &&
                    keywordCheckBuffer[5] == 't' &&
                    keywordCheckBuffer[6] == 'o' &&
                    keywordCheckBuffer[7] == 'r')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OperatorTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 344: // out
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'u' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OutTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 864: // !! DUPLICATES !!
                
                if (textSpan.Length != 8)
                    goto default;

                if (keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'v' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'r' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'i' &&
                    keywordCheckBuffer[6] == 'd' &&
                    keywordCheckBuffer[7] == 'e')
                {
                    // override
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OverrideTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'v' &&
                         keywordCheckBuffer[1] == 'o' &&
                         keywordCheckBuffer[2] == 'l' &&
                         keywordCheckBuffer[3] == 'a' &&
                         keywordCheckBuffer[4] == 't' &&
                         keywordCheckBuffer[5] == 'i' &&
                         keywordCheckBuffer[6] == 'l' &&
                         keywordCheckBuffer[7] == 'e')
                {
                    // volatile
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VolatileTokenKeyword, textSpan));
                    return;
                }
                    
                goto default;
            case 644: // params
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'p' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'm' &&
                    keywordCheckBuffer[5] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ParamsTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 763: // private
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'p' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'v' &&
                    keywordCheckBuffer[4] == 'a' &&
                    keywordCheckBuffer[5] == 't' &&
                    keywordCheckBuffer[6] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PrivateTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 970: // protected
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'p' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'e' &&
                    keywordCheckBuffer[5] == 'c' &&
                    keywordCheckBuffer[6] == 't' &&
                    keywordCheckBuffer[7] == 'e' &&
                    keywordCheckBuffer[8] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ProtectedTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 639: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (keywordCheckBuffer[0] == 'p' &&
                    keywordCheckBuffer[1] == 'u' &&
                    keywordCheckBuffer[2] == 'b' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'c')
                {
                    // public
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PublicTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'r' &&
                         keywordCheckBuffer[1] == 'e' &&
                         keywordCheckBuffer[2] == 'c' &&
                         keywordCheckBuffer[3] == 'o' &&
                         keywordCheckBuffer[4] == 'r' &&
                         keywordCheckBuffer[5] == 'd')
                {
                    // record
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RecordTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 862: // readonly
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'r' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 'd' &&
                    keywordCheckBuffer[4] == 'o' &&
                    keywordCheckBuffer[5] == 'n' &&
                    keywordCheckBuffer[6] == 'l' &&
                    keywordCheckBuffer[7] == 'y')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReadonlyTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 317: // ref
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'r' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'f')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RefTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 622: // sealed
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'e' &&
                    keywordCheckBuffer[5] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SealedTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 560: // short
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'r' &&
                    keywordCheckBuffer[4] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ShortTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 656: // sizeof
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'z' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'o' &&
                    keywordCheckBuffer[5] == 'f')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SizeofTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 1057: // stackalloc
                if (textSpan.Length == 10 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 't' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 'c' &&
                    keywordCheckBuffer[4] == 'k' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'l' &&
                    keywordCheckBuffer[7] == 'l' &&
                    keywordCheckBuffer[8] == 'o' &&
                    keywordCheckBuffer[9] == 'c')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StackallocTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 648: // static
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 't' &&
                    keywordCheckBuffer[2] == 'a' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'c')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StaticTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 663: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 't' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 'i' &&
                    keywordCheckBuffer[4] == 'n' &&
                    keywordCheckBuffer[5] == 'g')
                {
                    // string
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 't' &&
                         keywordCheckBuffer[1] == 'y' &&
                         keywordCheckBuffer[2] == 'p' &&
                         keywordCheckBuffer[3] == 'e' &&
                         keywordCheckBuffer[4] == 'o' &&
                         keywordCheckBuffer[5] == 'f')
                {
                    // typeof
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TypeofTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 677: // !! DUPLICATES !!
                
                if (textSpan.Length != 6)
                    goto default;

                if (keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 't' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 'u' &&
                    keywordCheckBuffer[4] == 'c' &&
                    keywordCheckBuffer[5] == 't')
                {
                    // struct
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StructTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'u' &&
                         keywordCheckBuffer[1] == 's' &&
                         keywordCheckBuffer[2] == 'h' &&
                         keywordCheckBuffer[3] == 'o' &&
                         keywordCheckBuffer[4] == 'r' &&
                         keywordCheckBuffer[5] == 't')
                {
                    // ushort
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UshortTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 440: // this
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 't' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThisTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 448: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (keywordCheckBuffer[0] == 't' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'u' &&
                    keywordCheckBuffer[3] == 'e')
                {
                    // true
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TrueTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'u' &&
                         keywordCheckBuffer[1] == 'i' &&
                         keywordCheckBuffer[2] == 'n' &&
                         keywordCheckBuffer[3] == 't')
                {
                    // uint
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UintTokenKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 351: // try
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 't' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'y')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TryTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 549: // ulong
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'u' &&
                    keywordCheckBuffer[1] == 'l' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 'g')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UlongTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 938: // unchecked
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'u' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 'c' &&
                    keywordCheckBuffer[3] == 'h' &&
                    keywordCheckBuffer[4] == 'e' &&
                    keywordCheckBuffer[5] == 'c' &&
                    keywordCheckBuffer[6] == 'k' &&
                    keywordCheckBuffer[7] == 'e' &&
                    keywordCheckBuffer[8] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UncheckedTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 642: // unsafe
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'u' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 's' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'f' &&
                    keywordCheckBuffer[5] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnsafeTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 550: // using
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'u' &&
                    keywordCheckBuffer[1] == 's' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 'g')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UsingTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 775: // virtual
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'v' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'u' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VirtualTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 434: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (keywordCheckBuffer[0] == 'v' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'd')
                {
                    // void
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VoidTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'w' &&
                         keywordCheckBuffer[1] == 'h' &&
                         keywordCheckBuffer[2] == 'e' &&
                         keywordCheckBuffer[3] == 'n')
                {
                    // when
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhenTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 517: // break
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'b' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'k')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BreakTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 412: // case
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 's' &&
                    keywordCheckBuffer[3] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CaseTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 869: // continue
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'c' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'n' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'n' &&
                    keywordCheckBuffer[6] == 'u' &&
                    keywordCheckBuffer[7] == 'e' &&)
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ContinueTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 211: // do
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'o')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 327: // for
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'r')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 728: // foreach
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'a' &&
                    keywordCheckBuffer[5] == 'c' &&
                    keywordCheckBuffer[6] == 'h')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForeachTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 441: // !! DUPLICATES !!
                
                if (textSpan.Length != 4)
                    goto default;

                if (keywordCheckBuffer[0] == 'g' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'o')
                {
                    // goto
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GotoTokenKeyword, textSpan));
                    return;
                }
                else if (keywordCheckBuffer[0] == 'n' &&
                         keywordCheckBuffer[1] == 'i' &&
                         keywordCheckBuffer[2] == 'n' &&
                         keywordCheckBuffer[3] == 't')
                {
                    // nint
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NintTokenContextualKeyword, textSpan));
                    return;
                }
                
                goto default;
            case 207: // if
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'f')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IfTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 672: // return
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'r' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'u' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'n')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReturnTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 658: // switch
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'w' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'c' &&
                    keywordCheckBuffer[5] == 'h')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SwitchTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 564: // throw
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 't' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 'o' &&
                    keywordCheckBuffer[4] == 'w')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThrowTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 537: // while
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'w' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhileTokenKeyword, textSpan));
                    return;
                }

                goto default;
            case 297: // add
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 'd' &&
                    keywordCheckBuffer[2] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AddTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 307: // and
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AndTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 522: // alias
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 'l' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AliasTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 940: // ascending
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 's' &&
                    keywordCheckBuffer[2] == 'c' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'n' &&
                    keywordCheckBuffer[5] == 'd' &&
                    keywordCheckBuffer[6] == 'i' &&
                    keywordCheckBuffer[7] == 'n' &&
                    keywordCheckBuffer[8] == 'g')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AscendingTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 429: // args
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'g' &&
                    keywordCheckBuffer[3] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ArgsTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 542: // async
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'a' &&
                    keywordCheckBuffer[1] == 's' &&
                    keywordCheckBuffer[2] == 'y' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 'c')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsyncTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 219: // by
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'b' &&
                    keywordCheckBuffer[1] == 'y')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 1044: // descending
                if (textSpan.Length == 10 &&
                    keywordCheckBuffer[0] == 'd' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 's' &&
                    keywordCheckBuffer[3] == 'c' &&
                    keywordCheckBuffer[4] == 'e' &&
                    keywordCheckBuffer[5] == 'n' &&
                    keywordCheckBuffer[6] == 'd' &&
                    keywordCheckBuffer[7] == 'i' &&
                    keywordCheckBuffer[8] == 'n' &&
                    keywordCheckBuffer[9] == 'g')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DescendingTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 651: // equals
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'e' &&
                    keywordCheckBuffer[1] == 'q' &&
                    keywordCheckBuffer[2] == 'u' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'l' &&
                    keywordCheckBuffer[5] == 's')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 416: // file
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'f' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FileTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 320: // get
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'g' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GetTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 625: // global
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'g' &&
                    keywordCheckBuffer[1] == 'l' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'b' &&
                    keywordCheckBuffer[4] == 'a' &&
                    keywordCheckBuffer[5] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GlobalTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 557: // group
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'g' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'u' &&
                    keywordCheckBuffer[4] == 'p')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GroupTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 442: // into
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'i' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'o')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntoTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 325: // let
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'l' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LetTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 717: // managed
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'm' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'n' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'g' &&
                    keywordCheckBuffer[5] == 'e' &&
                    keywordCheckBuffer[6] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ManagedTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 630: // nameof
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'm' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'o' &&
                    keywordCheckBuffer[5] == 'f')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NameofTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 337: // not
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 780: // notnull
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'o' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 'u' &&
                    keywordCheckBuffer[5] == 'l' &&
                    keywordCheckBuffer[6] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotnullTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 558: // nuint
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'n' &&
                    keywordCheckBuffer[1] == 'u' &&
                    keywordCheckBuffer[2] == 'i' &&
                    keywordCheckBuffer[3] == 'n' &&
                    keywordCheckBuffer[4] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NuintTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 221: // on
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'n')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OnTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 225: // or
                if (textSpan.Length == 2 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'r')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 759: // orderby
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'o' &&
                    keywordCheckBuffer[1] == 'r' &&
                    keywordCheckBuffer[2] == 'd' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'r' &&
                    keywordCheckBuffer[5] == 'b' &&
                    keywordCheckBuffer[6] == 'y')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrderbyTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 749: // partial
                if (textSpan.Length == 7 &&
                    keywordCheckBuffer[0] == 'p' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'r' &&
                    keywordCheckBuffer[3] == 't' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'l')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PartialTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 654: // remove
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 'r' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'm' &&
                    keywordCheckBuffer[3] == 'o' &&
                    keywordCheckBuffer[4] == 'v' &&
                    keywordCheckBuffer[5] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RemoveTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 865: // required
                if (textSpan.Length == 8 &&
                    keywordCheckBuffer[0] == 'r' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'q' &&
                    keywordCheckBuffer[3] == 'u' &&
                    keywordCheckBuffer[4] == 'i' &&
                    keywordCheckBuffer[5] == 'r' &&
                    keywordCheckBuffer[6] == 'e' &&
                    keywordCheckBuffer[7] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RequiredTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 638: // scoped
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'c' &&
                    keywordCheckBuffer[2] == 'o' &&
                    keywordCheckBuffer[3] == 'p' &&
                    keywordCheckBuffer[4] == 'e' &&
                    keywordCheckBuffer[5] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ScopedTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 640: // select
                if (textSpan.Length == 6 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 'e' &&
                    keywordCheckBuffer[4] == 'c' &&
                    keywordCheckBuffer[5] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SelectTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 332: // set
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 's' &&
                    keywordCheckBuffer[1] == 'e' &&
                    keywordCheckBuffer[2] == 't')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SetTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 944: // unmanaged
                if (textSpan.Length == 9 &&
                    keywordCheckBuffer[0] == 'u' &&
                    keywordCheckBuffer[1] == 'n' &&
                    keywordCheckBuffer[2] == 'm' &&
                    keywordCheckBuffer[3] == 'a' &&
                    keywordCheckBuffer[4] == 'n' &&
                    keywordCheckBuffer[5] == 'a' &&
                    keywordCheckBuffer[6] == 'g' &&
                    keywordCheckBuffer[7] == 'e' &&
                    keywordCheckBuffer[8] == 'd' &&)
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnmanagedTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 541: // value
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'v' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'l' &&
                    keywordCheckBuffer[3] == 'u' &&
                    keywordCheckBuffer[4] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ValueTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 329: // var
                if (textSpan.Length == 3 &&
                    keywordCheckBuffer[0] == 'v' &&
                    keywordCheckBuffer[1] == 'a' &&
                    keywordCheckBuffer[2] == 'r')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VarTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 539: // where
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'w' &&
                    keywordCheckBuffer[1] == 'h' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'r' &&
                    keywordCheckBuffer[4] == 'e')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhereTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 444: // with
                if (textSpan.Length == 4 &&
                    keywordCheckBuffer[0] == 'w' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 't' &&
                    keywordCheckBuffer[3] == 'h')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WithTokenContextualKeyword, textSpan));
                    return;
                }

                goto default;
            case 535: // yield
                if (textSpan.Length == 5 &&
                    keywordCheckBuffer[0] == 'y' &&
                    keywordCheckBuffer[1] == 'i' &&
                    keywordCheckBuffer[2] == 'e' &&
                    keywordCheckBuffer[3] == 'l' &&
                    keywordCheckBuffer[4] == 'd')
                {
                    textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.YieldTokenContextualKeyword, textSpan));
                    return;
                }

                    
            default:
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
                return;
        }
        
        switch (binder.CSharpCompilerService.SafeGetText(lexerOutput.ResourceUri.Value, textSpan) ?? string.Empty)
        {
            // NonContextualKeywords-NonControl
            // ================================
            case "abstract":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AbstractTokenKeyword, textSpan));
                return;
            case "as":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsTokenKeyword, textSpan));
                return;
            case "base":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BaseTokenKeyword, textSpan));
                return;
            case "bool":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BoolTokenKeyword, textSpan));
                return;
            case "byte":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByteTokenKeyword, textSpan));
                return;
            case "catch":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CatchTokenKeyword, textSpan));
                return;
            case "char":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharTokenKeyword, textSpan));
                return;
            case "checked":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CheckedTokenKeyword, textSpan));
                return;
            case "class":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ClassTokenKeyword, textSpan));
                return;
            case "const":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ConstTokenKeyword, textSpan));
                return;
            case "decimal":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DecimalTokenKeyword, textSpan));
                return;
            case "default":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DefaultTokenKeyword, textSpan));
                return;
            case "delegate":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DelegateTokenKeyword, textSpan));
                return;
            case "double":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoubleTokenKeyword, textSpan));
                return;
            case "enum":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EnumTokenKeyword, textSpan));
                return;
            case "event":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EventTokenKeyword, textSpan));
                return;
            case "explicit":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExplicitTokenKeyword, textSpan));
                return;
            case "extern":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExternTokenKeyword, textSpan));
                return;
            case "false":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FalseTokenKeyword, textSpan));
                return;
            case "finally":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FinallyTokenKeyword, textSpan));
                return;
            case "fixed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FixedTokenKeyword, textSpan));
                return;
            case "float":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FloatTokenKeyword, textSpan));
                return;
            case "implicit":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ImplicitTokenKeyword, textSpan));
                return;
            case "in":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InTokenKeyword, textSpan));
                return;
            case "int":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntTokenKeyword, textSpan));
                return;
            case "interface":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InterfaceTokenKeyword, textSpan));
                return;
            case "internal":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InternalTokenKeyword, textSpan));
                return;
            case "is":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IsTokenKeyword, textSpan));
                return;
            case "lock":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LockTokenKeyword, textSpan));
                return;
            case "long":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LongTokenKeyword, textSpan));
                return;
            case "namespace":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NamespaceTokenKeyword, textSpan));
                return;
            case "new":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NewTokenKeyword, textSpan));
                return;
            case "null":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NullTokenKeyword, textSpan));
                return;
            case "object":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ObjectTokenKeyword, textSpan));
                return;
            case "operator":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OperatorTokenKeyword, textSpan));
                return;
            case "out":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OutTokenKeyword, textSpan));
                return;
            case "override":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OverrideTokenKeyword, textSpan));
                return;
            case "params":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ParamsTokenKeyword, textSpan));
                return;
            case "private":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PrivateTokenKeyword, textSpan));
                return;
            case "protected":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ProtectedTokenKeyword, textSpan));
                return;
            case "public":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PublicTokenKeyword, textSpan));
                return;
            case "readonly":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReadonlyTokenKeyword, textSpan));
                return;
            case "ref":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RefTokenKeyword, textSpan));
                return;
            case "sbyte":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SbyteTokenKeyword, textSpan));
                return;
            case "sealed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SealedTokenKeyword, textSpan));
                return;
            case "short":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ShortTokenKeyword, textSpan));
                return;
            case "sizeof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SizeofTokenKeyword, textSpan));
                return;
            case "stackalloc":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StackallocTokenKeyword, textSpan));
                return;
            case "static":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StaticTokenKeyword, textSpan));
                return;
            case "string":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringTokenKeyword, textSpan));
                return;
            case "struct":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StructTokenKeyword, textSpan));
                return;
            case "this":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThisTokenKeyword, textSpan));
                return;
            case "true":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TrueTokenKeyword, textSpan));
                return;
            case "try":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TryTokenKeyword, textSpan));
                return;
            case "typeof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TypeofTokenKeyword, textSpan));
                return;
            case "uint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UintTokenKeyword, textSpan));
                return;
            case "ulong":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UlongTokenKeyword, textSpan));
                return;
            case "unchecked":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UncheckedTokenKeyword, textSpan));
                return;
            case "unsafe":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnsafeTokenKeyword, textSpan));
                return;
            case "ushort":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UshortTokenKeyword, textSpan));
                return;
            case "using":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UsingTokenKeyword, textSpan));
                return;
            case "virtual":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VirtualTokenKeyword, textSpan));
                return;
            case "void":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VoidTokenKeyword, textSpan));
                return;
            case "volatile":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VolatileTokenKeyword, textSpan));
                return;
            // NonContextualKeywords-IsControl
            // ===============================
            case "break":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BreakTokenKeyword, textSpan));
                return;
            case "case":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CaseTokenKeyword, textSpan));
                return;
            case "continue":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ContinueTokenKeyword, textSpan));
                return;
            case "do":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoTokenKeyword, textSpan));
                return;
            case "else":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ElseTokenKeyword, textSpan));
                return;
            case "for":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForTokenKeyword, textSpan));
                return;
            case "foreach":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForeachTokenKeyword, textSpan));
                return;
            case "goto":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GotoTokenKeyword, textSpan));
                return;
            case "if":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IfTokenKeyword, textSpan));
                return;
            case "return":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReturnTokenKeyword, textSpan));
                return;
            case "switch":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SwitchTokenKeyword, textSpan));
                return;
            case "throw":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThrowTokenKeyword, textSpan));
                return;
            case "while":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhileTokenKeyword, textSpan));
                return;
            // ContextualKeywords-NotControl
            // =============================
            case "add":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AddTokenContextualKeyword, textSpan));
                return;
            case "and":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AndTokenContextualKeyword, textSpan));
                return;
            case "alias":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AliasTokenContextualKeyword, textSpan));
                return;
            case "ascending":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AscendingTokenContextualKeyword, textSpan));
                return;
            case "args":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ArgsTokenContextualKeyword, textSpan));
                return;
            case "async":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsyncTokenContextualKeyword, textSpan));
                return;
            case "await":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AwaitTokenContextualKeyword, textSpan));
                return;
            case "by":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByTokenContextualKeyword, textSpan));
                return;
            case "descending":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DescendingTokenContextualKeyword, textSpan));
                return;
            case "dynamic":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DynamicTokenContextualKeyword, textSpan));
                return;
            case "equals":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsTokenContextualKeyword, textSpan));
                return;
            case "file":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FileTokenContextualKeyword, textSpan));
                return;
            case "from":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FromTokenContextualKeyword, textSpan));
                return;
            case "get":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GetTokenContextualKeyword, textSpan));
                return;
            case "global":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GlobalTokenContextualKeyword, textSpan));
                return;
            case "group":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GroupTokenContextualKeyword, textSpan));
                return;
            case "init":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InitTokenContextualKeyword, textSpan));
                return;
            case "into":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntoTokenContextualKeyword, textSpan));
                return;
            case "join":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.JoinTokenContextualKeyword, textSpan));
                return;
            case "let":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LetTokenContextualKeyword, textSpan));
                return;
            case "managed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ManagedTokenContextualKeyword, textSpan));
                return;
            case "nameof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NameofTokenContextualKeyword, textSpan));
                return;
            case "nint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NintTokenContextualKeyword, textSpan));
                return;
            case "not":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotTokenContextualKeyword, textSpan));
                return;
            case "notnull":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotnullTokenContextualKeyword, textSpan));
                return;
            case "nuint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NuintTokenContextualKeyword, textSpan));
                return;
            case "on":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OnTokenContextualKeyword, textSpan));
                return;
            case "or":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrTokenContextualKeyword, textSpan));
                return;
            case "orderby":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrderbyTokenContextualKeyword, textSpan));
                return;
            case "partial":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PartialTokenContextualKeyword, textSpan));
                return;
            case "record":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RecordTokenContextualKeyword, textSpan));
                return;
            case "remove":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RemoveTokenContextualKeyword, textSpan));
                return;
            case "required":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RequiredTokenContextualKeyword, textSpan));
                return;
            case "scoped":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ScopedTokenContextualKeyword, textSpan));
                return;
            case "select":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SelectTokenContextualKeyword, textSpan));
                return;
            case "set":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SetTokenContextualKeyword, textSpan));
                return;
            case "unmanaged":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnmanagedTokenContextualKeyword, textSpan));
                return;
            case "value":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ValueTokenContextualKeyword, textSpan));
                return;
            case "var":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VarTokenContextualKeyword, textSpan));
                return;
            case "when":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhenTokenContextualKeyword, textSpan));
                return;
            case "where":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhereTokenContextualKeyword, textSpan));
                return;
            case "with":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WithTokenContextualKeyword, textSpan));
                return;
            // ContextualKeywords-IsControl
            // ============================
            case "yield":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.YieldTokenContextualKeyword, textSpan));
                return;
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
