using System.Text;
using Walk.Common.RazorLib;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.CompilerServices;

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
    public bool IsEof => CurrentCharacter == ParserFacts.END_OF_FILE;
    
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
            return ParserFacts.END_OF_FILE;

        return SourceText[PositionIndex++];
    }

    /// <summary>If (<see cref="PositionIndex" /> + <see cref="offset" />) is within bounds of the <see cref="SourceText" />.<br /><br />Then the character within the string <see cref="SourceText" /> at index of (<see cref="PositionIndex" /> + <see cref="offset" />) is returned and <see cref="PositionIndex" /> is unchanged.<br /><br />Otherwise, <see cref="ParserFacts.END_OF_FILE" /> is returned and the value of <see cref="PositionIndex" /> is unchanged.<br /><br />offset must be > -1</summary>
    public char PeekCharacter(int offset)
    {
        if (offset <= -1)
            throw new WalkTextEditorException($"{nameof(offset)} must be > -1");

        if (PositionIndex + offset >= SourceText.Length)
            return ParserFacts.END_OF_FILE;

        return SourceText[PositionIndex + offset];
    }

    /// <summary>If <see cref="PositionIndex" /> being decremented by 1 would result in <see cref="PositionIndex" /> being less than 0.<br /><br />Then <see cref="ParserFacts.END_OF_FILE" /> will be returned and <see cref="PositionIndex" /> will be left unchanged.<br /><br />Otherwise, <see cref="PositionIndex" /> will be decremented by one and the character within the string <see cref="SourceText" /> at index of <see cref="PositionIndex" /> is returned.</summary>
    public char BacktrackCharacter()
    {
        if (PositionIndex == 0)
            return ParserFacts.END_OF_FILE;

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
            if (ReadCharacter() == ParserFacts.END_OF_FILE)
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

            if (BacktrackCharacter() == ParserFacts.END_OF_FILE)
                break;
        }
    }

    public string PeekNextWord()
    {
        _stringBuilder.Clear();

        var i = 0;

        char peekedChar;

        do
        {
            peekedChar = PeekCharacter(i++);

            if (WhitespaceFacts.ALL_LIST.Contains(peekedChar) ||
                CommonFacts.IsPunctuationCharacter(peekedChar))
            {
                break;
            }

            _stringBuilder.Append(peekedChar);
        } while (peekedChar != ParserFacts.END_OF_FILE);
        
        // 0 of 9,997 allocations
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringWalker_StringAllocation++;
        return _stringBuilder.ToString();
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

    public bool PeekForSubstringRange(List<string> substringsList, out string? matchedOn)
    {
        foreach (var substring in substringsList)
        {
            if (PeekForSubstring(substring))
            {
                matchedOn = substring;
                return true;
            }
        }

        matchedOn = null;
        return false;
    }

    /// <summary>
    /// Provide <see cref="whitespaceOverrideList"/> to override the
    /// default of what qualifies as whitespace.
    /// The default whitespace chars are: <see cref="WhitespaceFacts.ALL_LIST"/>
    /// </summary>
    public void SkipWhitespace(IEnumerable<char>? whitespaceOverrideList = null)
    {
        var whitespaceCharacterList = whitespaceOverrideList ?? WhitespaceFacts.ALL_LIST;

        while (whitespaceCharacterList.Contains(CurrentCharacter))
        {
            if (ReadCharacter() == ParserFacts.END_OF_FILE)
                break;
        }
    }

    /// <Summary>
    /// Ex: '1.73', positive only.<br/>
    /// { 0, ..., 1, ..., 2, ...}
    /// </Summary>
    public TextEditorTextSpan ReadUnsignedNumericLiteral()
    {
        var startingPosition = PositionIndex;
        var seenPeriod = false;

        while (!IsEof)
        {
            if (!char.IsDigit(CurrentCharacter))
            {
                if (CurrentCharacter == '.' && !seenPeriod)
                    seenPeriod = true;
                else
                    break;
            }

            _ = ReadCharacter();
        }

        // 6 of 9,997 allocations
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringWalker_StringAllocation++;
        return new TextEditorTextSpan(startingPosition, PositionIndex, 0);
    }

    public string ReadUntil(char deliminator)
    {
        _stringBuilder.Clear();

        while (!IsEof)
        {
            if (CurrentCharacter == deliminator)
                break;

            _stringBuilder.Append(ReadCharacter());
        }

        // 147 of 9,997 allocations
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringWalker_StringAllocation++;
        return _stringBuilder.ToString();
    }
    
    public void SkipUntil(char deliminator)
    {
        while (!IsEof)
        {
            if (CurrentCharacter == deliminator)
                break;

            ReadCharacter();
        }
    }

    /// <summary>
    /// The line ending is NOT included in the returned string
    /// </summary>
    public string ReadLine()
    {
        _stringBuilder.Clear();

        while (!IsEof)
        {
            if (WhitespaceFacts.LINE_ENDING_CHARACTER_LIST.Contains(CurrentCharacter))
                break;

            _stringBuilder.Append(ReadCharacter());
        }
        
        // 69 of 9,997 allocations
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringWalker_StringAllocation++;
        return _stringBuilder.ToString();
    }

    /// <summary>
    /// This method will return immediately upon encountering whitespace.
    /// Returns a text span with its <see cref="TextEditorTextSpan.StartInclusiveIndex"/> equal to '-1' if no word was found.
    /// </summary>
    public (TextEditorTextSpan textSpan, string value) ReadWordTuple(IReadOnlyList<char>? additionalCharactersToBreakOnList = null)
    {
        additionalCharactersToBreakOnList ??= Array.Empty<char>();

        // The wordBuilder is appended to everytime a character is consumed.
        _stringBuilder.Clear();

        // wordBuilderStartInclusiveIndex == -1 is to mean that wordBuilder is empty.
        var wordBuilderStartInclusiveIndex = -1;

        while (!IsEof)
        {
            if (WhitespaceFacts.ALL_LIST.Contains(CurrentCharacter) ||
                additionalCharactersToBreakOnList.Contains(CurrentCharacter))
            {
                break;
            }

            if (wordBuilderStartInclusiveIndex == -1)
            {
                // This is the start of a word as opposed to the continuation of a word
                wordBuilderStartInclusiveIndex = PositionIndex;
            }

            _stringBuilder.Append(CurrentCharacter);

            _ = ReadCharacter();
        }
        
        // 9 of 9,997 allocations
        Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringWalker_StringAllocation++;
        return (new TextEditorTextSpan(
                wordBuilderStartInclusiveIndex,
                PositionIndex,
                0),
            _stringBuilder.ToString());
    }
}
