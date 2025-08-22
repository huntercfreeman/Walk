using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.JavaScript;

public static class JavaScriptLexer
{
    public static JavaScriptLexerOutput Lex(JavaScriptCompilerService javaScriptCompilerService, StreamReaderWrap streamReaderWrap)
    {
        var output = new JavaScriptLexerOutput();
        
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
                    LexIdentifierOrKeywordOrKeywordContextual(javaScriptCompilerService, output.TextSpanList, streamReaderWrap);
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
                    goto default;
                case '\'':
                {
                    var stringStartPosition = streamReaderWrap.PositionIndex;
                    var stringStartByte = streamReaderWrap.ByteIndex;
                    _ = streamReaderWrap.ReadCharacter();
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter == '\\' &&
                            streamReaderWrap.PeekCharacter(1) == '\'')
                        {
                            _ = streamReaderWrap.ReadCharacter();
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        else if (streamReaderWrap.CurrentCharacter == '\'')
                        {
                            _ = streamReaderWrap.ReadCharacter();
                            break;
                        }
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    
                    output.TextSpanList.Add(new TextEditorTextSpan(
                        stringStartPosition,
                        streamReaderWrap.PositionIndex,
                        (byte)GenericDecorationKind.StringLiteral,
                        stringStartByte));
                    continue;
                }
                case '"':
                {
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
                    
                    output.TextSpanList.Add(new TextEditorTextSpan(
                        stringStartPosition,
                        streamReaderWrap.PositionIndex,
                        (byte)GenericDecorationKind.StringLiteral,
                        stringStartByte));
                    continue;
                }
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
                            (byte)GenericDecorationKind.CommentSingleLine,
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
                            (byte)GenericDecorationKind.CommentMultiLine,
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
                    /*if (interpolatedExpressionUnmatchedBraceCount != -1)
                        ++interpolatedExpressionUnmatchedBraceCount;*/
                
                    goto default;
                }
                case '}':
                {
                    /*if (interpolatedExpressionUnmatchedBraceCount != -1)
                    {
                        if (--interpolatedExpressionUnmatchedBraceCount <= 0)
                            goto forceExit;
                    }*/
                
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
    
    public static void LexIdentifierOrKeywordOrKeywordContextual(
        JavaScriptCompilerService compilerService,
        List<TextEditorTextSpan> textSpanList,
        StreamReaderWrap streamReaderWrap)
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
                compilerService.KeywordCheckBuffer[bufferIndex++] = streamReaderWrap.CurrentCharacter;
            
            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.Variable,
            byteEntryIndex,
            characterIntSum);
        
        switch (characterIntSum)
        {
            case 534: // !! DUPLICATES !!
            
                if (textSpan.Length != 5)
                    goto default;

                if (compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'w' &&
                    compilerService.KeywordCheckBuffer[2] == 'a' &&
                    compilerService.KeywordCheckBuffer[3] == 'i' &&
                    compilerService.KeywordCheckBuffer[4] == 't')
                {
                    // await
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                else if (compilerService.KeywordCheckBuffer[0] == 'c' &&
                         compilerService.KeywordCheckBuffer[1] == 'l' &&
                         compilerService.KeywordCheckBuffer[2] == 'a' &&
                         compilerService.KeywordCheckBuffer[3] == 's' &&
                         compilerService.KeywordCheckBuffer[4] == 's')
                {
                    // class
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 517: // break
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'b' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[2] == 'e' &&
                    compilerService.KeywordCheckBuffer[3] == 'a' &&
                    compilerService.KeywordCheckBuffer[4] == 'k')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 412: // case
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[2] == 's' &&
                    compilerService.KeywordCheckBuffer[3] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 515: // catch
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[2] == 't' &&
                    compilerService.KeywordCheckBuffer[3] == 'c' &&
                    compilerService.KeywordCheckBuffer[4] == 'h')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 551: // const
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[2] == 'n' &&
                    compilerService.KeywordCheckBuffer[3] == 's' &&
                    compilerService.KeywordCheckBuffer[4] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 869: // continue
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[2] == 'n' &&
                    compilerService.KeywordCheckBuffer[3] == 't' &&
                    compilerService.KeywordCheckBuffer[4] == 'i' &&
                    compilerService.KeywordCheckBuffer[5] == 'n' &&
                    compilerService.KeywordCheckBuffer[6] == 'u' &&
                    compilerService.KeywordCheckBuffer[7] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 837: // debugger
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 'b' &&
                    compilerService.KeywordCheckBuffer[3] == 'u' &&
                    compilerService.KeywordCheckBuffer[4] == 'g' &&
                    compilerService.KeywordCheckBuffer[5] == 'g' &&
                    compilerService.KeywordCheckBuffer[6] == 'e' &&
                    compilerService.KeywordCheckBuffer[7] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 741: // default
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 'f' &&
                    compilerService.KeywordCheckBuffer[3] == 'a' &&
                    compilerService.KeywordCheckBuffer[4] == 'u' &&
                    compilerService.KeywordCheckBuffer[5] == 'l' &&
                    compilerService.KeywordCheckBuffer[6] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 627: // delete
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 'l' &&
                    compilerService.KeywordCheckBuffer[3] == 'e' &&
                    compilerService.KeywordCheckBuffer[4] == 't' &&
                    compilerService.KeywordCheckBuffer[5] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 211: // do
                if (textSpan.Length == 2 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'o')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 425: // else
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[2] == 's' &&
                    compilerService.KeywordCheckBuffer[3] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 437: // enum
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[2] == 'u' &&
                    compilerService.KeywordCheckBuffer[3] == 'm')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 674: // export
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'x' &&
                    compilerService.KeywordCheckBuffer[2] == 'p' &&
                    compilerService.KeywordCheckBuffer[3] == 'o' &&
                    compilerService.KeywordCheckBuffer[4] == 'r' &&
                    compilerService.KeywordCheckBuffer[5] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 763: // !! DUPLICATES !!
            
                if (textSpan.Length != 7)
                    goto default;

                if (compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'x'&&
                    compilerService.KeywordCheckBuffer[2] == 't'&&
                    compilerService.KeywordCheckBuffer[3] == 'e'&&
                    compilerService.KeywordCheckBuffer[4] == 'n'&&
                    compilerService.KeywordCheckBuffer[5] == 'd'&&
                    compilerService.KeywordCheckBuffer[6] == 's')
                {
                    // extends
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                else if (compilerService.KeywordCheckBuffer[0] == 'p' &&
                         compilerService.KeywordCheckBuffer[1] == 'r'&&
                         compilerService.KeywordCheckBuffer[2] == 'i'&&
                         compilerService.KeywordCheckBuffer[3] == 'v'&&
                         compilerService.KeywordCheckBuffer[4] == 'a'&&
                         compilerService.KeywordCheckBuffer[5] == 't'&&
                         compilerService.KeywordCheckBuffer[6] == 'e')
                {
                    // private
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 523: // false
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[2] == 'l' &&
                    compilerService.KeywordCheckBuffer[3] == 's' &&
                    compilerService.KeywordCheckBuffer[4] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 751: // finally
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[2] == 'n' &&
                    compilerService.KeywordCheckBuffer[3] == 'a' &&
                    compilerService.KeywordCheckBuffer[4] == 'l' &&
                    compilerService.KeywordCheckBuffer[5] == 'l' &&
                    compilerService.KeywordCheckBuffer[6] == 'y')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 327: // for
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[2] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 870: // function
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[2] == 'n' &&
                    compilerService.KeywordCheckBuffer[3] == 'c' &&
                    compilerService.KeywordCheckBuffer[4] == 't' &&
                    compilerService.KeywordCheckBuffer[5] == 'i' &&
                    compilerService.KeywordCheckBuffer[6] == 'o' &&
                    compilerService.KeywordCheckBuffer[7] == 'n')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 207: // if
                if (textSpan.Length == 2 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'f')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 1086: // implements
                if (textSpan.Length == 10 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'm' &&
                    compilerService.KeywordCheckBuffer[2] == 'p' &&
                    compilerService.KeywordCheckBuffer[3] == 'l' &&
                    compilerService.KeywordCheckBuffer[4] == 'e' &&
                    compilerService.KeywordCheckBuffer[5] == 'm' &&
                    compilerService.KeywordCheckBuffer[6] == 'e' &&
                    compilerService.KeywordCheckBuffer[7] == 'n' &&
                    compilerService.KeywordCheckBuffer[8] == 't' &&
                    compilerService.KeywordCheckBuffer[9] == 's')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 667: // import
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'm' &&
                    compilerService.KeywordCheckBuffer[2] == 'p' &&
                    compilerService.KeywordCheckBuffer[3] == 'o' &&
                    compilerService.KeywordCheckBuffer[4] == 'r' &&
                    compilerService.KeywordCheckBuffer[5] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 215: // in
                if (textSpan.Length == 2 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 1066: // instanceof
                if (textSpan.Length == 10 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[2] == 's' &&
                    compilerService.KeywordCheckBuffer[3] == 't' &&
                    compilerService.KeywordCheckBuffer[4] == 'a' &&
                    compilerService.KeywordCheckBuffer[5] == 'n' &&
                    compilerService.KeywordCheckBuffer[6] == 'c' &&
                    compilerService.KeywordCheckBuffer[7] == 'e' &&
                    compilerService.KeywordCheckBuffer[8] == 'o' &&
                    compilerService.KeywordCheckBuffer[9] == 'f')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 945: // interface
                if (textSpan.Length == 9 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[2] == 't' &&
                    compilerService.KeywordCheckBuffer[3] == 'e' &&
                    compilerService.KeywordCheckBuffer[4] == 'r' &&
                    compilerService.KeywordCheckBuffer[5] == 'f' &&
                    compilerService.KeywordCheckBuffer[6] == 'a' &&
                    compilerService.KeywordCheckBuffer[7] == 'c' &&
                    compilerService.KeywordCheckBuffer[8] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 325: // let
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 330: // new
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 'w')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 443: // null
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[2] == 'l' &&
                    compilerService.KeywordCheckBuffer[3] == 'l')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 716: // package
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[2] == 'c' &&
                    compilerService.KeywordCheckBuffer[3] == 'k' &&
                    compilerService.KeywordCheckBuffer[4] == 'a' &&
                    compilerService.KeywordCheckBuffer[5] == 'g' &&
                    compilerService.KeywordCheckBuffer[6] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 970: // protected
                if (textSpan.Length == 9 &&
                    compilerService.KeywordCheckBuffer[0] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[2] == 'o' &&
                    compilerService.KeywordCheckBuffer[3] == 't' &&
                    compilerService.KeywordCheckBuffer[4] == 'e' &&
                    compilerService.KeywordCheckBuffer[5] == 'c' &&
                    compilerService.KeywordCheckBuffer[6] == 't' &&
                    compilerService.KeywordCheckBuffer[7] == 'e' &&
                    compilerService.KeywordCheckBuffer[8] == 'd')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 639: // public
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[2] == 'b' &&
                    compilerService.KeywordCheckBuffer[3] == 'l' &&
                    compilerService.KeywordCheckBuffer[4] == 'i' &&
                    compilerService.KeywordCheckBuffer[5] == 'c')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 672: // return
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'r' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[2] == 't' &&
                    compilerService.KeywordCheckBuffer[3] == 'u' &&
                    compilerService.KeywordCheckBuffer[4] == 'r' &&
                    compilerService.KeywordCheckBuffer[5] == 'n')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 559: // super
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[2] == 'p' &&
                    compilerService.KeywordCheckBuffer[3] == 'e' &&
                    compilerService.KeywordCheckBuffer[4] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 658: // switch
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 'w' &&
                    compilerService.KeywordCheckBuffer[2] == 'i' &&
                    compilerService.KeywordCheckBuffer[3] == 't' &&
                    compilerService.KeywordCheckBuffer[4] == 'c' &&
                    compilerService.KeywordCheckBuffer[5] == 'h')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 648: // static
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[2] == 'a' &&
                    compilerService.KeywordCheckBuffer[3] == 't' &&
                    compilerService.KeywordCheckBuffer[4] == 'i' &&
                    compilerService.KeywordCheckBuffer[5] == 'c')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 440: // this
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'h' &&
                    compilerService.KeywordCheckBuffer[2] == 'i' &&
                    compilerService.KeywordCheckBuffer[3] == 's')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 564: // throw
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'h' &&
                    compilerService.KeywordCheckBuffer[2] == 'r' &&
                    compilerService.KeywordCheckBuffer[3] == 'o' &&
                    compilerService.KeywordCheckBuffer[4] == 'w')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 351: // try
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[2] == 'y')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 416: // True
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'T' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[2] == 'u' &&
                    compilerService.KeywordCheckBuffer[3] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 663: // typeof
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'y' &&
                    compilerService.KeywordCheckBuffer[2] == 'p' &&
                    compilerService.KeywordCheckBuffer[3] == 'e' &&
                    compilerService.KeywordCheckBuffer[4] == 'o' &&
                    compilerService.KeywordCheckBuffer[5] == 'f')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 329: // var
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'v' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[2] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 434: // void
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'v' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[2] == 'i' &&
                    compilerService.KeywordCheckBuffer[3] == 'd')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 537: // while
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'w' &&
                    compilerService.KeywordCheckBuffer[1] == 'h' &&
                    compilerService.KeywordCheckBuffer[2] == 'i' &&
                    compilerService.KeywordCheckBuffer[3] == 'l' &&
                    compilerService.KeywordCheckBuffer[4] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 444: // with
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'w' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[2] == 't' &&
                    compilerService.KeywordCheckBuffer[3] == 'h')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 535: // yield
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'y' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[2] == 'e' &&
                    compilerService.KeywordCheckBuffer[3] == 'l' &&
                    compilerService.KeywordCheckBuffer[4] == 'd')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            default:
                
                var endPosition = streamReaderWrap.PositionIndex;
                
                while (!streamReaderWrap.IsEof)
                {
                    if (!char.IsWhiteSpace(streamReaderWrap.CurrentCharacter))
                    {
                        break;
                    }
                    _ = streamReaderWrap.ReadCharacter();
                }
                
                if (streamReaderWrap.CurrentCharacter == '(')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Function });
                }
                else if (streamReaderWrap.CurrentCharacter == ':')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Field });
                }
                else
                {
                    textSpanList.Add(textSpan);
                }
                return;
        }
    }
}
