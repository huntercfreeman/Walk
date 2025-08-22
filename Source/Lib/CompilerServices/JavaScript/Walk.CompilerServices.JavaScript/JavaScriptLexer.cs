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
                    goto default;
                case '"':
                    goto default;
                case '/':
                    if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '*')
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
            (byte)GenericDecorationKind.None,
            byteEntryIndex,
            characterIntSum);
        
        switch (characterIntSum)
        {
            case 534: // !! DUPLICATES !!
            
                if (textSpan.Length != 5)
                    goto default;

                if (compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'w' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
                {
                    // await
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                else if (compilerService.KeywordCheckBuffer[0] == 'c' &&
                         compilerService.KeywordCheckBuffer[1] == 'l' &&
                         compilerService.KeywordCheckBuffer[1] == 'a' &&
                         compilerService.KeywordCheckBuffer[1] == 's' &&
                         compilerService.KeywordCheckBuffer[1] == 's')
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
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'k')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 412: // case
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 515: // catch
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'h')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 551: // const
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 869: // continue
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 837: // debugger
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'b' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'g' &&
                    compilerService.KeywordCheckBuffer[1] == 'g' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 741: // default
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 627: // delete
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'd' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
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
                    compilerService.KeywordCheckBuffer[1] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 437: // enum
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'm')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 674: // export
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'x' &&
                    compilerService.KeywordCheckBuffer[1] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
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
                    compilerService.KeywordCheckBuffer[1] == 't'&&
                    compilerService.KeywordCheckBuffer[1] == 'e'&&
                    compilerService.KeywordCheckBuffer[1] == 'n'&&
                    compilerService.KeywordCheckBuffer[1] == 'd'&&
                    compilerService.KeywordCheckBuffer[1] == 's')
                {
                    // extends
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                else if (compilerService.KeywordCheckBuffer[0] == 'p' &&
                         compilerService.KeywordCheckBuffer[1] == 'r'&&
                         compilerService.KeywordCheckBuffer[1] == 'i'&&
                         compilerService.KeywordCheckBuffer[1] == 'v'&&
                         compilerService.KeywordCheckBuffer[1] == 'a'&&
                         compilerService.KeywordCheckBuffer[1] == 't'&&
                         compilerService.KeywordCheckBuffer[1] == 'e')
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
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 751: // finally
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'y')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 327: // for
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'r')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 870: // function
                if (textSpan.Length == 8 &&
                    compilerService.KeywordCheckBuffer[0] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'n')
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
                    compilerService.KeywordCheckBuffer[1] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'm' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 's')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 667: // import
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'm' &&
                    compilerService.KeywordCheckBuffer[1] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
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
                    compilerService.KeywordCheckBuffer[1] == 's' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 'f')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 945: // interface
                if (textSpan.Length == 9 &&
                    compilerService.KeywordCheckBuffer[0] == 'i' &&
                    compilerService.KeywordCheckBuffer[1] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[1] == 'f' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 325: // let
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 't')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 330: // new
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'w')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 443: // null
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'n' &&
                    compilerService.KeywordCheckBuffer[1] == 'u' &&
                    compilerService.KeywordCheckBuffer[1] == 'l' &&
                    compilerService.KeywordCheckBuffer[1] == 'l')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 716: // package
                if (textSpan.Length == 7 &&
                    compilerService.KeywordCheckBuffer[0] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 'k' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'g' &&
                    compilerService.KeywordCheckBuffer[1] == 'e')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 970: // protected
                if (textSpan.Length == 9 &&
                    compilerService.KeywordCheckBuffer[0] == 'p' &&
                    compilerService.KeywordCheckBuffer[1] == 'r' &&
                    compilerService.KeywordCheckBuffer[1] == 'o' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'c' &&
                    compilerService.KeywordCheckBuffer[1] == 't' &&
                    compilerService.KeywordCheckBuffer[1] == 'e' &&
                    compilerService.KeywordCheckBuffer[1] == 'd')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 639: // public
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 672: // return
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 559: // super
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 658: // switch
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 648: // static
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 440: // this
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 564: // throw
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 351: // try
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 416: // True
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 663: // typeof
                if (textSpan.Length == 6 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 329: // var
                if (textSpan.Length == 3 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 434: // void
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 537: // while
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 444: // with
                if (textSpan.Length == 4 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            case 535: // yield
                if (textSpan.Length == 5 &&
                    compilerService.KeywordCheckBuffer[0] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a' &&
                    compilerService.KeywordCheckBuffer[1] == 'a')
                {
                    textSpanList.Add(textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword });
                    return;
                }
                
                goto default;
            default:
                textSpanList.Add(textSpan);
                return;
        }
    }
}
