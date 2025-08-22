using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Css;

public static class CssLexer
{
    public enum CssLexerContextKind
    {
        Expect_PropertyName,
        Expect_PropertyValue,
    }
    
    public static CssLexerOutput Lex(StreamReaderWrap streamReaderWrap)
    {
        var context = CssLexerContextKind.Expect_PropertyName;
        var output = new CssLexerOutput();
        
        var braceMatching = 0;
        
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
                    var textStartPosition = streamReaderWrap.PositionIndex;
                    var textStartByte = streamReaderWrap.ByteIndex;
                    while (!streamReaderWrap.IsEof)
                    {
                        if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter))
                        {
                            if (streamReaderWrap.CurrentCharacter != '_' &&
                                streamReaderWrap.CurrentCharacter != '-')
                            {
                                break;
                            }
                        }
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    
                    if (streamReaderWrap.CurrentCharacter == '(')
                    {
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            textStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)CssDecorationKind.Function,
                            textStartByte));
                    }
                    else if (braceMatching == 0)
                    {
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            textStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)CssDecorationKind.Identifier,
                            textStartByte));
                    }
                    else
                    {
                        if (context == CssLexerContextKind.Expect_PropertyName)
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                textStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)CssDecorationKind.PropertyName,
                                textStartByte));
                        }
                        else
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                textStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)CssDecorationKind.PropertyValue,
                                textStartByte));
                        }
                    }
                    continue;
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
                    goto default;
                case '\'':
                    goto default;
                case '"':
                    goto default;
                case '/':
                    if (streamReaderWrap.PeekCharacter(1) == '*')
                    {
                        var commentStartPosition = streamReaderWrap.PositionIndex;
                        var commentStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        _ = streamReaderWrap.ReadCharacter();
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '*' &&
                                streamReaderWrap.PeekCharacter(1) == '/')
                            {
                                _ = streamReaderWrap.ReadCharacter();
                                _ = streamReaderWrap.ReadCharacter();
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            commentStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)CssDecorationKind.Comment,
                            commentStartByte));
                        continue;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '+':
                    if (streamReaderWrap.PeekCharacter(1) == '+')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '-':
                    if (streamReaderWrap.PeekCharacter(1) == '-')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '=':
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '?':
                    if (streamReaderWrap.PeekCharacter(1) == '?')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '|':
                    if (streamReaderWrap.PeekCharacter(1) == '|')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case '&':
                    if (streamReaderWrap.PeekCharacter(1) == '&')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case '*':
                {
                    goto default;
                }
                case '!':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case ';':
                {
                    context = CssLexerContextKind.Expect_PropertyName;
                    goto default;
                }
                case '(':
                {
                    _ = streamReaderWrap.ReadCharacter();
                    
                    var positionIndex = streamReaderWrap.PositionIndex;
                    var byteIndex = streamReaderWrap.ByteIndex;
                    
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter == ')')
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                positionIndex,
                                streamReaderWrap.PositionIndex,
                                (byte)CssDecorationKind.None,
                                byteIndex));
                            _ = streamReaderWrap.ReadCharacter();
                            break;
                        }
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    continue;
                }
                case ')':
                {
                    goto default;
                }
                case '{':
                {
                    ++braceMatching;
                    context = CssLexerContextKind.Expect_PropertyName;
                    goto default;
                }
                case '}':
                {
                    --braceMatching;
                    goto default;
                }
                case '<':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case '>':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case '[':
                {
                    goto default;
                }
                case ']':
                {
                    goto default;
                }
                case '$':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '@' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.NextCharacter == '$')
                    {
                        /*var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
    
                        // The while loop starts counting from and including the first dollar sign.
                        var countDollarSign = 0;
                    
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter != '$')
                                break;
                            
                            ++countDollarSign;
                            _ = streamReaderWrap.ReadCharacter();
                        }*/
                        
                        goto default;
                        
                        /*if (streamReaderWrap.NextCharacter == '"')
                            LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: countDollarSign, useVerbatim: false);*/
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '@':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '$' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case ':':
                {
                    if (streamReaderWrap.PeekCharacter(1) == ':')
                    {
                        context = CssLexerContextKind.Expect_PropertyName;
                        _ = streamReaderWrap.ReadCharacter();
                        _ = streamReaderWrap.ReadCharacter();
                        continue;
                    }
                    else
                    {
                        context = CssLexerContextKind.Expect_PropertyValue;
                        goto default;
                    }
                }
                case '.':
                {
                    goto default;
                }
                case ',':
                {
                    goto default;
                }
                case '#':
                    if (context == CssLexerContextKind.Expect_PropertyValue)
                    {
                        _ = streamReaderWrap.ReadCharacter();
                        
                        var startPosition = streamReaderWrap.PositionIndex;
                        var startByte = streamReaderWrap.ByteIndex;
                        
                        while (!streamReaderWrap.IsEof)
                        {
                            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter))
                            {
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            startPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)CssDecorationKind.PropertyValue,
                            startByte));
                        continue;
                    }
                    goto default;
                default:
                    _ = streamReaderWrap.ReadCharacter();
                    break;
            }
        }
    
        forceExit:
        return output;
    }
}
