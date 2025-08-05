using Walk.CompilerServices.CSharp.BinderCase;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.CSharp.LexerCase;

public static class CSharpLexer
{
    public class StreamReaderWrap
    {
        public StreamReaderWrap(StreamReader streamReader)
        {
            StreamReader = streamReader;
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

        public StreamReader StreamReader { get; }

        /// <summary>
        /// The count is unreliable (on accurate when the most recent ReadCharacter() came from the StreamReader.
        /// The main purpose is to check if it is non-zero to indicate you are NOT at the end of the file.
        /// </summary>
        public int LastReadCharacterCount { get; set; }

        private int _positionIndex;
        public int PositionIndex
        {
            get
            {
                if (_peekIndex == -1)
                {
                    return _positionIndex;
                }
                else
                {
                    return _peekBuffer[_peekIndex].PositionIndex;
                }
            }
            set
            {
                _positionIndex = value;
            }
        }

        private int _byteIndex;
        public int ByteIndex
        {
            get
            {
                if (_peekIndex == -1)
                {
                    return _byteIndex;
                }
                else
                {
                    return _peekBuffer[_peekIndex].ByteIndex;
                }
            }
            set
            {
                _byteIndex = value;
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

                PositionIndex++;
                ByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);

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

                                PositionIndex++;
                                ByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
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

                PositionIndex++;
                ByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
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
            PositionIndex++;
            ByteIndex += StreamReader.CurrentEncoding.GetByteCount(_streamReaderCharBuffer);
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

    /// <summary>
    /// Initialize the CSharpLexerOutput here, then start the while loop with 'Lex_Frame(...)'.
    /// </summary>
    public static CSharpLexerOutput Lex(CSharpBinder binder, string sourceText, StreamReader streamReader, bool shouldUseSharedStringWalker)
    {
        var lexerOutput = new CSharpLexerOutput(sourceText);
        
        var previousEscapeCharacterTextSpan = new TextEditorTextSpan(
            0,
            0,
            (byte)GenericDecorationKind.None);
            
        var interpolatedExpressionUnmatchedBraceCount = -1;

        var streamReaderWrap = new StreamReaderWrap(streamReader);

        Lex_Frame(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, ref interpolatedExpressionUnmatchedBraceCount);
        
        var endOfFileTextSpan = new TextEditorTextSpan(
            streamReaderWrap.PositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None,
            streamReaderWrap.ByteIndex);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, endOfFileTextSpan));
        return lexerOutput;
    }
    
    /// <summary>
    /// Isolate the while loop within its own function in order to permit recursion without allocating new state.
    /// </summary>
    public static void Lex_Frame(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        ref int interpolatedExpressionUnmatchedBraceCount)
    {
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
                    LexIdentifierOrKeywordOrKeywordContextual(binder, ref lexerOutput, streamReaderWrap);
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
                    LexNumericLiteralToken(binder, ref lexerOutput, streamReaderWrap);
                    break;
                case '\'':
                    LexCharLiteralToken(binder, ref lexerOutput, streamReaderWrap);
                    break;
                case '"':
                    LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 0, useVerbatim: false);
                    break;
                case '/':
                    if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        LexCommentSingleLineToken(ref lexerOutput, streamReaderWrap);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '*')
                    {
                        LexCommentMultiLineToken(ref lexerOutput, streamReaderWrap);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DivisionToken, textSpan));
                    }
                    break;
                case '+':
                    if (streamReaderWrap.PeekCharacter(1) == '+')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PlusPlusToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PlusToken, textSpan));
                    }
                    break;
                case '-':
                    if (streamReaderWrap.PeekCharacter(1) == '-')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MinusMinusToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MinusToken, textSpan));
                    }
                    break;
                case '=':
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsEqualsToken, textSpan));
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsCloseAngleBracketToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsToken, textSpan));
                    }
                    break;
                case '?':
                    if (streamReaderWrap.PeekCharacter(1) == '?')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.QuestionMarkQuestionMarkToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.QuestionMarkToken, textSpan));
                    }
                    break;
                case '|':
                    if (streamReaderWrap.PeekCharacter(1) == '|')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PipePipeToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PipeToken, textSpan));
                    }
                    break;
                case '&':
                    if (streamReaderWrap.PeekCharacter(1) == '&')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AmpersandToken, textSpan));
                    }
                    break;
                case '*':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StarToken, textSpan));
                    break;
                }
                case '!':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BangEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BangToken, textSpan));
                    }
                    break;
                }
                case ';':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StatementDelimiterToken, textSpan));
                    break;
                }
                case '(':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenParenthesisToken, textSpan));
                    break;
                }
                case ')':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseParenthesisToken, textSpan));
                    break;
                }
                case '{':
                {
                    if (interpolatedExpressionUnmatchedBraceCount != -1)
                        ++interpolatedExpressionUnmatchedBraceCount;
                
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenBraceToken, textSpan));
                    break;
                }
                case '}':
                {
                    if (interpolatedExpressionUnmatchedBraceCount != -1)
                    {
                        if (--interpolatedExpressionUnmatchedBraceCount <= 0)
                            goto forceExit;
                    }
                
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseBraceToken, textSpan));
                    break;
                }
                case '<':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenAngleBracketEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenAngleBracketToken, textSpan));
                    }
                    break;
                }
                case '>':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseAngleBracketEqualsToken, textSpan));
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseAngleBracketToken, textSpan));
                    }
                    break;
                }
                case '[':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OpenSquareBracketToken, textSpan));
                    break;
                }
                case ']':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CloseSquareBracketToken, textSpan));
                    break;
                }
                case '$':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: false);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '@' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: true);
                    }
                    else if (streamReaderWrap.NextCharacter == '$')
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        
                        // The while loop starts counting from and including the first dollar sign.
                        var countDollarSign = 0;
                    
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter != '$')
                                break;
                            
                            ++countDollarSign;
                            _ = streamReaderWrap.ReadCharacter();
                        }
                        
                        // Only the last '$' (dollar sign character) will be syntax highlighted
                        // if this code is NOT included.
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.StringLiteral);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringLiteralToken, textSpan));
                        
                        // From the LexString(...) method:
                        //     "awkwardly even if there are many of these it is expected
                        //      that the last one will not have been consumed."
                        streamReaderWrap.BacktrackCharacterNoReturnValue();
                        
                        if (streamReaderWrap.NextCharacter == '"')
                            LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: countDollarSign, useVerbatim: false);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DollarSignToken, textSpan));
                        break;
                    }
                    break;
                case '@':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 0, useVerbatim: true);
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '$' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: 1, useVerbatim: true);
                    }
                    else
                    {
                        var entryPositionIndex = streamReaderWrap.PositionIndex;
                        streamReaderWrap.ReadCharacter();
                        var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AtToken, textSpan));
                        break;
                    }
                    break;
                case ':':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ColonToken, textSpan));
                    break;
                }
                case '.':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.MemberAccessToken, textSpan));
                    break;
                }
                case ',':
                {
                    var entryPositionIndex = streamReaderWrap.PositionIndex;
                    streamReaderWrap.ReadCharacter();
                    var textSpan = new TextEditorTextSpan(entryPositionIndex, streamReaderWrap.PositionIndex, (byte)GenericDecorationKind.None);
                    lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CommaToken, textSpan));
                    break;
                }
                case '#':
                    LexPreprocessorDirectiveToken(ref lexerOutput, streamReaderWrap);
                    break;
                default:
                    _ = streamReaderWrap.ReadCharacter();
                    break;
            }
        }

        forceExit:
        return;
    }
    
    /// <summary>
    /// The invoker of this method is expected to count the amount of '$' (dollar sign characters).
    /// When it comes to raw strings however, this logic will counted inside of this method.
    ///
    /// The reason being: you don't know if it is a string until you've read all of the '$' (dollar sign characters).
    /// So in order to invoke this method the invoker had to have counted them.
    /// </summary>
    private static void LexString(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        int countDollarSign,
        bool useVerbatim)
    {
        // Interpolated expressions will be done recursively and added to this 'SyntaxTokenList'
        var syntaxTokenListIndex = lexerOutput.SyntaxTokenList.Count;
    
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        var useInterpolation = countDollarSign > 0;
        
        if (useInterpolation)
            _ = streamReaderWrap.ReadCharacter(); // Move past the '$' (dollar sign character); awkwardly even if there are many of these it is expected that the last one will not have been consumed.
        if (useVerbatim)
            _ = streamReaderWrap.ReadCharacter(); // Move past the '@' (at character)
        
        var useRaw = false;
        int countDoubleQuotes = 0;
        
        if (!useVerbatim && streamReaderWrap.PeekCharacter(1) == '\"' && streamReaderWrap.PeekCharacter(2) == '\"')
        {
            useRaw = true;
            
            // Count the amount of double quotes to be used as the delimiter.
            while (!streamReaderWrap.IsEof)
            {
                if (streamReaderWrap.CurrentCharacter != '\"')
                    break;
    
                ++countDoubleQuotes;
                _ = streamReaderWrap.ReadCharacter();
            }
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter(); // Move past the '"' (double quote character)
        }

        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == '\"')
            {
                if (useRaw)
                {
                    var matchDoubleQuotes = 0;
                    
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter != '\"')
                            break;
                        
                        _ = streamReaderWrap.ReadCharacter();
                        if (++matchDoubleQuotes == countDoubleQuotes)
                            goto foundEndDelimiter;
                    }
                    
                    continue;
                }
                else if (useVerbatim && streamReaderWrap.NextCharacter == '\"')
                {
                    EscapeCharacterListAdd(ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, new TextEditorTextSpan(
                        streamReaderWrap.PositionIndex,
                        streamReaderWrap.PositionIndex + 2,
                        (byte)GenericDecorationKind.EscapeCharacterPrimary));
    
                    _ = streamReaderWrap.ReadCharacter();
                }
                else
                {
                    _ = streamReaderWrap.ReadCharacter();
                    break;
                }
            }
            else if (!useVerbatim && streamReaderWrap.CurrentCharacter == '\\')
            {
                EscapeCharacterListAdd(ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex + 2,
                    (byte)GenericDecorationKind.EscapeCharacterPrimary));

                // Presuming the escaped text is 2 characters, then read an extra character.
                _ = streamReaderWrap.ReadCharacter();
            }
            else if (useInterpolation && streamReaderWrap.CurrentCharacter == '{')
            {
                // With raw, one is escaping by way of typing less.
                // With normal interpolation, one is escaping by way of typing more.
                //
                // Thus, these are two separate cases written as an if-else.
                if (useRaw)
                {
                    var interpolationTemporaryPositionIndex = streamReaderWrap.PositionIndex;
                    var matchBrace = 0;
                        
                    while (!streamReaderWrap.IsEof)
                    {
                        if (streamReaderWrap.CurrentCharacter != '{')
                            break;
                        
                        _ = streamReaderWrap.ReadCharacter();
                        if (++matchBrace >= countDollarSign)
                        {
                            // Found yet another '{' match beyond what was needed.
                            // So, this logic will match from the inside to the outside.
                            if (streamReaderWrap.CurrentCharacter == '{')
                            {
                                ++interpolationTemporaryPositionIndex;
                            }
                            else
                            {
                                // 'LexInterpolatedExpression' is expected to consume one more after it is finished.
                                // Thus, if this while loop were to consume, it would skip the
                                // closing double quotes if the expression were the last thing in the string.
                                //
                                // So, a backtrack is done.
                                LexInterpolatedExpression(
                                    binder,
                                    ref lexerOutput,
                                    streamReaderWrap,
                                    ref previousEscapeCharacterTextSpan,
                                    startInclusiveOpenDelimiter: interpolationTemporaryPositionIndex,
                                    countDollarSign: countDollarSign,
                                    useRaw);
                                streamReaderWrap.BacktrackCharacterNoReturnValue();
                            }
                        }
                    }
                }
                else
                {
                    if (streamReaderWrap.NextCharacter == '{')
                    {
                        _ = streamReaderWrap.ReadCharacter();
                    }
                    else
                    {
                        // 'LexInterpolatedExpression' is expected to consume one more after it is finished.
                        // Thus, if this while loop were to consume, it would skip the
                        // closing double quotes if the expression were the last thing in the string.
                        LexInterpolatedExpression(
                            binder,
                            ref lexerOutput,
                            streamReaderWrap,
                            ref previousEscapeCharacterTextSpan,
                            startInclusiveOpenDelimiter: streamReaderWrap.PositionIndex,
                            countDollarSign: countDollarSign,
                            useRaw);
                        continue;
                    }
                }
            }

            _ = streamReaderWrap.ReadCharacter();
        }

        foundEndDelimiter:

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.StringLiteral);

        if (useInterpolation)
        {
            lexerOutput.SyntaxTokenList.Insert(
                syntaxTokenListIndex,
                new SyntaxToken(SyntaxKind.StringInterpolatedStartToken, textSpan));
                
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedEndToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None)));
        }
        else
        {
            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringLiteralToken, textSpan));
        }
    }
    
    /// <summary>
    /// 'startInclusiveFirstOpenDelimiter' refers to:
    ///     $"Hello, {name}"
    ///
    /// how there is a '{' that deliminates the start of the interpolated expression.
    /// at what position index does it lie at?
    ///
    /// In the case of raw strings, the start-inclusive of the multi-character open delimiter is what needs to be provided.
    /// </summary>
    private static void LexInterpolatedExpression(
        CSharpBinder binder,
        ref CSharpLexerOutput lexerOutput,
        StreamReaderWrap streamReaderWrap,
        ref TextEditorTextSpan previousEscapeCharacterTextSpan,
        int startInclusiveOpenDelimiter,
        int countDollarSign,
        bool useRaw)
    {
        int unmatchedBraceCounter;
        
        if (useRaw)
        {
            // Starts inside the expression
        
            lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                streamReaderWrap.PositionIndex - countDollarSign,
                streamReaderWrap.PositionIndex,
                (byte)GenericDecorationKind.None));
        
            /*var readOpenDelimiterCount = streamReaderWrap.PositionIndex - startInclusiveOpenDelimiter;
        
            for (; readOpenDelimiterCount < countDollarSign; readOpenDelimiterCount++)
            {
                _ = streamReaderWrap.ReadCharacter();
            }*/
            
            unmatchedBraceCounter = countDollarSign;
        }
        else
        {
            // Starts at the OpenBraceToken
        
            lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                streamReaderWrap.PositionIndex,
                streamReaderWrap.PositionIndex + 1,
                (byte)GenericDecorationKind.None));
                
            var readOpenDelimiterCount = streamReaderWrap.PositionIndex - startInclusiveOpenDelimiter;
        
            for (; readOpenDelimiterCount < countDollarSign; readOpenDelimiterCount++)
            {
                _ = streamReaderWrap.ReadCharacter();
            }
            
            unmatchedBraceCounter = countDollarSign;
        }
    
        // Recursive solution that lexes the interpolated expression only, (not including the '{' or '}').
        Lex_Frame(
            binder,
            ref lexerOutput,
            streamReaderWrap,
            ref previousEscapeCharacterTextSpan,
            ref unmatchedBraceCounter);
        
        if (useRaw)
        {
            _ = streamReaderWrap.ReadCharacter(); // This consumes the final '}'.

            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedContinueToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex - 1,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None)));
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter(); // This consumes the final '}'.

            lexerOutput.SyntaxTokenList.Add(new SyntaxToken(
                SyntaxKind.StringInterpolatedContinueToken,
                new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex - 1,
                    streamReaderWrap.PositionIndex,
                    (byte)GenericDecorationKind.None)));
        }
    }
    
    private static void EscapeCharacterListAdd(
        ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap, ref TextEditorTextSpan previousEscapeCharacterTextSpan, TextEditorTextSpan textSpan)
    {
        if (lexerOutput.MiscTextSpanList.Count > 0)
        {
            if (previousEscapeCharacterTextSpan.EndExclusiveIndex == textSpan.StartInclusiveIndex &&
                previousEscapeCharacterTextSpan.DecorationByte == (byte)GenericDecorationKind.EscapeCharacterPrimary)
            {
                textSpan = textSpan with
                {
                    DecorationByte = (byte)GenericDecorationKind.EscapeCharacterSecondary,
                };
            }
        }
        
        previousEscapeCharacterTextSpan = textSpan;
        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexIdentifierOrKeywordOrKeywordContextual(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        while (!streamReaderWrap.IsEof)
        {
            if (!char.IsLetterOrDigit(streamReaderWrap.CurrentCharacter) &&
                streamReaderWrap.CurrentCharacter != '_')
            {
                break;
            }

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None);
        
        switch (binder.TextEditorService.EditContext_GetText(lexerOutput.Text.Slice(textSpan.StartInclusiveIndex, textSpan.Length)))
        {
            // NonContextualKeywords-NonControl
            // ================================
            case "abstract":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AbstractTokenKeyword, textSpan));
                return;
            case "as":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsTokenKeyword, textSpan));
                return;
            case "base":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BaseTokenKeyword, textSpan));
                return;
            case "bool":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BoolTokenKeyword, textSpan));
                return;
            case "byte":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByteTokenKeyword, textSpan));
                return;
            case "catch":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CatchTokenKeyword, textSpan));
                return;
            case "char":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharTokenKeyword, textSpan));
                return;
            case "checked":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CheckedTokenKeyword, textSpan));
                return;
            case "class":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ClassTokenKeyword, textSpan));
                return;
            case "const":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ConstTokenKeyword, textSpan));
                return;
            case "decimal":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DecimalTokenKeyword, textSpan));
                return;
            case "default":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DefaultTokenKeyword, textSpan));
                return;
            case "delegate":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DelegateTokenKeyword, textSpan));
                return;
            case "double":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoubleTokenKeyword, textSpan));
                return;
            case "enum":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EnumTokenKeyword, textSpan));
                return;
            case "event":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EventTokenKeyword, textSpan));
                return;
            case "explicit":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExplicitTokenKeyword, textSpan));
                return;
            case "extern":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ExternTokenKeyword, textSpan));
                return;
            case "false":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FalseTokenKeyword, textSpan));
                return;
            case "finally":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FinallyTokenKeyword, textSpan));
                return;
            case "fixed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FixedTokenKeyword, textSpan));
                return;
            case "float":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FloatTokenKeyword, textSpan));
                return;
            case "implicit":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ImplicitTokenKeyword, textSpan));
                return;
            case "in":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InTokenKeyword, textSpan));
                return;
            case "int":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntTokenKeyword, textSpan));
                return;
            case "interface":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InterfaceTokenKeyword, textSpan));
                return;
            case "internal":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InternalTokenKeyword, textSpan));
                return;
            case "is":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IsTokenKeyword, textSpan));
                return;
            case "lock":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LockTokenKeyword, textSpan));
                return;
            case "long":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LongTokenKeyword, textSpan));
                return;
            case "namespace":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NamespaceTokenKeyword, textSpan));
                return;
            case "new":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NewTokenKeyword, textSpan));
                return;
            case "null":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NullTokenKeyword, textSpan));
                return;
            case "object":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ObjectTokenKeyword, textSpan));
                return;
            case "operator":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OperatorTokenKeyword, textSpan));
                return;
            case "out":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OutTokenKeyword, textSpan));
                return;
            case "override":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OverrideTokenKeyword, textSpan));
                return;
            case "params":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ParamsTokenKeyword, textSpan));
                return;
            case "private":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PrivateTokenKeyword, textSpan));
                return;
            case "protected":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ProtectedTokenKeyword, textSpan));
                return;
            case "public":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PublicTokenKeyword, textSpan));
                return;
            case "readonly":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReadonlyTokenKeyword, textSpan));
                return;
            case "ref":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RefTokenKeyword, textSpan));
                return;
            case "sbyte":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SbyteTokenKeyword, textSpan));
                return;
            case "sealed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SealedTokenKeyword, textSpan));
                return;
            case "short":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ShortTokenKeyword, textSpan));
                return;
            case "sizeof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SizeofTokenKeyword, textSpan));
                return;
            case "stackalloc":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StackallocTokenKeyword, textSpan));
                return;
            case "static":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StaticTokenKeyword, textSpan));
                return;
            case "string":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StringTokenKeyword, textSpan));
                return;
            case "struct":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.StructTokenKeyword, textSpan));
                return;
            case "this":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThisTokenKeyword, textSpan));
                return;
            case "true":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TrueTokenKeyword, textSpan));
                return;
            case "try":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TryTokenKeyword, textSpan));
                return;
            case "typeof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.TypeofTokenKeyword, textSpan));
                return;
            case "uint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UintTokenKeyword, textSpan));
                return;
            case "ulong":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UlongTokenKeyword, textSpan));
                return;
            case "unchecked":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UncheckedTokenKeyword, textSpan));
                return;
            case "unsafe":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnsafeTokenKeyword, textSpan));
                return;
            case "ushort":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UshortTokenKeyword, textSpan));
                return;
            case "using":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UsingTokenKeyword, textSpan));
                return;
            case "virtual":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VirtualTokenKeyword, textSpan));
                return;
            case "void":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VoidTokenKeyword, textSpan));
                return;
            case "volatile":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VolatileTokenKeyword, textSpan));
                return;
            // NonContextualKeywords-IsControl
            // ===============================
            case "break":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.BreakTokenKeyword, textSpan));
                return;
            case "case":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CaseTokenKeyword, textSpan));
                return;
            case "continue":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ContinueTokenKeyword, textSpan));
                return;
            case "do":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DoTokenKeyword, textSpan));
                return;
            case "else":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ElseTokenKeyword, textSpan));
                return;
            case "for":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForTokenKeyword, textSpan));
                return;
            case "foreach":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ForeachTokenKeyword, textSpan));
                return;
            case "goto":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GotoTokenKeyword, textSpan));
                return;
            case "if":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IfTokenKeyword, textSpan));
                return;
            case "return":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ReturnTokenKeyword, textSpan));
                return;
            case "switch":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SwitchTokenKeyword, textSpan));
                return;
            case "throw":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ThrowTokenKeyword, textSpan));
                return;
            case "while":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhileTokenKeyword, textSpan));
                return;
            // ContextualKeywords-NotControl
            // =============================
            case "add":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AddTokenContextualKeyword, textSpan));
                return;
            case "and":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AndTokenContextualKeyword, textSpan));
                return;
            case "alias":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AliasTokenContextualKeyword, textSpan));
                return;
            case "ascending":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AscendingTokenContextualKeyword, textSpan));
                return;
            case "args":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ArgsTokenContextualKeyword, textSpan));
                return;
            case "async":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AsyncTokenContextualKeyword, textSpan));
                return;
            case "await":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.AwaitTokenContextualKeyword, textSpan));
                return;
            case "by":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ByTokenContextualKeyword, textSpan));
                return;
            case "descending":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DescendingTokenContextualKeyword, textSpan));
                return;
            case "dynamic":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.DynamicTokenContextualKeyword, textSpan));
                return;
            case "equals":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.EqualsTokenContextualKeyword, textSpan));
                return;
            case "file":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FileTokenContextualKeyword, textSpan));
                return;
            case "from":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.FromTokenContextualKeyword, textSpan));
                return;
            case "get":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GetTokenContextualKeyword, textSpan));
                return;
            case "global":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GlobalTokenContextualKeyword, textSpan));
                return;
            case "group":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.GroupTokenContextualKeyword, textSpan));
                return;
            case "init":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.InitTokenContextualKeyword, textSpan));
                return;
            case "into":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IntoTokenContextualKeyword, textSpan));
                return;
            case "join":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.JoinTokenContextualKeyword, textSpan));
                return;
            case "let":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.LetTokenContextualKeyword, textSpan));
                return;
            case "managed":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ManagedTokenContextualKeyword, textSpan));
                return;
            case "nameof":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NameofTokenContextualKeyword, textSpan));
                return;
            case "nint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NintTokenContextualKeyword, textSpan));
                return;
            case "not":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotTokenContextualKeyword, textSpan));
                return;
            case "notnull":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NotnullTokenContextualKeyword, textSpan));
                return;
            case "nuint":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NuintTokenContextualKeyword, textSpan));
                return;
            case "on":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OnTokenContextualKeyword, textSpan));
                return;
            case "or":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrTokenContextualKeyword, textSpan));
                return;
            case "orderby":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.OrderbyTokenContextualKeyword, textSpan));
                return;
            case "partial":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PartialTokenContextualKeyword, textSpan));
                return;
            case "record":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RecordTokenContextualKeyword, textSpan));
                return;
            case "remove":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RemoveTokenContextualKeyword, textSpan));
                return;
            case "required":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.RequiredTokenContextualKeyword, textSpan));
                return;
            case "scoped":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ScopedTokenContextualKeyword, textSpan));
                return;
            case "select":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SelectTokenContextualKeyword, textSpan));
                return;
            case "set":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.SetTokenContextualKeyword, textSpan));
                return;
            case "unmanaged":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.UnmanagedTokenContextualKeyword, textSpan));
                return;
            case "value":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.ValueTokenContextualKeyword, textSpan));
                return;
            case "var":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.VarTokenContextualKeyword, textSpan));
                return;
            case "when":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhenTokenContextualKeyword, textSpan));
                return;
            case "where":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WhereTokenContextualKeyword, textSpan));
                return;
            case "with":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.Keyword };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.WithTokenContextualKeyword, textSpan));
                return;
            // ContextualKeywords-IsControl
            // ============================
            case "yield":
                textSpan = textSpan with { DecorationByte = (byte)GenericDecorationKind.KeywordControl };
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.YieldTokenContextualKeyword, textSpan));
                return;
            default:
                lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.IdentifierToken, textSpan));
                return;
        }
    }
    
    public static void LexNumericLiteralToken(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNotANumber = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
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
                    break;
                default:
                    isNotANumber = true;
                    break;
            }

            if (isNotANumber)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.None);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.NumericLiteralToken, textSpan));
    }
    
    public static void LexCharLiteralToken(CSharpBinder binder, ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var delimiter = '\'';
        var escapeCharacter = '\\';
        
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        // Move past the opening delimiter
        _ = streamReaderWrap.ReadCharacter();

        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == delimiter)
            {
                _ = streamReaderWrap.ReadCharacter();
                break;
            }
            else if (streamReaderWrap.CurrentCharacter == escapeCharacter)
            {
                lexerOutput.MiscTextSpanList.Add(new TextEditorTextSpan(
                    streamReaderWrap.PositionIndex,
                    streamReaderWrap.PositionIndex + 2,
                    (byte)GenericDecorationKind.EscapeCharacterPrimary));

                // Presuming the escaped text is 2 characters,
                // then read an extra character.
                _ = streamReaderWrap.ReadCharacter();
            }

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.StringLiteral);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.CharLiteralToken, textSpan));
    }
    
    public static void LexCommentSingleLineToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNewLineCharacter = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '\r':
                case '\n':
                    isNewLineCharacter = true;
                    break;
                default:
                    break;
            }

            if (isNewLineCharacter)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.CommentSingleLine);

        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexCommentMultiLineToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        // Move past the initial "/*"
        streamReaderWrap.SkipRange(2);

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var possibleClosingText = false;
        var sawClosingText = false;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '*':
                    possibleClosingText = true;
                    break;
                case '/':
                    if (possibleClosingText)
                        sawClosingText = true;
                    break;
                default:
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();

            if (sawClosingText)
                break;
        }

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            streamReaderWrap.PositionIndex,
            (byte)GenericDecorationKind.CommentMultiLine);

        lexerOutput.MiscTextSpanList.Add(textSpan);
    }
    
    public static void LexPreprocessorDirectiveToken(ref CSharpLexerOutput lexerOutput, StreamReaderWrap streamReaderWrap)
    {
        var entryPositionIndex = streamReaderWrap.PositionIndex;

        // Declare outside the while loop to avoid overhead of redeclaring each loop? not sure
        var isNewLineCharacter = false;
        var firstWhitespaceCharacterPositionIndex = -1;

        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                case '\r':
                case '\n':
                    isNewLineCharacter = true;
                    break;
                case '\t':
                case ' ':
                    if (firstWhitespaceCharacterPositionIndex == -1)
                        firstWhitespaceCharacterPositionIndex = streamReaderWrap.PositionIndex;
                    break;
                default:
                    break;
            }

            if (isNewLineCharacter)
                break;

            _ = streamReaderWrap.ReadCharacter();
        }
        
        if (firstWhitespaceCharacterPositionIndex == -1)
            firstWhitespaceCharacterPositionIndex = streamReaderWrap.PositionIndex;

        var textSpan = new TextEditorTextSpan(
            entryPositionIndex,
            firstWhitespaceCharacterPositionIndex,
            (byte)GenericDecorationKind.PreprocessorDirective);

        lexerOutput.SyntaxTokenList.Add(new SyntaxToken(SyntaxKind.PreprocessorDirectiveToken, textSpan));
    }
}
