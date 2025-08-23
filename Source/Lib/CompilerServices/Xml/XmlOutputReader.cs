using System.Text;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;

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
    /// targetTagName: will match on any open tag, or a self closing tag; that have this text as their tag name.
    ///
    /// shouldIncludeFullMissLines: if the 'targetTagNameOpenString' was matched, but neither target attribute was found
    /// on the tag, still add the result to the List even though both attribute values will be null.
    ///
    /// getTextBuffer: a char array with capacity of 1.
    /// </summary>
    public void FindTagGetAttributeValue(
        string targetTagName,
        string targetAttributeOne,
        bool shouldIncludeFullMissLines,
        StreamReader sr,
        StringBuilder stringBuilder,
        char[] getTextBuffer,
        List<string> output,
        string? attributeValueMustEndsWith = null)
    {
        for (int indexTextSpan = 0; indexTextSpan < TextSpanList.Count; indexTextSpan++)
        {
            var textSpan = TextSpanList[indexTextSpan];
            var decorationKind = (GenericDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == GenericDecorationKind.Xml_TagNameOpen || decorationKind == GenericDecorationKind.Xml_TagNameSelf)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagName = stringBuilder.ToString();
            
                if (tagName != targetTagName)
                    continue;
                
                string? valueAttributeOne = null;
            
                while (indexTextSpan < TextSpanList.Count - 1)
                {
                    if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeName)
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
                            var nextDecorationKind = (GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte;
                            
                            if (nextDecorationKind == GenericDecorationKind.Xml_AttributeOperator)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeValue)
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
                                    {
                                        var temporaryValue = stringBuilder.ToString();
                                    
                                        if (attributeValueMustEndsWith is null)
                                        {
                                            valueAttributeOne = temporaryValue;
                                        }
                                        else
                                        {
                                            if (temporaryValue.EndsWith(attributeValueMustEndsWith))
                                                valueAttributeOne = temporaryValue;
                                        }
                                    }
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
                        if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeDelimiter)
                        {
                            ++indexTextSpan;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if (valueAttributeOne is not null || shouldIncludeFullMissLines)
                    output.Add(valueAttributeOne);
            }
        }
    }

    /// <summary>
    /// targetTagName: will match on any open tag, or a self closing tag; that have this text as their tag name.
    ///
    /// shouldIncludeFullMissLines: if the 'targetTagNameOpenString' was matched, but neither target attribute was found
    /// on the tag, still add the result to the List even though both attribute values will be null.
    ///
    /// getTextBuffer: a char array with capacity of 1.
    /// </summary>
    public void FindTagGetEitherOrBothAttributeValue(
        string targetTagName,
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
            var decorationKind = (GenericDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == GenericDecorationKind.Xml_TagNameOpen || decorationKind == GenericDecorationKind.Xml_TagNameSelf)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagName = stringBuilder.ToString();
            
                if (tagName != targetTagName)
                    continue;
                
                string? valueAttributeOne = null;
                string? valueAttributeTwo = null;
            
                while (indexTextSpan < TextSpanList.Count - 1)
                {
                    if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeName)
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
                            var nextDecorationKind = (GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte;
                            
                            if (nextDecorationKind == GenericDecorationKind.Xml_AttributeOperator)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeValue)
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
                        if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeDelimiter)
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
    
    /// <summary>
    /// ---------------------------------------------------------------------------
    /// <Folder Name="/eng/" />
    /// ---------------------------------------------------------------------------
    /// The above XML which is demarcated by dashed lines will output a tuple entry of:
    ///     ("/eng/", new List<string> { /* an empty list */ })
    ///
    ///
    /// ===========================================================================
    /// ===========================================================================
    ///
    ///
    /// ---------------------------------------------------------------------------
    /// <Folder Name="/eng/tools/" Id="daae2ffb-70a9-dcef-23a0-0abaed0a9720">
    ///     <Project Path="eng/tools/BaselineGenerator/BaselineGenerator.csproj" />
    ///     <Project Path="eng/tools/RepoTasks/RepoTasks.csproj" />
    /// </Folder>
    /// ---------------------------------------------------------------------------
    /// The above XML which is demarcated by dashed lines will output a tuple entry of:
    ///     ("/eng/", new List<string> { "eng/tools/BaselineGenerator/BaselineGenerator.csproj", "eng/tools/RepoTasks/RepoTasks.csproj" })
    ///
    ///
    /// ===========================================================================
    /// ===========================================================================
    ///
    /// A parent tag that is nested inside a parent tag will not be traversed. It will just be skipped over.
    /// </summary>
    public void CollectParentChildrenRelationship(
        string parentTagName,
        string parentAttributeName,
        string childTagName,
        string childAttributeName,
        StreamReader sr,
        StringBuilder stringBuilder,
        char[] getTextBuffer,
        List<(string Name, List<string> ChildProjectRelativePathList)> folderTupleList)
    {
        var withinFolder = false;
        var childProjectRelativePathList = new List<string>();
    
        for (int indexTextSpan = 0; indexTextSpan < TextSpanList.Count; indexTextSpan++)
        {
            var textSpan = TextSpanList[indexTextSpan];
            var decorationKind = (GenericDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == GenericDecorationKind.Xml_TagNameOpen || decorationKind == GenericDecorationKind.Xml_TagNameSelf)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagName = stringBuilder.ToString();
            
                if (tagName != parentTagName)
                    continue;
                
                string? valueAttributeOne = null;
            
                while (indexTextSpan < TextSpanList.Count - 1)
                {
                    if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeName)
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
                            var nextDecorationKind = (GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte;
                            
                            if (nextDecorationKind == GenericDecorationKind.Xml_AttributeOperator)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeValue)
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
                                
                                if (attributeNameString == parentAttributeName)
                                {
                                    if (valueAttributeOne is null)
                                        valueAttributeOne = stringBuilder.ToString();
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
                        if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeDelimiter)
                        {
                            ++indexTextSpan;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if (valueAttributeOne is not null)
                {
                    folderTupleList.Add((valueAttributeOne, childProjectRelativePathList));
                    
                    if (decorationKind == GenericDecorationKind.Xml_TagNameOpen)
                    {
                        indexTextSpan = Until(
                            parentTagName,
                            parentAttributeName,
                            childTagName,
                            childAttributeName,
                            sr,
                            stringBuilder,
                            getTextBuffer,
                            childProjectRelativePathList,
                            indexTextSpan);
                    }
                    
                    childProjectRelativePathList = new List<string>();
                }
            }
        }
    }
    
    private int Until(
        string parentTagName,
        string parentAttributeName,
        string childTagName,
        string childAttributeName,
        StreamReader sr,
        StringBuilder stringBuilder,
        char[] getTextBuffer,
        List<string> childProjectRelativePathList,
        int indexTextSpan)
    {
        for (; indexTextSpan < TextSpanList.Count; indexTextSpan++)
        {
            var textSpan = TextSpanList[indexTextSpan];
            var decorationKind = (GenericDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == GenericDecorationKind.Xml_TagNameOpen || decorationKind == GenericDecorationKind.Xml_TagNameSelf)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagName = stringBuilder.ToString();
            
                if (tagName != childTagName)
                    continue;
                
                string? valueAttributeOne = null;
            
                while (indexTextSpan < TextSpanList.Count - 1)
                {
                    if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeName)
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
                            var nextDecorationKind = (GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte;
                            
                            if (nextDecorationKind == GenericDecorationKind.Xml_AttributeOperator)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else if (nextDecorationKind == GenericDecorationKind.Xml_AttributeValue)
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
                                
                                if (attributeNameString == childAttributeName)
                                {
                                    if (valueAttributeOne is null)
                                        valueAttributeOne = stringBuilder.ToString();
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
                        if ((GenericDecorationKind)TextSpanList[indexTextSpan + 1].DecorationByte == GenericDecorationKind.Xml_AttributeDelimiter)
                        {
                            ++indexTextSpan;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if (valueAttributeOne is not null)
                    childProjectRelativePathList.Add(valueAttributeOne);
            }
            else if (decorationKind == GenericDecorationKind.Xml_TagNameClose)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagName = stringBuilder.ToString();
            
                if (tagName == parentTagName)
                    return indexTextSpan;
            }
        }
        
        return indexTextSpan;
    }
    
    public void ConsoleWriteFormatted(
        StreamReader sr,
        StringBuilder stringBuilder,
        char[] getTextBuffer)
    {
        foreach (var textSpan in TextSpanList)
        {
            sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
            sr.DiscardBufferedData();
        
            stringBuilder.Clear();
        
            for (int i = 0; i < textSpan.Length; i++)
            {
                sr.Read(getTextBuffer, 0, 1);
                stringBuilder.Append(getTextBuffer[0]);
            }
        
            var decorationKind = (GenericDecorationKind)textSpan.DecorationByte;
            
            switch (decorationKind)
            {
                case GenericDecorationKind.Xml_Text:
                    Console.Write(decorationKind + "              ");
                    break;
                case GenericDecorationKind.Xml_TagNameOpen:
                    Console.Write(decorationKind + "       ");
                    break;
                case GenericDecorationKind.Xml_TagNameClose:
                    Console.Write(decorationKind + "      ");
                    break;
                case GenericDecorationKind.Xml_TagNameSelf:
                    Console.Write(decorationKind + "       ");
                    break;
                case GenericDecorationKind.Xml_AttributeName:
                    Console.Write(decorationKind + "     ");
                    break;
                case GenericDecorationKind.Xml_AttributeValue:
                    Console.Write(decorationKind + "    ");
                    break;
                case GenericDecorationKind.Xml_AttributeOperator:
                    Console.Write(decorationKind + " ");
                    break;
                case GenericDecorationKind.Xml_AttributeDelimiter:
                    Console.Write(decorationKind);
                    break;
                default:
                    Console.Write(decorationKind);
                    break;
            }
        
            Console.WriteLine($" | {stringBuilder.ToString()}");
        }
    }
}
