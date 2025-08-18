using System.Text;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.Xml;

public struct XmlOutputReader
{
    public XmlOutputReader(/*int index, */List<TextEditorTextSpan> textSpanList)
    {
        // Index = index;
        TextSpanList = textSpanList;
    }

    // public int Index { get; set; }
    public List<TextEditorTextSpan> TextSpanList { get; }

    /// <summary>
    /// shouldIncludeFullMissLines: if the 'targetTagNameOpenString' was matched, but neither target attribute was found
    /// on the tag, still add the result to the List even though both attribute values will be null.
    ///
    /// getTextBuffer: a char array with capacity of 1.
    /// </summary>
    public void FindTagGetEitherOrBothAttributeValue(
        string targetTagNameOpenString,
        string targetAttributeOne,
        string targetAttributeTwo,
        bool shouldIncludeFullMissLines,
        StreamReader sr,
        StringBuilder stringBuilder,
        char[] getTextBuffer,
        List<(string ValueAttributeOne, string ValueAttributeTwo)> output)
    {
        for (int indexTextSpan = 0; indexTextSpan < TextSpanList.Count; indexTextSpan++)
        {
            var textSpan = TextSpanList[indexTextSpan];
            var decorationKind = (XmlDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == XmlDecorationKind.TagNameOpen)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagNameOpenString = stringBuilder.ToString();
            
                if (tagNameOpenString == targetTagNameOpenString)
                {
                    string? valueAttributeOne = null;
                    string? valueAttributeTwo = null;
                
                    while (indexTextSpan < TextSpanList.Count - 1)
                    {
                        if ((XmlDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeName)
                        {
                            var attributeNameTextSpan = TextSpanList[indexTextSpan + 1];
                            ++indexTextSpan;
                            
                            sr.BaseStream.Seek(attributeNameTextSpan.ByteIndex, SeekOrigin.Begin);
                            sr.DiscardBufferedData();
                            stringBuilder.Clear();
                            for (int i = 0; i < attributeNameTextSpan.Length; i++)
                            {
                                sr.Read(getTextBuffer, 0, 1);
                                stringBuilder.Append(getTextBuffer[0]);
                            }
                            var attributeNameString = stringBuilder.ToString();
                            
                            while (indexTextSpan < TextSpanList.Count - 1)
                            {
                                var nextDecorationKind = (XmlDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte;
                                
                                if (nextDecorationKind == XmlDecorationKind.AttributeOperator)
                                {
                                    ++indexTextSpan;
                                }
                                else if (nextDecorationKind == XmlDecorationKind.AttributeDelimiter)
                                {
                                    ++indexTextSpan;
                                }
                                else if (nextDecorationKind == XmlDecorationKind.AttributeValue)
                                {
                                    var attributeValueTextSpan = TextSpanList[indexTextSpan + 1];
                                    
                                    sr.BaseStream.Seek(attributeValueTextSpan.ByteIndex, SeekOrigin.Begin);
                                    sr.DiscardBufferedData();
                                    stringBuilder.Clear();
                                    for (int i = 0; i < attributeValueTextSpan.Length; i++)
                                    {
                                        sr.Read(getTextBuffer, 0, 1);
                                        stringBuilder.Append(getTextBuffer[0]);
                                    }
                                    
                                    if (attributeNameString == targetAttributeOne)
                                    {
                                        if (valueAttributeOne is null)
                                            valueAttributeOne = stringBuilder.ToString();
                                    }
                                    else if (attributeNameString == targetAttributeTwo)
                                    {
                                        if (valueAttributeTwo is null)
                                            valueAttributeTwo = stringBuilder.ToString();
                                    }
                                    
                                    ++indexTextSpan;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if ((XmlDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    
                    if (valueAttributeOne is not null || valueAttributeTwo is not null || shouldIncludeFullMissLines)
                        output.Add(new(valueAttributeOne, valueAttributeTwo));
                }
            }
        }
    }
}
