using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Razor;

public static class RazorLexer
{
    public enum RazorLexerContextKind
    {
        Expect_TagOrText,
        // Expect_TagName, // There is no expect tag name, you can't have whitespace here
        Expect_AttributeName,
        Expect_AttributeValue,
    }

    public static RazorLexerOutput Lex(StreamReaderWrap streamReaderWrap)
    {
        var context = RazorLexerContextKind.Expect_TagOrText;
        var output = new RazorLexerOutput();
        
        // This gets updated throughout the loop
        var startPosition = streamReaderWrap.PositionIndex;
        var startByte = streamReaderWrap.ByteIndex;
        
        var indexOfMostRecentTagOpen = -1;
        
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
                /* At */
                case '@':
                    if (context == RazorLexerContextKind.Expect_AttributeName)
                    {
                        if (streamReaderWrap.CurrentCharacter == '@')
                        {
                            var atCharStartPosition = streamReaderWrap.PositionIndex;
                            var atCharStartByte = streamReaderWrap.ByteIndex;
                            _ = streamReaderWrap.ReadCharacter();
                            // Attribute skips HTML identifier because ':' example: 'onclick:stopPropagation="true"'
                            SkipHtmlIdentifier(streamReaderWrap);
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                atCharStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)RazorDecorationKind.AttributeNameInjectedLanguageFragment,
                                atCharStartByte));
                        }
                        else
                        {
                            var attributeNameStartPosition = streamReaderWrap.PositionIndex;
                            var attributeNameStartByte = streamReaderWrap.ByteIndex;
                            var wasInjectedLanguageFragment = false;
                            while (!streamReaderWrap.IsEof)
                            {
                                if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter))
                                {
                                    if (streamReaderWrap.CurrentCharacter != '_' &&
                                             streamReaderWrap.CurrentCharacter != '-' &&
                                             streamReaderWrap.CurrentCharacter != ':')
                                    {
                                        break;
                                    }
                                }
                                _ = streamReaderWrap.ReadCharacter();
                            }
                            
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeNameStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)RazorDecorationKind.AttributeName,
                                attributeNameStartByte));
                        }
                        
                        context = RazorLexerContextKind.Expect_AttributeValue;
                        break;
                    }
                    else if (context == RazorLexerContextKind.Expect_AttributeValue)
                    {
                        if (streamReaderWrap.CurrentCharacter == '@')
                        {
                            var atCharStartPosition = streamReaderWrap.PositionIndex;
                            var atCharStartByte = streamReaderWrap.ByteIndex;
                            _ = streamReaderWrap.ReadCharacter();
                            SkipCSharpdentifier(streamReaderWrap);
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                atCharStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)RazorDecorationKind.AttributeValueInjectedLanguageFragment,
                                atCharStartByte));
                        }
                        else
                        {
                            var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                            var attributeValueStartByte = streamReaderWrap.ByteIndex;
                            while (!streamReaderWrap.IsEof)
                            {
                                if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter))
                                {
                                    if (streamReaderWrap.CurrentCharacter == '@' &&
                                        streamReaderWrap.CurrentCharacter != '_' &&
                                        streamReaderWrap.CurrentCharacter != '-' &&
                                        streamReaderWrap.CurrentCharacter != ':')
                                    {
                                        break;
                                    }
                                }
                                _ = streamReaderWrap.ReadCharacter();
                            }
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeValueStartPosition,
                                streamReaderWrap.PositionIndex,
                                (byte)RazorDecorationKind.AttributeValue,
                                attributeValueStartByte));
                        }
                        
                        context = RazorLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    else if (context == RazorLexerContextKind.Expect_TagOrText)
                    {
                        var textStartPosition = streamReaderWrap.PositionIndex;
                        var textStartByte = streamReaderWrap.ByteIndex;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '<')
                            {
                                break;
                            }
                            else if (streamReaderWrap.CurrentCharacter == '@')
                            {
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    textStartPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)RazorDecorationKind.Text,
                                    textStartByte));
                                var atCharStartPosition = streamReaderWrap.PositionIndex;
                                var atCharStartByte = streamReaderWrap.ByteIndex;
                                _ = streamReaderWrap.ReadCharacter();
                            
                                if (streamReaderWrap.CurrentCharacter == '*')
                                {
                                    _ = streamReaderWrap.ReadCharacter();
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        atCharStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.InjectedLanguageFragment,
                                        atCharStartByte));
                                        
                                    var commentStartPosition = streamReaderWrap.PositionIndex;
                                    var commentStartByte = streamReaderWrap.ByteIndex;
                                    
                                    while (!streamReaderWrap.IsEof)
                                    {
                                        if (streamReaderWrap.CurrentCharacter == '*' && streamReaderWrap.PeekCharacter(1) == '@')
                                            break;
                                    
                                        _ = streamReaderWrap.ReadCharacter();
                                    }
                                    
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        commentStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.Comment,
                                        commentStartByte));
                                    
                                    // The while loop has 2 break cases, thus !IsEof means "*@" was the break cause.
                                    if (!streamReaderWrap.IsEof)
                                    {
                                        var starStartPosition = streamReaderWrap.PositionIndex;
                                        var starStartByte = streamReaderWrap.ByteIndex;
                                        
                                        _ = streamReaderWrap.ReadCharacter();
                                        _ = streamReaderWrap.ReadCharacter();
                                        output.TextSpanList.Add(new TextEditorTextSpan(
                                            starStartPosition,
                                            streamReaderWrap.PositionIndex,
                                            (byte)RazorDecorationKind.InjectedLanguageFragment,
                                            starStartByte));
                                    }
                                }
                                else
                                {
                                    SkipCSharpdentifier(streamReaderWrap);
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        atCharStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.InjectedLanguageFragment,
                                        atCharStartByte));
                                }
                                
                                textStartPosition = streamReaderWrap.PositionIndex;
                                textStartByte = streamReaderWrap.ByteIndex;
                                continue;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            textStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.Text,
                            textStartByte));
                        context = RazorLexerContextKind.Expect_TagOrText;
                        break;
                    }
                    
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
                    if (context == RazorLexerContextKind.Expect_AttributeValue)
                    {
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                                streamReaderWrap.CurrentCharacter != '_' &&
                                streamReaderWrap.CurrentCharacter != '-' &&
                                streamReaderWrap.CurrentCharacter != ':')
                            {
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            attributeValueStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeValue,
                            attributeValueStartByte));
                        context = RazorLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    
                    goto default;
                case '\'':
                    if (context == RazorLexerContextKind.Expect_AttributeValue)
                    {
                        var delimiterStartPosition = streamReaderWrap.PositionIndex;
                        var delimiterStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeDelimiter,
                            delimiterStartByte));
                            
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        var attributeValueEnd = streamReaderWrap.PositionIndex;
                        var hasSeenInterpolation = false;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '\'')
                            {
                                attributeValueEnd = streamReaderWrap.PositionIndex;
                                delimiterStartPosition = streamReaderWrap.PositionIndex;
                                delimiterStartByte = streamReaderWrap.ByteIndex;
                                _ = streamReaderWrap.ReadCharacter();
                                break;
                            }
                            else if (streamReaderWrap.CurrentCharacter == '@')
                            {
                                if (!hasSeenInterpolation)
                                {
                                    hasSeenInterpolation = true;
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        attributeValueStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.AttributeValueInterpolationStart,
                                        attributeValueStartByte));
                                }
                                else
                                {
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        attributeValueStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.AttributeValueInterpolationContinue,
                                        attributeValueStartByte));
                                }
                                
                                var interpolationStartPosition = streamReaderWrap.PositionIndex;
                                var interpolationStartByte = streamReaderWrap.ByteIndex;
                                _ = streamReaderWrap.ReadCharacter();
                                SkipCSharpdentifier(streamReaderWrap);
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    interpolationStartPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)RazorDecorationKind.AttributeValueInjectedLanguageFragment,
                                    interpolationStartByte));
                                
                                attributeValueStartPosition = streamReaderWrap.PositionIndex;
                                attributeValueStartByte = streamReaderWrap.ByteIndex;
                                continue;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        if (hasSeenInterpolation)
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeValueStartPosition,
                                attributeValueEnd,
                                (byte)RazorDecorationKind.AttributeValueInterpolationContinue,
                                attributeValueStartByte));
                        
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                delimiterStartPosition,
                                delimiterStartPosition,
                                (byte)RazorDecorationKind.AttributeValueInterpolationEnd,
                                delimiterStartByte));
                        }
                        else
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeValueStartPosition,
                                attributeValueEnd,
                                (byte)RazorDecorationKind.AttributeValue,
                                attributeValueStartByte));
                        }
                        
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeDelimiter,
                            delimiterStartByte));
                            
                        context = RazorLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    goto default;
                case '"':
                    if (context == RazorLexerContextKind.Expect_AttributeValue)
                    {
                        var delimiterStartPosition = streamReaderWrap.PositionIndex;
                        var delimiterStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeDelimiter,
                            delimiterStartByte));
                        
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        var attributeValueEnd = streamReaderWrap.PositionIndex;
                        var hasSeenInterpolation = false;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '"')
                            {
                                attributeValueEnd = streamReaderWrap.PositionIndex;
                                delimiterStartPosition = streamReaderWrap.PositionIndex;
                                delimiterStartByte = streamReaderWrap.ByteIndex;
                                _ = streamReaderWrap.ReadCharacter();
                                break;
                            }
                            else if (streamReaderWrap.CurrentCharacter == '@')
                            {
                                if (!hasSeenInterpolation)
                                {
                                    hasSeenInterpolation = true;
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        attributeValueStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.AttributeValueInterpolationStart,
                                        attributeValueStartByte));
                                }
                                else
                                {
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        attributeValueStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.AttributeValueInterpolationContinue,
                                        attributeValueStartByte));
                                }
                                
                                var interpolationStartPosition = streamReaderWrap.PositionIndex;
                                var interpolationStartByte = streamReaderWrap.ByteIndex;
                                _ = streamReaderWrap.ReadCharacter();
                                SkipCSharpdentifier(streamReaderWrap);
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    interpolationStartPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)RazorDecorationKind.AttributeValueInjectedLanguageFragment,
                                    interpolationStartByte));
                                
                                attributeValueStartPosition = streamReaderWrap.PositionIndex;
                                attributeValueStartByte = streamReaderWrap.ByteIndex;
                                continue;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        if (hasSeenInterpolation)
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeValueStartPosition,
                                attributeValueEnd,
                                (byte)RazorDecorationKind.AttributeValueInterpolationContinue,
                                attributeValueStartByte));
                        
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                delimiterStartPosition,
                                delimiterStartPosition,
                                (byte)RazorDecorationKind.AttributeValueInterpolationEnd,
                                delimiterStartByte));
                        }
                        else
                        {
                            output.TextSpanList.Add(new TextEditorTextSpan(
                                attributeValueStartPosition,
                                attributeValueEnd,
                                (byte)RazorDecorationKind.AttributeValue,
                                attributeValueStartByte));
                        }
                        
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeDelimiter,
                            delimiterStartByte));
                        
                        context = RazorLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    goto default;
                case '/':
                
                    if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        if (context == RazorLexerContextKind.Expect_AttributeName || context == RazorLexerContextKind.Expect_AttributeValue)
                        {
                            if (indexOfMostRecentTagOpen != -1)
                            {
                                output.TextSpanList[indexOfMostRecentTagOpen] = output.TextSpanList[indexOfMostRecentTagOpen] with
                                {
                                    DecorationByte = (byte)RazorDecorationKind.TagNameSelf,
                                };
                                indexOfMostRecentTagOpen = -1;
                            }
                            context = RazorLexerContextKind.Expect_TagOrText;
                        }
                    }
                
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
                    if (context == RazorLexerContextKind.Expect_AttributeValue)
                    {
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            attributeValueStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)RazorDecorationKind.AttributeOperator,
                            attributeValueStartByte));
                        break;
                    }
                
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
                    
                    var tagDecoration = (byte)RazorDecorationKind.TagNameOpen;
                    
                    if (context == RazorLexerContextKind.Expect_TagOrText)
                    {
                        _ = streamReaderWrap.ReadCharacter();
                        
                        if (streamReaderWrap.CurrentCharacter == '/')
                        {
                            tagDecoration = (byte)RazorDecorationKind.TagNameClose;
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        else if (streamReaderWrap.CurrentCharacter == '!')
                        {
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        var tagNameStartPosition = streamReaderWrap.PositionIndex;
                        var tagNameStartByte = streamReaderWrap.ByteIndex;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                                streamReaderWrap.CurrentCharacter != '_' &&
                                streamReaderWrap.CurrentCharacter != '-' &&
                                streamReaderWrap.CurrentCharacter != ':')
                            {
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        if (tagDecoration == (byte)RazorDecorationKind.TagNameOpen)
                        {
                            indexOfMostRecentTagOpen = output.TextSpanList.Count;
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            tagNameStartPosition,
                            streamReaderWrap.PositionIndex,
                            tagDecoration,
                            tagNameStartByte));

                        if (streamReaderWrap.CurrentCharacter == '>')
                        {
                            context = RazorLexerContextKind.Expect_TagOrText;
                        }
                        else
                        {
                            context = RazorLexerContextKind.Expect_AttributeName;
                        }
                        
                        break;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case '>':
                {
                    context = RazorLexerContextKind.Expect_TagOrText;
                
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
    
    private static void SkipHtmlIdentifier(StreamReaderWrap streamReaderWrap)
    {
        while (!streamReaderWrap.IsEof)
        {
            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                streamReaderWrap.CurrentCharacter != '_' &&
                streamReaderWrap.CurrentCharacter != '-' &&
                streamReaderWrap.CurrentCharacter != ':')
            {
                break;
            }
            _ = streamReaderWrap.ReadCharacter();
        }
    }
    
    private static void SkipCSharpdentifier(StreamReaderWrap streamReaderWrap)
    {
        while (!streamReaderWrap.IsEof)
        {
            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                streamReaderWrap.CurrentCharacter != '_')
            {
                break;
            }
            _ = streamReaderWrap.ReadCharacter();
        }
    }
}
