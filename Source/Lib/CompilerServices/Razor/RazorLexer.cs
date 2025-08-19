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
                                else if (streamReaderWrap.CurrentCharacter == '{')
                                {
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        atCharStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.InjectedLanguageFragment,
                                        atCharStartByte));
                                        
                                    LexCSharpCodeBlock(streamReaderWrap, output.TextSpanList);
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
    
    private static void SkipCSharpdentifierOrKeyword(StreamReaderWrap streamReaderWrap)
    {
        /*
        
        
            
        
            a97 + d100                = 197
            a97 + t116                = 213
            c99 + a97                 = 196
            c99 + l108                = 207
            c99 + o111                = 210
            d100 + e101               = 201
            d100 + o111^              = 211
            f102 + o111 + r114^       = 327
            f102 + o111 + r114 + e101 = 428
            f102 + u117               = 219
            l108 + a97                = 205
            l108 + o111               = 219
            m109                      = 109
            p112 + a97                = 209
            p112 + r114               = 226
            i105 + f102^              = 207
            i105 + m109               = 214
            i105 + n110 + h104        = 319
            i105 + n110 + j106        = 321
            n110                      = 110
            r114 + e101 + m109        = 324
            r114 + e101 + n110        = 325
            s115 + e101               = 216
            s115 + w119               = 234
            t116 + a97                = 213
            t116 + r114               = 230
            t116 + y121               = 237
            u117                      = 117
            w119                      = 119
        
        
        
        ================================================
        ================================================
        
        
        
        
        
            ad
            at
            ca
            cl
            co
            de
            do^
            for^
            fore
            fu
            la
            lo
            m
            pa
            pr
            if^
            im
            inh
            inj
            n
            rem
            ren
            se
            sw
            ta
            tr
            ty
            u
            w
        */
    
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
    
    /// <summary>
    /// Current character ought to be the open brace, and the syntax highlighting for the open brace will be made as part of this method.
    ///
    /// The C# simply gets syntax highlighted as one single color for now during the outlining process.
    /// This method is intended to handle all false positives for brace matching:
    /// - Single line comment
    /// - Multi line comment
    /// - String
    /// But, when it comes to 'string' only the simple case of one pair of double quotes deliminating the string is being explicitly handled.
    /// Anything else relating to 'string' might by coincidence lex correctly, or it might not.
    /// TODO: Handle raw string literals and check whether verbatim and interpolated strings lex correctly.
    ///
    /// This method returns 1 character after the close brace, or EOF.
    /// </summary>
    private static void LexCSharpCodeBlock(StreamReaderWrap streamReaderWrap, List<TextEditorTextSpan> textSpanList)
    {
        var openBraceStartPosition = streamReaderWrap.PositionIndex;
        var openBraceStartByte = streamReaderWrap.ByteIndex;
        _ = streamReaderWrap.ReadCharacter();
        textSpanList.Add(new TextEditorTextSpan(
            openBraceStartPosition,
            streamReaderWrap.PositionIndex,
            (byte)RazorDecorationKind.InjectedLanguageFragment,
            openBraceStartByte));
        
        var cSharpStartPosition = streamReaderWrap.PositionIndex;
        var cSharpStartByte = streamReaderWrap.ByteIndex;
        
        var braceMatch = 1;
        
        var isSingleLineComment = false;
        var isMultiLineComment = false;
        var isString = false;
        
        var previousCharWasForwardSlash = false;
        
        while (!streamReaderWrap.IsEof)
        {
            var localPreviousCharWasForwardSlash = previousCharWasForwardSlash;
            
            if (streamReaderWrap.CurrentCharacter == '/')
            {
                previousCharWasForwardSlash = true;
            }
            else
            {
                previousCharWasForwardSlash = false;
            }
        
            if (isMultiLineComment)
            {
                if (streamReaderWrap.CurrentCharacter == '*' && streamReaderWrap.PeekCharacter(1) == '/')
                {
                    _ = streamReaderWrap.ReadCharacter();
                    _ = streamReaderWrap.ReadCharacter();
                    isMultiLineComment = false;
                    continue;
                }
            }
            else if (isSingleLineComment)
            {
                if (streamReaderWrap.CurrentCharacter == '\r' ||
                    streamReaderWrap.CurrentCharacter == '\n')
                {
                    isSingleLineComment = false;
                }
            }
            else if (isString)
            {
                if (streamReaderWrap.CurrentCharacter == '"')
                {
                    isString = false;
                }
            }
            else if (streamReaderWrap.CurrentCharacter == '"')
            {
                isString = true;
            }
            else if (streamReaderWrap.CurrentCharacter == '/')
            {
                if (localPreviousCharWasForwardSlash)
                {
                    isSingleLineComment = true;
                }
            }
            else if (streamReaderWrap.CurrentCharacter == '*')
            {
                if (localPreviousCharWasForwardSlash)
                {
                    isMultiLineComment = true;
                }
            }
            else if (streamReaderWrap.CurrentCharacter == '}')
            {
                if (--braceMatch == 0)
                    break;
            }
            else if (streamReaderWrap.CurrentCharacter == '{')
            {
                ++braceMatch;
            }
        
            _ = streamReaderWrap.ReadCharacter();
        }
        
        textSpanList.Add(new TextEditorTextSpan(
            cSharpStartPosition,
            streamReaderWrap.PositionIndex,
            (byte)RazorDecorationKind.CSharpMarker,
            cSharpStartByte));
        
        // The while loop has 2 break cases, thus !IsEof means "*@" was the break cause.
        if (!streamReaderWrap.IsEof)
        {
            var closeBraceStartPosition = streamReaderWrap.PositionIndex;
            var closeBraceStartByte = streamReaderWrap.ByteIndex;
            
            _ = streamReaderWrap.ReadCharacter();
            textSpanList.Add(new TextEditorTextSpan(
                closeBraceStartPosition,
                streamReaderWrap.PositionIndex,
                (byte)RazorDecorationKind.InjectedLanguageFragment,
                closeBraceStartByte));
        }
    }
    
    /*
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    Conditionals: @if, else if, else, and @switch
    
    Looping: @for, @foreach, @while, and @do while
    
    Compound: @using
    
    ...: @try, catch, finally
    
    ...: @lock
    
    Directives
    ----------
    @attribute [Authorize]                    
    @page "/counter"
    @code { } // More than one @code block is permissible.
    @functions { }  // More than one @functions block is permissible.
    // In Razor components, use @code over @functions to add C# members.
    @implements IDisposable
    @inherits TypeNameOfClassToInheritFrom
    @inherits CustomRazorPage<TModel>
    @model and @inherits can be used in the same view. @inherits can be in a _ViewImports.cshtml file that the view imports:
    @inherits CustomRazorPage<TModel>
    @inject
    @layout
    @model // CSHTML
    @namespace
    @page
    @preservewhitespace
    @rendermode
    @section
    @typeparam
    @using
    
    Directive attributes
    --------------------
    @attributes
    @bind
    @bind:culture
    @formname
    @on{EVENT}
    @on{EVENT}:preventDefault
    @on{EVENT}:stopPropagation
    @key
    @ref
    
    Templated Razor delegates
    -------------------------
    @<tag>...</tag>
    @{ Func<dynamic, object> petTemplate = @<p>You have a pet named <strong>@item.Name</strong>.</p>; }
    
    Tag Helpers // MVC
    ------------------
    @addTagHelper
    @removeTagHelper
    @tagHelperPrefix
    
    Razor reserved keywords
    -----------------------
    page
    namespace
    functions
    inherits
    model
    section
    helper (Not currently supported by ASP.NET Core)
    // Razor keywords are escaped with @(Razor Keyword) (for example, @(functions)).

    C# Razor keywords
    -----------------
    case
    do
    default
    for
    foreach
    if
    else
    lock
    switch
    try
    catch
    finally
    using
    while
    C# Razor keywords must be double-escaped with @(@C# Razor Keyword) (for example, @(@case)).
        The first @ escapes the Razor parser.
        The second @ escapes the C# parser.
    
    Reserved keywords not used by Razor
    -----------------------------------
    class
    
    Templating methods
    ------------------
    
    Flat A
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    @if, else if, else
    @switch
    @for,
    @foreach,
    @while,
    @do while
    @using
    @try, catch, finally
    @lock
    @attribute [Authorize]                    
    @page "/counter"
    @code { } // More than one @code block is permissible.
    @functions { }  // More than one @functions block is permissible.
    // In Razor components, use @code over @functions to add C# members.
    @implements IDisposable
    @inherits TypeNameOfClassToInheritFrom
    @inherits CustomRazorPage<TModel>
    @model and @inherits can be used in the same view. @inherits can be in a _ViewImports.cshtml file that the view imports:
    @inherits CustomRazorPage<TModel>
    @inject
    @layout
    @model // CSHTML
    @namespace
    @page
    @preservewhitespace
    @rendermode
    @section
    @typeparam
    @using
    @<tag>...</tag>
    @{ Func<dynamic, object> petTemplate = @<p>You have a pet named <strong>@item.Name</strong>.</p>; }
    @addTagHelper
    @removeTagHelper
    @tagHelperPrefix
    page
    namespace
    functions
    inherits
    model
    section
    helper (Not currently supported by ASP.NET Core)
    case
    do
    default
    for
    foreach
    if
    else
    lock
    switch
    try
    catch
    finally
    using
    while
    class
    
    Directive attributes
    --------------------
    @attributes
    @bind
    @bind:culture
    @formname
    @on{EVENT}
    @on{EVENT}:preventDefault
    @on{EVENT}:stopPropagation
    @key
    @ref
    
    Flat B
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    if
    switch
    for
    foreach
    while
    do
    using
    try
    lock
    attribute
    page
    code { }
    functions { }
    implements
    inherits
    inject
    layout
    namespace
    page
    preservewhitespace
    rendermode
    section
    typeparam
    using
    addTagHelper
    removeTagHelper
    tagHelperPrefix
    page
    namespace
    functions
    inherits
    model
    section
    case
    do
    default
    for
    foreach
    if
    else
    lock
    switch
    try
    catch
    finally
    using
    while
    class
    
    Flat C
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    addTagHelper
    attribute
    case
    class
    code { }
    default
    do
    for
    foreach
    functions { }
    layout
    lock
    model
    page
    preservewhitespace
    if
    implements
    inherits
    inject
    namespace
    removeTagHelper
    rendermode
    section
    switch
    tagHelperPrefix
    try
    typeparam
    using
    while
    
    Flat D
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    a
     ddTagHelper
     ttribute
     
    c
     ase
     lass
     ode { }
     
    d
     efault
     o
     
    f
     or
     oreach
     unctions { }
     
    l
     ayout
     ock
     
    m
     odel
     
    p
     age
     reservewhitespace
     
    i
     f
     mplements
     nherits
     nject
     
    n
     amespace
     
    r
     emoveTagHelper
     endermode
     
    s
     ection
     witch
     
    t
     agHelperPrefix
     ry
     ypeparam
     
    u
     sing
     
    w
     hile
    
    Flat E
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    ad
      dTagHelper
    at
      tribute
     
    ca
      se
    cl
      ass
    co
      de { }
     
    de
      fault
    do
      // 'do' was the word itself
     
    fo
      r
      reach
    fu
      nctions { }
     
    la
      yout
    lo
      ck
     
    m
     odel
     
    pa
      ge
    pr
      eservewhitespace
     
    if
      // 'if' was the word itself
    im
      plements
    in
      herits
      ject
     
    n
     amespace
     
    re
      moveTagHelper
      ndermode
     
    se
      ction
    sw
      itch
     
    ta
      gHelperPrefix
    tr
      y
    ty
      peparam
     
    u
     sing
     
    w
     hile
    
    Flat F
    
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    ad
      dTagHelper
    at
      tribute
     
    ca
      se
    cl
      ass
    co
      de { }
     
    de
      fault
    do
      // 'do' was the word itself
     
    for
       // 'for' was a word itself
       each
    fu
      nctions { }
     
    la
      yout
    lo
      ck
     
    m
     odel
     
    pa
      ge
    pr
      eservewhitespace
     
    if
      // 'if' was the word itself
    im
      plements
    inh
       erits
    inj
       ect
     
    n
     amespace
     
    rem
       oveTagHelper
    ren
       dermode
     
    se
      ction
    sw
      itch
     
    ta
      gHelperPrefix
    tr
      y
    ty
      peparam
     
    u
     sing
     
    w
     hile
    
    Flat G
            
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    ad
    at
     
    ca
    cl
    co
     
    de
    do // 'do' was a word itself
      
     
    for // 'for' was a word itself
       each
    fu
     
    la
    lo
     
    m
     
    pa
    pr
     
    if // 'if' was a word itself
    im
    inh
    inj
     
    n
     
    rem
    ren
     
    se
    sw
     
    ta
    tr
    ty
     
    u
     
    w
    
    Flat H
            
    https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0
    =================================================================================
    
    ad
    at
    ca
    cl
    co
    de
    do^
    for^
    fore
    fu
    la
    lo
    m
    pa
    pr
    if^
    im
    inh
    inj
    n
    rem
    ren
    se
    sw
    ta
    tr
    ty
    u
    w
    */
}
