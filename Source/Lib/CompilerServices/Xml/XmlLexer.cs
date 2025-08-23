using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.CompilerServices.Xml;

public static class XmlLexer
{
    public enum XmlLexerContextKind
    {
        Expect_TagOrText,
        // Expect_TagName, // There is no expect tag name, you can't have whitespace here
        Expect_AttributeName,
        Expect_AttributeValue,
    }

    public static XmlLexerOutput Lex(StreamReaderWrap streamReaderWrap)
    {
        var context = XmlLexerContextKind.Expect_TagOrText;
        var output = new XmlLexerOutput();
        
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
                    if (context == XmlLexerContextKind.Expect_AttributeName)
                    {
                        var attributeNameStartPosition = streamReaderWrap.PositionIndex;
                        var attributeNameStartByte = streamReaderWrap.ByteIndex;
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
                            attributeNameStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeName,
                            attributeNameStartByte));
                        context = XmlLexerContextKind.Expect_AttributeValue;
                        break;
                    }
                    else if (context == XmlLexerContextKind.Expect_AttributeValue)
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
                            (byte)GenericDecorationKind.Xml_AttributeValue,
                            attributeValueStartByte));
                        context = XmlLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    else if (context == XmlLexerContextKind.Expect_TagOrText)
                    {
                        var textStartPosition = streamReaderWrap.PositionIndex;
                        var textStartByte = streamReaderWrap.ByteIndex;
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter == '<')
                            {
                                break;
                            }
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            textStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_Text,
                            textStartByte));
                        context = XmlLexerContextKind.Expect_TagOrText;
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
                    if (context == XmlLexerContextKind.Expect_AttributeValue)
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
                            (byte)GenericDecorationKind.Xml_AttributeValue,
                            attributeValueStartByte));
                        context = XmlLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    
                    goto default;
                case '\'':
                    if (context == XmlLexerContextKind.Expect_AttributeValue)
                    {
                        var delimiterStartPosition = streamReaderWrap.PositionIndex;
                        var delimiterStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeDelimiter,
                            delimiterStartByte));
                            
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        var attributeValueEnd = streamReaderWrap.PositionIndex;
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
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            attributeValueStartPosition,
                            attributeValueEnd,
                            (byte)GenericDecorationKind.Xml_AttributeValue,
                            attributeValueStartByte));
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeDelimiter,
                            delimiterStartByte));
                        context = XmlLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    goto default;
                case '"':
                    if (context == XmlLexerContextKind.Expect_AttributeValue)
                    {
                        var delimiterStartPosition = streamReaderWrap.PositionIndex;
                        var delimiterStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeDelimiter,
                            delimiterStartByte));
                        
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        var attributeValueEnd = streamReaderWrap.PositionIndex;
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
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            attributeValueStartPosition,
                            attributeValueEnd,
                            (byte)GenericDecorationKind.Xml_AttributeValue,
                            attributeValueStartByte));
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            delimiterStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeDelimiter,
                            delimiterStartByte));
                        context = XmlLexerContextKind.Expect_AttributeName;
                        break;
                    }
                    goto default;
                case '/':
                
                    if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        if (context == XmlLexerContextKind.Expect_AttributeName || context == XmlLexerContextKind.Expect_AttributeValue)
                        {
                            if (indexOfMostRecentTagOpen != -1)
                            {
                                output.TextSpanList[indexOfMostRecentTagOpen] = output.TextSpanList[indexOfMostRecentTagOpen] with
                                {
                                    DecorationByte = (byte)GenericDecorationKind.Xml_TagNameSelf,
                                };
                                indexOfMostRecentTagOpen = -1;
                            }
                            context = XmlLexerContextKind.Expect_TagOrText;
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
                    if (context == XmlLexerContextKind.Expect_AttributeValue)
                    {
                        var attributeValueStartPosition = streamReaderWrap.PositionIndex;
                        var attributeValueStartByte = streamReaderWrap.ByteIndex;
                        _ = streamReaderWrap.ReadCharacter();
                        output.TextSpanList.Add(new TextEditorTextSpan(
                            attributeValueStartPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Xml_AttributeOperator,
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
                    
                    var tagDecoration = (byte)GenericDecorationKind.Xml_TagNameOpen;
                    
                    if (context == XmlLexerContextKind.Expect_TagOrText)
                    {
                        _ = streamReaderWrap.ReadCharacter();
                        
                        if (streamReaderWrap.CurrentCharacter == '/')
                        {
                            tagDecoration = (byte)GenericDecorationKind.Xml_TagNameClose;
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
                        if (tagDecoration == (byte)GenericDecorationKind.Xml_TagNameOpen)
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
                            context = XmlLexerContextKind.Expect_TagOrText;
                        }
                        else
                        {
                            context = XmlLexerContextKind.Expect_AttributeName;
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
                    context = XmlLexerContextKind.Expect_TagOrText;
                
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
