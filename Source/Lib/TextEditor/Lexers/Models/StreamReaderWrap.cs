using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Exceptions;

namespace Walk.TextEditor.RazorLib.Lexers.Models;

public class StreamReaderWrap
{
    public StreamReaderWrap()
    {
    }
    
    public StreamReaderWrap(StreamReader streamReader)
    {
        StreamReader = streamReader;
        StreamReader.Read(_streamReaderCharBuffer);
    }
    
    public void ReInitialize(StreamReader streamReader)
    {
        // You probably don't have to set these to default because they just get overwritten when the time comes.
        // But I'm unsure, and there is far more valuable changes to be made so I'm just gonna set them to default.
        _peekBuffer[0] = default;
        _peekBuffer[1] = default;
        _peekBuffer[2] = default;
        
        _peekIndex = -1;
        _peekSize = 0;
        
        _backtrackTuple = default;

        StreamReader = streamReader;

        LastReadCharacterCount = 1;

        _streamPositionIndex = default;
        _streamByteIndex = default;
        
        StreamReader.Read(_streamReaderCharBuffer);
    }

    private char[] _streamReaderCharBuffer = new char[1];

    /// <summary>
    /// I'm pretty sure the PositionIndex is '_positionIndex - PeekSize;'
    /// But I'm adding it here cause I'm tired and don't want to end up in a possible rabbit hole over this right now.
    /// </summary>
    private (char Character, int PositionIndex, int ByteIndex)[] _peekBuffer = new (char Character, int PositionIndex, int ByteIndex)[3]; // largest Peek is 2
    private int _peekIndex = -1;
    private int _peekSize = 0;
    
    private (char Character, int PositionIndex, int ByteIndex) _backtrackTuple;

    public StreamReader StreamReader { get; private set; }

    /// <summary>
    /// The count is unreliable (only accurate when the most recent ReadCharacter() came from the StreamReader.
    /// The main purpose is to check if it is non-zero to indicate you are NOT at the end of the file.
    /// </summary>
    public int LastReadCharacterCount { get; set; } = 1;

    private int _streamPositionIndex;
    public int PositionIndex
    {
        get
        {
            if (_peekIndex == -1)
            {
                return _streamPositionIndex;
            }
            else
            {
                return _peekBuffer[_peekIndex].PositionIndex;
            }
        }
        set
        {
            _streamPositionIndex = value;
        }
    }

    private int _streamByteIndex;
    public int ByteIndex
    {
        get
        {
            if (_peekIndex == -1)
            {
                return _streamByteIndex;
            }
            else
            {
                return _peekBuffer[_peekIndex].ByteIndex;
            }
        }
        set
        {
            _streamByteIndex = value;
        }
    }

    public bool IsEof
    {
        get
        {
            if (_peekIndex == -1)
            {
                return StreamReader.EndOfStream;
            }
            else
            {
                // peak immediate end of stream bad
                // peak overlap end of stream bad

                return false;
            }
        }
    }

    public char CurrentCharacter
    {
        get
        {
            if (_peekIndex == -1)
            {
                return _streamReaderCharBuffer[0];
            }
            else
            {
                return _peekBuffer[_peekIndex].Character;
            }
        }
    }

    public char NextCharacter
    {
        get
        {
            if (_peekIndex == -1)
            {
                return PeekCharacter(1);
            }
            else
            {
                if (_peekIndex + 1 < _peekSize)
                {
                    return _peekBuffer[_peekIndex + 1].Character;
                }
                else
                {
                    return _streamReaderCharBuffer[0];
                }
            }
        }
    }

    public char ReadCharacter()
    {
        if (_peekIndex != -1)
        {
            _backtrackTuple = _peekBuffer[_peekIndex++];

            if (_peekIndex >= _peekSize)
            {
                _peekIndex = -1;
                _peekSize = 0;
            }

            LastReadCharacterCount = 1;
        }
        else
        {
            if (StreamReader.EndOfStream)
                return ParserFacts.END_OF_FILE;

            // This is duplicated more than once inside the Peek(int) code.

            _backtrackTuple = (_streamReaderCharBuffer[0], PositionIndex, ByteIndex);

            _streamPositionIndex++;
            _streamByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);

            LastReadCharacterCount = StreamReader.Read(_streamReaderCharBuffer, 0, 1);
        }
        
        return CurrentCharacter;
    }

    public char PeekCharacter(int offset)
    {
        // Peek(1)
        // -------
        //
        // 
        // The '=' represents the StreamReader
        // The '+' represents the "peek buffer position".
        //
        //
        // Before state
        // ------------
        // Abcd
        //  =
        //
        //
        // After state
        // -----------
        // Abcd
        //  +=
        //
        // 
        // The _peekCount = 1
        // The _peekIndex is _peekCount - 1
        //
        // 


        if (offset <= -1)
            throw new WalkTextEditorException($"{nameof(offset)} must be > -1");
        if (offset == 0)
            return _streamReaderCharBuffer[0];

        if (_peekIndex != -1)
        {
            // This means a Peek() was performed,
            // then before the PeekBuffer was fully traversed
            // another peek occurred.
            //
            // I'm hoping that this case just doesn't occur in the Lexer at the moment
            // because I'm quite tired.

            // Followup: this did happen
            // so I'm splitting by cases
            //
            // - Second Peek(int) is within PeekSize
            // - Second Peek(int) is currentCharacter
            // - ...

            if (_peekIndex + offset < _peekSize)
            {
                throw new NotImplementedException();
            }
            // This 'else if' is probably wrong.
            else if (_peekIndex + offset == _peekSize)
            {
                return _streamReaderCharBuffer[0];
            }
            else
            {
                if (_peekIndex == 0)
                {
                    // Note: this says '<' NOT '<=' (which is probably what people would expect)...
                    // ...I'm tired and worried so I'm just moving extremely one by one.
                    // I know the less than works, maybe the less than equal to works.
                    // I mean it probably should work but I have a somewhat empty fuel tank right now.
                    if (_peekSize < _peekBuffer.Length)
                    {
                        if (_peekSize == 1 && offset == 2)
                        {
                            _peekBuffer[_peekSize] = (_streamReaderCharBuffer[0], PositionIndex, ByteIndex);
                            _peekSize++;

                            // This is duplicated inside the ReadCharacter() code.

                            _streamPositionIndex++;
                            _streamByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
                            StreamReader.Read(_streamReaderCharBuffer);
                            return _streamReaderCharBuffer[0];
                        }
                    }
                }

                throw new NotImplementedException();
            }
        }

        for (int i = 0; i < offset; i++)
        {
            // TODO: Peek() before any Read()

            _peekBuffer[i] = (_streamReaderCharBuffer[0], PositionIndex, ByteIndex);
            _peekIndex++;
            _peekSize++;

            // This is duplicated inside the ReadCharacter() code.

            _streamPositionIndex++;
            _streamByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
            StreamReader.Read(_streamReaderCharBuffer);
        }

        // TODO: Peek EOF
        // TODO: Peek overlap EOF
        return _streamReaderCharBuffer[0];
    }

    /// <summary>
    /// Backtrack is somewhat a sub-case of Peek(int)
    /// </summary>
    public void BacktrackCharacterNoReturnValue()
    {
        if (_peekIndex != -1)
        {
            // This means a Peek() was performed,
            // then before the PeekBuffer was fully traversed
            // another peek occurred.
            //
            // I'm hoping that this case just doesn't occur in the Lexer at the moment
            // because I'm quite tired.
            throw new NotImplementedException();
        }

        if (PositionIndex == 0)
            return;

        // This code is a repeat of the Peek() method's for loop but for one iteration

        _peekBuffer[0] = _backtrackTuple;
        _peekIndex++;
        _peekSize++;

        /*
        // This is duplicated inside the ReadCharacter() code.
        StreamReader.Read(_streamReaderCharBuffer);
        _streamPositionIndex++;
        _streamByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
        */
    }

    public void SkipRange(int length)
    {
        for (var i = 0; i < length; i++)
        {
            if (ReadCharacter() == ParserFacts.END_OF_FILE)
                break;
        }
    }
}
