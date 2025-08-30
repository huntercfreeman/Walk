using Walk.Extensions.CompilerServices.Syntax;
using Walk.TextEditor.RazorLib.Exceptions;

namespace Walk.Extensions.CompilerServices.Utility;

public class TokenWalker
{
    private int _index;

    /// <summary>
    /// Use '-1' for each int value to indicate 'null' for the entirety of the _deferredParsingTuple;
    /// </summary>
    private (int openTokenIndex, int closeTokenIndex, int tokenIndexToRestore) _deferredParsingTuple = (-1, -1, -1);

    /// <summary>
    /// '-1' should not appear for any of the int values in the stack.
    /// _deferredParsingTuple is the cached Peek() result.
    ///
    /// If this stack is empty, them the cached Peek() result should be '(-1, -1, -1)'.
    /// </summary>
    private Stack<(int openTokenIndex, int closeTokenIndex, int tokenIndexToRestore)>? _deferredParsingTupleStack;

    public TokenWalker(IReadOnlyList<SyntaxToken> tokenList, bool useDeferredParsing = false)
    {
#if DEBUG
        if (tokenList.Count > 0 &&
            tokenList[tokenList.Count - 1].SyntaxKind != SyntaxKind.EndOfFileToken)
        {
            throw new WalkTextEditorException($"The last token must be 'SyntaxKind.EndOfFileToken'.");
        }
#endif

        TokenList = tokenList;
        
        if (useDeferredParsing)
            _deferredParsingTupleStack = new();
    }

    public int ConsumeCounter { get; private set; }

    public IReadOnlyList<SyntaxToken> TokenList { get; private set; }
    public SyntaxToken Current => Peek(0);
    public SyntaxToken Next => Peek(1);
    public SyntaxToken Previous => Peek(-1);
    public bool IsEof => Current.SyntaxKind == SyntaxKind.EndOfFileToken;
    public int Index => _index;

    /// <summary>If there are any tokens, then assume the final token is the end of file token. Otherwise, fabricate an end of file token.</summary>
    private SyntaxToken EOF => TokenList.Count > 0
        ? TokenList[TokenList.Count - 1]
        : new SyntaxToken(SyntaxKind.EndOfFileToken, new(0, 0, 0));

    /// <summary>The input to this method can be positive OR negative.<br/><br/>Returns <see cref="BadToken"/> when an index out of bounds error would've occurred.</summary>
    public SyntaxToken Peek(int offset)
    {
        var index = _index + offset;

        if (index < 0)
            return GetBadToken();
        else if (index >= TokenList.Count)
            return EOF; // Return the end of file token (the last token)

        return TokenList[index];
    }

    public SyntaxToken Consume()
    {
        if (_index >= TokenList.Count)
            return EOF; // Return the end of file token (the last token)

        if (_deferredParsingTuple.closeTokenIndex != -1)
        {
            if (_index == _deferredParsingTuple.closeTokenIndex)
            {
                _deferredParsingTupleStack.Pop();

                var closeChildScopeToken = TokenList[_index];
                _index = _deferredParsingTuple.tokenIndexToRestore;
                ConsumeCounter++;

                if (_deferredParsingTupleStack.Count > 0)
                    _deferredParsingTuple = _deferredParsingTupleStack.Peek();
                else
                    _deferredParsingTuple = (-1, -1, -1);

                return closeChildScopeToken;
            }
        }
        
        ConsumeCounter++;
        return TokenList[_index++];
    }

    public SyntaxToken Backtrack()
    {
        if (_index > 0)
        {
            _index--;
            ConsumeCounter--;
        }

        return Peek(_index);
    }
    
    public void BacktrackNoReturnValue()
    {
        if (_index > 0)
        {
            _index--;
            ConsumeCounter--;
        }
    }

    /// <summary>If the syntaxKind passed in does not match the current token, then a syntax token with that syntax kind will be fabricated and then returned instead.</summary>
    public SyntaxToken Match(SyntaxKind expectedSyntaxKind)
    {
        if (Current.SyntaxKind == expectedSyntaxKind)
            return Consume();
        else
            return this.FabricateToken(expectedSyntaxKind);
    }

    /// <summary>
    /// TODO: This method is being added to support breadth first parsing...
    /// ...Having this be a public method is a bit hacky,
    /// preferably deferring the parsing of a child scope would
    /// be done entirely from this class, so the _index cannot be changed
    /// externally (2024-06-18).
    /// </summary>
    public void DeferredParsing(
        int openTokenIndex,
        int closeTokenIndex,
        int tokenIndexToRestore)
    {
        _index = openTokenIndex;
        _deferredParsingTuple = (openTokenIndex, closeTokenIndex, tokenIndexToRestore);
        _deferredParsingTupleStack.Push((openTokenIndex, closeTokenIndex, tokenIndexToRestore));
        ConsumeCounter++;
    }

    public void SetNullDeferredParsingTuple()
    {
        _deferredParsingTuple = (-1, -1, -1);
    }

    public void ConsumeCounterReset()
    {
        ConsumeCounter = 0;
    }
    
    public void Reinitialize(List<SyntaxToken> tokenList)
    {
        // TODO: Don't duplicate the constructor here...
#if DEBUG
        if (tokenList.Count > 0 &&
            tokenList[tokenList.Count - 1].SyntaxKind != SyntaxKind.EndOfFileToken)
        {
            throw new WalkTextEditorException($"The last token must be 'SyntaxKind.EndOfFileToken'.");
        }
#endif
        TokenList = tokenList;
        
        _index = 0;
        ConsumeCounter = 0;
        _deferredParsingTuple = (-1, -1, -1);
        _deferredParsingTupleStack.Clear();
    }

    private SyntaxToken GetBadToken() => new SyntaxToken(SyntaxKind.BadToken, new(0, 0, 0));
}
