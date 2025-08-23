using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Json;

public static class JsonLexer
{
    public enum CssLexerContextKind
    {
        Expect_PropertyName,
        Expect_PropertyValue,
    }

    public static JsonLexerOutput Lex(StreamReaderWrap streamReaderWrap)
    {
        var context = CssLexerContextKind.Expect_PropertyName;
        var output = new JsonLexerOutput();
        
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
                    
                    output.TextSpanList.Add(new TextEditorTextSpan(
                        textStartPosition,
                        streamReaderWrap.PositionIndex,
                        (byte)GenericDecorationKind.Json_Keyword,
                        textStartByte));
                    goto default;
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
                    var stringStartPosition = streamReaderWrap.PositionIndex;
                    var stringStartByte = streamReaderWrap.ByteIndex;
                    _ = streamReaderWrap.ReadCharacter();
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter == '\\' &&
                            streamReaderWrap.PeekCharacter(1) == '"')
                        {
                            _ = streamReaderWrap.ReadCharacter();
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        else if (streamReaderWrap.CurrentCharacter == '"')
                        {
                            _ = streamReaderWrap.ReadCharacter();
                            break;
                        }
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    
                    var endPosition = streamReaderWrap.PositionIndex;
                    
                    while (!streamReaderWrap.IsEof)
                    {
                        if (!char.IsWhiteSpace(streamReaderWrap.CurrentCharacter))
                        {
                            break;
                        }
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    
                    if (streamReaderWrap.CurrentCharacter == ':')
                    {
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            stringStartPosition,
                            endPosition,
                            (byte)GenericDecorationKind.Json_PropertyKey,
                            stringStartByte));
                    }
                    else
                    {
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            stringStartPosition,
                            endPosition,
                            (byte)GenericDecorationKind.Json_String,
                            stringStartByte));
                    }
                    continue;
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
                            (byte)GenericDecorationKind.Json_BlockComment,
                            commentStartByte));
                        continue;
                    }
                
                    if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        var commentStartPosition = streamReaderWrap.PositionIndex;
                        var commentStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        _ = streamReaderWrap.ReadCharacter();
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '\r' ||
                                streamReaderWrap.CurrentCharacter == '\n')
                            {
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            commentStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Json_LineComment,
                            commentStartByte));
                        continue;
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
                    goto default;
                }
                case '(':
                {
                    goto default;
                }
                case ')':
                {
                    goto default;
                }
                case '{':
                {
                    goto default;
                }
                case '}':
                {
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
                    goto default;
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
