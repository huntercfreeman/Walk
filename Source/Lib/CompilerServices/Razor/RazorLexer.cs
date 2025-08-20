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
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        atCharStartPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)RazorDecorationKind.InjectedLanguageFragment,
                                        atCharStartByte));
                                
                                    var wordStartPosition = streamReaderWrap.PositionIndex;
                                    var wordStartByte = streamReaderWrap.ByteIndex;
                                    
                                    var everythingWasHandledForMe = SkipCSharpdentifierOrKeyword(streamReaderWrap, output.TextSpanList);
                                    if (!everythingWasHandledForMe)
                                    {
                                        output.TextSpanList.Add(new TextEditorTextSpan(
                                            wordStartPosition,
                                            streamReaderWrap.PositionIndex,
                                            (byte)RazorDecorationKind.InjectedLanguageFragment,
                                            wordStartByte));
                                    }
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
    
    /// <summary>
    /// When this returns true, then the state of the lexer has entirely changed
    /// and the invoker should disregard any of their previous state and reset it.
    ///
    /// This method when finding a brace deliminated code blocked keyword will entirely lex to the close brace.
    /// </summary>
    private static bool SkipCSharpdentifierOrKeyword(StreamReaderWrap streamReaderWrap, List<TextEditorTextSpan> textSpanList)
    {
        var wordStartPosition = streamReaderWrap.PositionIndex;
        var wordStartByte = streamReaderWrap.ByteIndex;
        
        var characterIntSum = 0;
    
        while (!streamReaderWrap.IsEof)
        {
            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                streamReaderWrap.CurrentCharacter != '_')
            {
                break;
            }
            
            characterIntSum += (int)streamReaderWrap.CurrentCharacter;
            _ = streamReaderWrap.ReadCharacter();
        }
        
        switch (characterIntSum)
        {
            case 1189: // addTagHelper
                break;
            case 980: // attribute
                break;
            case 412: // case
                break;
            case 534: // class
                break;
            case 411: // code
            case 985: // functions
                textSpanList.Add(new TextEditorTextSpan(
                    wordStartPosition,
                    streamReaderWrap.PositionIndex,
                    (byte)RazorDecorationKind.InjectedLanguageFragment,
                    wordStartByte));
                    
                while (!streamReaderWrap.IsEof)
                {
                    if (!char.IsWhiteSpace(streamReaderWrap.CurrentCharacter))
                    {
                        break;
                    }
                    
                    _ = streamReaderWrap.ReadCharacter();
                }
                
                if (streamReaderWrap.CurrentCharacter == '{')
                {
                    LexCSharpCodeBlock(streamReaderWrap, textSpanList);
                    return true;
                }
                else
                {
                    return true;
                }
            case 741: // default
                break;
            case 211: // do
                break;
            case 327: // for
                break;
            case 728: // foreach
                break;
            case 670: // layout
                break;
            case 425: // lock
                break;
            case 529: // model
                break;
            case 413: // page
                break;
            case 1945: // preservewhitespace
                break;
            case 207: // if
                break;
            case 1086: // implements
                break;
            case 870: // inherits
                break;
            case 637: // inject
                break;
            case 941: // namespace
                break;
            // !DUPLICATE!:1546
                case 1546:
                    // removeTagHelper
                    // tagHelperPrefix
                    break;
            case 1061: // rendermode
                break;
            case 757: // section
                break;
            case 658: // switch
                break;
            case 351: // try
                break;
            case 979: // typeparam
                break;
            case 550: // using
                break;
            case 537: // while
                break;
            default:
                break;
        }
        
        return false;
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
        =======================
        =======================
        1189 // addTagHelper
        980 // attribute
        412 // case
        534 // class
        411 // code
        741 // default
        211 // do
        327 // for
        728 // foreach
        985 // functions
        670 // layout
        425 // lock
        529 // model
        413 // page
        1945 // preservewhitespace
        207 // if
        1086 // implements
        870 // inherits
        637 // inject
        941 // namespace
        !DUPLICATE!:1546/tagHelperPrefix
            1546 // removeTagHelper
            1546 // tagHelperPrefix
        1061 // rendermode
        757 // section
        658 // switch
        351 // try
        979 // typeparam
        550 // using
        537 // while
        =======================
        =======================
    
        var keywordList = new List<string>
        {
            "addTagHelper",
            "attribute",
            "case",
            "class",
            "code",
            "default",
            "do",
            "for",
            "foreach",
            "functions",
            "layout",
            "lock",
            "model",
            "page",
            "preservewhitespace",
            "if",
            "implements",
            "inherits",
            "inject",
            "namespace",
            "removeTagHelper",
            "rendermode",
            "section",
            "switch",
            "tagHelperPrefix",
            "try",
            "typeparam",
            "using",
            "while",
        };
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");
        
        var hashSet = new HashSet<int>();
        
        foreach (var keyword in keywordList)
        {
            var sum = 0;
            foreach (var character in keyword)
            {
                sum += (int)character;
            }
            
            if (!hashSet.Add(sum))
            {
                Console.WriteLine($"!DUPLICATE!:{sum}/{keyword}");
            }
            Console.WriteLine($"{sum} // {keyword}");
        }
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");
        */
    
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
        
        /*
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
        */
    
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






/*switch (binder.CSharpCompilerService.SafeGetText(lexerOutput.ResourceUri.Value, textSpan) ?? string.Empty)
{
    // NonContextualKeywords-NonControl
    // ================================
    
    // case "abstract":
        
    // case "as":
        
    // case "base":
        
    // case "bool":
        
    // case "byte":
        
    // case "catch":
        
    // case "char":
        
    // case "checked":
        
    // case "class":
        
    // case "const":
        
    // case "decimal":
        
    // case "default":
        
    // case "delegate":
        
    // case "double":
        
    // case "enum":
        
    // case "event":
        
    // case "explicit":
        
    // case "extern":
        
    // case "false":
        
    // case "finally":
        
    // case "fixed":
        
    // case "float":
        
    // case "implicit":
        
    // case "in":
        
    // case "int":
        
    // case "interface":
        
    // case "internal":
        
    // case "is":
        
    // case "lock":
        
    // case "long":
        
    // case "namespace":
        
    // case "new":
        
    // case "null":
        
    // case "object":
        
    // case "operator":
        
    // case "out":
        
    // case "override":
        
    // case "params":
        
    // case "private":
        
    // case "protected":
        
    // case "public":
        
    // case "readonly":
        
    // case "ref":
        
    // case "sbyte":
        
    // case "sealed":
        
    // case "short":
        
    // case "sizeof":
        
    // case "stackalloc":
        
    // case "static":
        
    // case "string":
        
    // case "struct":
        
    // case "this":
        
    // case "true":
        
    // case "try":
        
    // case "typeof":
        
    // case "uint":
        
    // case "ulong":
        
    // case "unchecked":
        
    // case "unsafe":
        
    // case "ushort":
        
    // case "using":
        
    // case "virtual":
        
    // case "void":
        
    // case "volatile":
        
    // NonContextualKeywords-IsControl
    // ===============================
    // case "break":
        
    // case "case":
        
    // case "continue":
        
    // case "do":
        
    // case "else":
        
    // case "for":
        
    // case "foreach":
        
    // case "goto":
        
    // case "if":
        
    // case "return":
        
    // case "switch":
        
    // case "throw":
        
    // case "while":
        
    // ContextualKeywords-NotControl
    // =============================
    // case "add":
        
    // case "and":
        
    // case "alias":
        
    // case "ascending":
        
    // case "args":
        
    // case "async":
        
    // case "await":
        
    // case "by":
        
    // case "descending":
        
    // case "dynamic":
        
    // case "equals":
        
    // case "file":
        
    // case "from":
        
    // case "get":
        
    // case "global":
        
    // case "group":
        
    // case "init":
        
    // case "into":
        
    // case "join":
        
    // case "let":
        
    // case "managed":
        
    // case "nameof":
        
    // case "nint":
        
    // case "not":
        
    // case "notnull":
        
    // case "nuint":
        
    // case "on":
        
    // case "or":
        
    // case "orderby":
        
    // case "partial":
        
    // case "record":
        
    // case "remove":
        
    // case "required":
        
    // case "scoped":
        
    // case "select":
        
    // case "set":
        
    // case "unmanaged":
        
    // case "value":
        
    // case "var":
        
    // case "when":
        
    // case "where":
        
    // case "with":
        
    // ContextualKeywords-IsControl
    // ============================
    // case "yield":
        
    default:
        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
        return;
}*/








/*
var keywordList = new List<string>
        {
            // NonContextualKeywords-NonControl
            // ================================
            "abstract",
            "as",
            "base",
            "bool",
            "byte",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "decimal",
            "default",
            "delegate",
            "double",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "this",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            // NonContextualKeywords-IsControl
            // ===============================
            "break",
            "case",
            "continue",
            "do",
            "else",
            "for",
            "foreach",
            "goto",
            "if",
            "return",
            "switch",
            "throw",
            "while",
            // ContextualKeywords-NotControl
            // =============================
            "add",
            "and",
            "alias",
            "ascending",
            "args",
            "async",
            "await",
            "by",
            "descending",
            "dynamic",
            "equals",
            "file",
            "from",
            "get",
            "global",
            "group",
            "init",
            "into",
            "join",
            "let",
            "managed",
            "nameof",
            "nint",
            "not",
            "notnull",
            "nuint",
            "on",
            "or",
            "orderby",
            "partial",
            "record",
            "remove",
            "required",
            "scoped",
            "select",
            "set",
            "unmanaged",
            "value",
            "var",
            "when",
            "where",
            "with",
            // ContextualKeywords-IsControl
            // ============================
            "yield",
        };
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");
        
        // var hashSet = new HashSet<int>();
        
        foreach (var keyword in keywordList)
        {
            var sum = 0;
            foreach (var character in keyword)
            {
                sum += (int)character;
            }
            
            // if (!hashSet.Add(sum))
            // {
            //    Console.WriteLine($"!DUPLICATE!:{sum}/{keyword}");
            //}
            Console.WriteLine($"case {sum}: // {keyword}");
        }
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");
*/

/*var keywordList = new List<string>
        {
            // NonContextualKeywords-NonControl
            // ================================
            "abstract",
            "as",
            "base",
            "bool",
            "byte",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "decimal",
            "default",
            "delegate",
            "double",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "this",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            // NonContextualKeywords-IsControl
            // ===============================
            "break",
            "case",
            "continue",
            "do",
            "else",
            "for",
            "foreach",
            "goto",
            "if",
            "return",
            "switch",
            "throw",
            "while",
            // ContextualKeywords-NotControl
            // =============================
            "add",
            "and",
            "alias",
            "ascending",
            "args",
            "async",
            "await",
            "by",
            "descending",
            "dynamic",
            "equals",
            "file",
            "from",
            "get",
            "global",
            "group",
            "init",
            "into",
            "join",
            "let",
            "managed",
            "nameof",
            "nint",
            "not",
            "notnull",
            "nuint",
            "on",
            "or",
            "orderby",
            "partial",
            "record",
            "remove",
            "required",
            "scoped",
            "select",
            "set",
            "unmanaged",
            "value",
            "var",
            "when",
            "where",
            "with",
            // ContextualKeywords-IsControl
            // ============================
            "yield",
        };
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");
        
        // var hashSet = new HashSet<int>();
        
        var longestKeyword = string.Empty;
        
        foreach (var keyword in keywordList)
        {
            if (keyword.Length > longestKeyword.Length)
                longestKeyword = keyword;
        
            var sum = 0;
            foreach (var character in keyword)
            {
                sum += (int)character;
            }
            
            // if (!hashSet.Add(sum))
            // {
            //    Console.WriteLine($"!DUPLICATE!:{sum}/{keyword}");
            //}
            Console.WriteLine($"case {sum}: // {keyword}");
        }
        
        Console.WriteLine($"{longestKeyword} {longestKeyword.Length}");
        
        Console.WriteLine("=======================");
        Console.WriteLine("=======================");*/











