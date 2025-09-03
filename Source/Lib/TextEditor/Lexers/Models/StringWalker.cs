using System.Text;
using Walk.TextEditor.RazorLib.Exceptions;

namespace Walk.TextEditor.RazorLib.Lexers.Models;

/// <summary>Provides common API that can be used when implementing an <see cref="ITextEditorLexer" /> for the <see cref="TextEditorModel" />.<br /><br />The marker for an out of bounds read is <see cref="ParserFacts.END_OF_FILE" />.</summary>
public class StringWalker
{
    private readonly StringBuilder _stringBuilder = new();

    /// <summary>See: Initialize(...)</summary>
    public StringWalker()
    {
    }

    /// <summary>Pass in the <see cref="ResourceUri"/> of a file, and its text. One can pass in <see cref="string.Empty"/> for the <see cref="ResourceUri"/> if they are only working with the text itself.</summary>
    public StringWalker(ResourceUri resourceUri, string sourceText)
    {
        Initialize(resourceUri, sourceText);
    }

    /// <summary>The character index within the <see cref="SourceText" />.</summary>
    public int PositionIndex { get; private set; }

    /// <summary>The file that the <see cref="SourceText"/> came from.</summary>
    public ResourceUri ResourceUri { get; private set; }

    /// <summary>The text which is to be stepped through.</summary>
    public string SourceText { get; private set; }

    /// <summary>Returns <see cref="PeekCharacter" /> invoked with the value of zero</summary>
    public char CurrentCharacter => PeekCharacter(0);

    /// <summary>Returns <see cref="PeekCharacter" /> invoked with the value of one</summary>
    public char NextCharacter => PeekCharacter(1);

    /// <summary>Starting with <see cref="PeekCharacter" /> evaluated at 0 return that and the rest of the <see cref="SourceText" /><br /><br /><see cref="RemainingText" /> => SourceText.Substring(PositionIndex);</summary>
    public string RemainingText => SourceText[PositionIndex..];

    /// <summary>Returns if the current character is the end of file character</summary>
    public bool IsEof => CurrentCharacter == '\0';
    
    public void Initialize(ResourceUri resourceUri, string sourceText)
    {
        PositionIndex = 0;
        ResourceUri = resourceUri;
        SourceText = sourceText;
    }

    /// <summary>If <see cref="PositionIndex" /> is within bounds of the <see cref="SourceText" />.<br /><br />Then the character within the string <see cref="SourceText" /> at index of <see cref="PositionIndex" /> is returned and <see cref="PositionIndex" /> is incremented by one.<br /><br />Otherwise, <see cref="ParserFacts.END_OF_FILE" /> is returned and the value of <see cref="PositionIndex" /> is unchanged.</summary>
    public char ReadCharacter()
    {
        if (PositionIndex >= SourceText.Length)
            return '\0';

        return SourceText[PositionIndex++];
    }

    /// <summary>If (<see cref="PositionIndex" /> + <see cref="offset" />) is within bounds of the <see cref="SourceText" />.<br /><br />Then the character within the string <see cref="SourceText" /> at index of (<see cref="PositionIndex" /> + <see cref="offset" />) is returned and <see cref="PositionIndex" /> is unchanged.<br /><br />Otherwise, <see cref="ParserFacts.END_OF_FILE" /> is returned and the value of <see cref="PositionIndex" /> is unchanged.<br /><br />offset must be > -1</summary>
    public char PeekCharacter(int offset)
    {
        if (offset <= -1)
            throw new WalkTextEditorException($"{nameof(offset)} must be > -1");

        if (PositionIndex + offset >= SourceText.Length)
            return '\0';

        return SourceText[PositionIndex + offset];
    }

    /// <summary>If <see cref="PositionIndex" /> being decremented by 1 would result in <see cref="PositionIndex" /> being less than 0.<br /><br />Then <see cref="ParserFacts.END_OF_FILE" /> will be returned and <see cref="PositionIndex" /> will be left unchanged.<br /><br />Otherwise, <see cref="PositionIndex" /> will be decremented by one and the character within the string <see cref="SourceText" /> at index of <see cref="PositionIndex" /> is returned.</summary>
    public char BacktrackCharacter()
    {
        if (PositionIndex == 0)
            return '\0';

        PositionIndex--;

        return PeekCharacter(0);
    }
    
    public void BacktrackCharacterNoReturnValue()
    {
        if (PositionIndex == 0)
            return;

        PositionIndex--;
    }

    /// <summary>Iterates a counter from 0 until the counter is equal to <see cref="length" />.<br /><br />Each iteration <see cref="ReadCharacter" /> will be invoked.<br /><br />If an iteration's invocation of <see cref="ReadCharacter" /> returned <see cref="ParserFacts.END_OF_FILE" /> then the method will short circuit and return regardless of whether it finished iterating to <see cref="length" /> or not.</summary>
    public void SkipRange(int length)
    {
        for (var i = 0; i < length; i++)
        {
            if (ReadCharacter() == '\0')
                break;
        }
    }

    /// <summary>Iterates a counter from 0 until the counter is equal to <see cref="length" />.<br /><br />Each iteration <see cref="BacktrackCharacter" /> will be invoked using the.<br /><br />If an iteration's invocation of <see cref="BacktrackCharacter" /> returned <see cref="ParserFacts.END_OF_FILE" /> then the method will short circuit and return regardless of whether it finished iterating to <see cref="length" /> or not.</summary>
    public void BacktrackRange(int length)
    {
        for (var i = 0; i < length; i++)
        {
            if (PositionIndex == 0)
                return;

            if (BacktrackCharacter() == '\0')
                break;
        }
    }

    /// <summary>Form a substring of the <see cref="SourceText" /> that starts inclusively at the index <see cref="PositionIndex" /> and has a maximum length of <see cref="substring" />.Length.<br /><br />This method uses <see cref="PeekRange" /> internally and therefore will return a string that ends with <see cref="ParserFacts.END_OF_FILE" /> if an index out of bounds read was performed on <see cref="SourceText" /></summary>
    public bool PeekForSubstring(string substring)
    {
        var isMatch = true;
    
        for (var i = 0; i < substring.Length; i++)
        {
            if (PeekCharacter(i) != substring[i])
            {
                isMatch = false;
                break;
            }
        }

        return isMatch;
    }
    
    /// <summary>
    /// Provide <see cref="whitespaceOverrideList"/> to override the
    /// default of what qualifies as whitespace.
    /// The default whitespace chars are: <see cref="WhitespaceFacts.ALL_LIST"/>
    /// </summary>
    public void SkipWhitespace()
    {
        while (CurrentCharacter == ' ' ||
               CurrentCharacter == '\t' ||
               CurrentCharacter == '\r' ||
               CurrentCharacter == '\n')
        {
            if (ReadCharacter() == '\0')
                break;
        }
    }
}
