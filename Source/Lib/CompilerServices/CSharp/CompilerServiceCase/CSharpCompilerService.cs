using System.Reflection;
using System.Text;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.Menus.Models;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.LexerCase;
using Walk.CompilerServices.CSharp.ParserCase;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Displays;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Autocompletes.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Events.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

public sealed class CSharpCompilerService : IExtendedCompilerService
{
    // <summary>Public because the RazorCompilerService uses it.</summary>
    public readonly CSharpBinder __CSharpBinder;
    private readonly StreamReaderWrap _streamReaderWrap = new();
    
    // Service dependencies
    private readonly TextEditorService _textEditorService;
    
    private const string EmptyFileHackForLanguagePrimitiveText = "NotApplicable empty" + " void int char string bool var";
    
    public CSharpCompilerService(TextEditorService textEditorService)
    {
        _textEditorService = textEditorService;
        
        __CSharpBinder = new(_textEditorService, this);
        
        var primitiveKeywordsTextFile = new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData);
        
        __CSharpBinder.UpsertCompilationUnit(new ResourceUri(string.Empty), primitiveKeywordsTextFile);

        _safeOnlyUTF8Encoding = new SafeOnlyUTF8Encoding();
    }
    
    public TextEditorService TextEditorService => _textEditorService;

    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    private readonly StringBuilder _unsafeGetTextStringBuilder = new();
    private readonly char[] _unsafeGetTextBuffer = new char[1];

    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    private readonly StringBuilder _safeGetTextStringBuilder = new();
    private readonly char[] _safeGetTextBufferOne = new char[1];
    private readonly char[] _safeGetTextBufferTwo = new char[1];

    /// <summary>
    /// The currently being parsed file should reflect the TextEditorModel NOT the file system.
    /// Furthermore, long term, all files should reflect their TextEditorModel IF it exists.
    /// 
    /// This is a bit of misnomer because the solution wide parse doesn't set this.
    /// It is specifically a TextEditor based event having led to a parse that sets this.
    /// </summary>
    public (string AbsolutePathString, string Content) _currentFileBeingParsedTuple;

    /// <summary>
    /// This needs to be ensured to be cleared after the solution wide parse.
    /// 
    /// In order to avoid a try-catch-finally per file being parsed,
    /// this is being made public so the DotNetBackgroundTaskApi can guarantee this is cleared
    /// by wrapping the solution wide parse as a whole in a try-catch-finally.
    /// 
    /// The StreamReaderTupleCache does NOT contain this StreamReader.
    /// </summary>
    public (string AbsolutePathString, StreamReader Sr) FastParseTuple;

    public Dictionary<string, StreamReader> StreamReaderTupleCache = new();
    /// <summary>
    /// When you have two text spans that exist in the same file,
    /// and this file is not currently being parsed.
    /// 
    /// You must open 1 additional StreamReader, that reads from the same file
    /// as the existing cached one.
    /// </summary>
    public Dictionary<string, StreamReader> StreamReaderTupleCacheBackup = new();

    public void Clear_MAIN_StreamReaderTupleCache()
    {
        foreach (var streamReader in StreamReaderTupleCache.Values)
        {
            streamReader.Dispose();
        }
        StreamReaderTupleCache.Clear();
    }
    
    public void Clear_BACKUP_StreamReaderTupleCache()
    {
        foreach (var streamReader in StreamReaderTupleCacheBackup.Values)
        {
            streamReader.Dispose();
        }
        StreamReaderTupleCacheBackup.Clear();
    }

    public IReadOnlyList<ICompilerServiceResource> CompilerServiceResources { get; }
    
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions { get; }
    
    public IReadOnlyList<GenericParameterEntry> GenericParameterEntryList => __CSharpBinder.GenericParameterEntryList;
    public IReadOnlyList<FunctionParameterEntry> FunctionParameterEntryList => __CSharpBinder.FunctionParameterEntryList;
    public IReadOnlyList<FunctionArgumentEntry> FunctionArgumentEntryList => __CSharpBinder.FunctionArgumentEntryList;

    public void RegisterResource(ResourceUri resourceUri, bool shouldTriggerResourceWasModified)
    {
        __CSharpBinder.UpsertCompilationUnit(resourceUri, new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData));
            
        if (shouldTriggerResourceWasModified)
            ResourceWasModified(resourceUri, Array.Empty<TextEditorTextSpan>());
    }
    
    public void DisposeResource(ResourceUri resourceUri)
    {
        __CSharpBinder.RemoveCompilationUnit(resourceUri);
    }

    public void ResourceWasModified(ResourceUri resourceUri, IReadOnlyList<TextEditorTextSpan> editTextSpansList)
    {
        _textEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(resourceUri);

            if (modelModifier is null)
                return ValueTask.CompletedTask;

            return ParseAsync(editContext, modelModifier, shouldApplySyntaxHighlighting: true);
        });
    }

    public ICompilerServiceResource? GetResource(ResourceUri resourceUri)
    {
        __CSharpBinder.__CompilationUnitMap.TryGetValue(resourceUri, out var compilerServiceResource);
        return compilerServiceResource;
    }
    
    public MenuRecord GetContextMenu(TextEditorVirtualizationResult virtualizationResult, ContextMenu contextMenu)
    {
        return contextMenu.GetDefaultMenuRecord();
    }

    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    public string? UnsafeGetText(string absolutePathString, TextEditorTextSpan textSpan)
    {
        if (absolutePathString == string.Empty)
        {
            if (textSpan.EndExclusiveIndex > EmptyFileHackForLanguagePrimitiveText.Length)
                return null;
            return textSpan.GetText(EmptyFileHackForLanguagePrimitiveText, _textEditorService);
        }

        var model = _textEditorService.Model_GetOrDefault(new ResourceUri(absolutePathString));

        if (model is not null)
        {
            if (textSpan.EndExclusiveIndex > model.AllText.Length)
                return null;
            return textSpan.GetText(model.AllText, _textEditorService);
        }
        else if (File.Exists(absolutePathString))
        {
            using (StreamReader sr = new StreamReader(absolutePathString))
            {
                // I presume this is needed so the StreamReader can get the encoding.
                sr.Read();

                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                // sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();

                _unsafeGetTextStringBuilder.Clear();

                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(_unsafeGetTextBuffer, 0, 1);
                    _unsafeGetTextStringBuilder.Append(_unsafeGetTextBuffer[0]);
                }

                return _unsafeGetTextStringBuilder.ToString();
            }
        }
        
        return null;
    }

    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    public string? SafeGetText(string absolutePathString, TextEditorTextSpan textSpan)
    {
        StreamReader sr;

        if (absolutePathString == string.Empty)
        {
            if (textSpan.EndExclusiveIndex > EmptyFileHackForLanguagePrimitiveText.Length)
                return null;
            return textSpan.GetText(EmptyFileHackForLanguagePrimitiveText, _textEditorService);
        }
        else if (absolutePathString == _currentFileBeingParsedTuple.AbsolutePathString)
        {
            if (textSpan.EndExclusiveIndex > _currentFileBeingParsedTuple.Content.Length)
                return null;
            return textSpan.GetText(_currentFileBeingParsedTuple.Content, _textEditorService);
        }
        else if (absolutePathString == FastParseTuple.AbsolutePathString)
        {
            // TODO: What happens if I split a multibyte word?
            FastParseTuple.Sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
            // sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
            FastParseTuple.Sr.DiscardBufferedData();

            _safeGetTextStringBuilder.Clear();

            for (int i = 0; i < textSpan.Length; i++)
            {
                FastParseTuple.Sr.Read(_safeGetTextBufferOne, 0, 1);
                _safeGetTextStringBuilder.Append(_safeGetTextBufferOne[0]);
            }

            return _safeGetTextStringBuilder.ToString();
        }

        if (!StreamReaderTupleCache.TryGetValue(absolutePathString, out sr))
        {
            if (!File.Exists(absolutePathString))
                return null;
        
            sr = new StreamReader(absolutePathString, _safeOnlyUTF8Encoding);
            // Solution wide parse on Walk.sln
            //
            // 350 -> _countCacheClear: 15
            // 450 -> _countCacheClear: 9
            // 500 -> _countCacheClear: 7
            // 800 -> _countCacheClear: 2
            // 1000 -> _countCacheClear: 0
            //
            // 512 is c library limit?
            // 1024 is linux DEFAULT soft limit?
            // The reality is that you can go FAR higher when not limited?
            // But how do I know the limit of each user?
            // So I guess 500 is a safe bet for now?
            //
            // CSharpCompilerService at ~2k lines of text needed `StreamReaderTupleCache.Count: 214`.
            // ParseExpressions at ~4k lines of text needed `StreamReaderTupleCache.Count: 139`.
            //
            // This isn't just used for single file parsing though, it is also used for solution wide.
            if (StreamReaderTupleCache.Count >= 360)
            {
                Clear_MAIN_StreamReaderTupleCache();
            }

            StreamReaderTupleCache.Add(absolutePathString, sr);
            
            // I presume this is needed so the StreamReader can get the encoding.
            sr.Read();
        }

        // TODO: What happens if I split a multibyte word?
        sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
        sr.DiscardBufferedData();

        _safeGetTextStringBuilder.Clear();

        for (int i = 0; i < textSpan.Length; i++)
        {
            sr.Read(_safeGetTextBufferOne, 0, 1);
            _safeGetTextStringBuilder.Append(_safeGetTextBufferOne[0]);
        }

        return _safeGetTextStringBuilder.ToString();
    }
    
    public bool SafeCompareText(string absolutePathString, string value, TextEditorTextSpan textSpan)
    {
        if (value.Length != textSpan.Length)
            return false;
    
        StreamReader sr;

        if (absolutePathString == string.Empty)
        {
            if (textSpan.EndExclusiveIndex > EmptyFileHackForLanguagePrimitiveText.Length)
                return false;
            return value == textSpan.GetText(EmptyFileHackForLanguagePrimitiveText, _textEditorService);
            // The object allocation counts are nearly identical when I swap to using this code that compares
            // each character.
            //
            // Even odder, the counts actually end up on the slightly larger side (although incredibly minorly so).
            //
            // It seems there are a lot of SafeGetText(...) invocations that I'm still making, and that the majority
            // of string allocations are coming from those, not these sub cases?
            //
            /*for (int i = 0; i < textSpan.Length; i++)
            {
                
                if (value[i] != EmptyFileHackForLanguagePrimitiveText[textSpan.StartInclusiveIndex + i])
                    return false;
            }
            return true;*/
        }
        else if (absolutePathString == _currentFileBeingParsedTuple.AbsolutePathString)
        {
            if (textSpan.EndExclusiveIndex > _currentFileBeingParsedTuple.Content.Length)
                return false;
            return value == textSpan.GetText(_currentFileBeingParsedTuple.Content, _textEditorService);
            // The object allocation counts are nearly identical when I swap to using this code that compares
            // each character.
            //
            // Even odder, the counts actually end up on the slightly larger side (although incredibly minorly so).
            //
            // It seems there are a lot of SafeGetText(...) invocations that I'm still making, and that the majority
            // of string allocations are coming from those, not these sub cases?
            //
            /*
            for (int i = 0; i < textSpan.Length; i++)
            {

                if (value[i] != _currentFileBeingParsedTuple.Content[textSpan.StartInclusiveIndex + i])
                    return false;
            }
            return true;*/
        }
        else if (absolutePathString == FastParseTuple.AbsolutePathString)
        {
            // TODO: What happens if I split a multibyte word?
            FastParseTuple.Sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
            FastParseTuple.Sr.DiscardBufferedData();

            for (int i = 0; i < textSpan.Length; i++)
            {
                FastParseTuple.Sr.Read(_safeGetTextBufferOne, 0, 1);
                if (value[i] != _safeGetTextBufferOne[0])
                    return false;
            }

            return true;
        }
        
        if (!StreamReaderTupleCache.TryGetValue(absolutePathString, out sr))
        {
            if (!File.Exists(absolutePathString))
                return false;
        
            sr = new StreamReader(absolutePathString, _safeOnlyUTF8Encoding);
            // Solution wide parse on Walk.sln
            //
            // 350 -> _countCacheClear: 15
            // 450 -> _countCacheClear: 9
            // 500 -> _countCacheClear: 7
            // 800 -> _countCacheClear: 2
            // 1000 -> _countCacheClear: 0
            //
            // 512 is c library limit?
            // 1024 is linux DEFAULT soft limit?
            // The reality is that you can go FAR higher when not limited?
            // But how do I know the limit of each user?
            // So I guess 500 is a safe bet for now?
            //
            // CSharpCompilerService at ~2k lines of text needed `StreamReaderTupleCache.Count: 214`.
            // ParseExpressions at ~4k lines of text needed `StreamReaderTupleCache.Count: 139`.
            //
            // This isn't just used for single file parsing though, it is also used for solution wide.
            if (StreamReaderTupleCache.Count >= 360)
            {
                Clear_MAIN_StreamReaderTupleCache();
            }

            StreamReaderTupleCache.Add(absolutePathString, sr);
            
            // I presume this is needed so the StreamReader can get the encoding.
            sr.Read();
        }

        // TODO: What happens if I split a multibyte word?
        sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
        // sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
        sr.DiscardBufferedData();

        for (int i = 0; i < textSpan.Length; i++)
        {
            sr.Read(_safeGetTextBufferOne, 0, 1);
            if (value[i] != _safeGetTextBufferOne[0])
                return false;
        }

        return true;
    }
    
    public bool SafeCompareTextSpans(string sourceAbsolutePathString, TextEditorTextSpan sourceTextSpan, string otherAbsolutePathString, TextEditorTextSpan otherTextSpan)
    {
        if (sourceTextSpan.Length != otherTextSpan.Length ||
            sourceTextSpan.CharIntSum != otherTextSpan.CharIntSum)
        {
            return false;
        }

        var length = otherTextSpan.Length;

        if (sourceAbsolutePathString == FastParseTuple.AbsolutePathString)
        {
            FastParseTuple.Sr.BaseStream.Seek(sourceTextSpan.ByteIndex, SeekOrigin.Begin);
            FastParseTuple.Sr.DiscardBufferedData();

            // string.Empty as file path is primitive keywords hack.
            if (otherAbsolutePathString == string.Empty)
            {
                if (otherTextSpan.StartInclusiveIndex + (length - 1) >= EmptyFileHackForLanguagePrimitiveText.Length)
                    return false;
            
                for (int i = 0; i < length; i++)
                {
                    FastParseTuple.Sr.Read(_safeGetTextBufferOne, 0, 1);

                    if (_safeGetTextBufferOne[0] !=
                        EmptyFileHackForLanguagePrimitiveText[otherTextSpan.StartInclusiveIndex + i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                // StreamReader cache does not contain the FastParseTuple.Sr
                var otherSr = GetOtherStreamReader(otherAbsolutePathString, otherTextSpan);
                if (otherSr is null)
                    return false;

                for (int i = 0; i < length; i++)
                {
                    FastParseTuple.Sr.Read(_safeGetTextBufferOne, 0, 1);
                    otherSr.Read(_safeGetTextBufferTwo, 0, 1);
                    if (_safeGetTextBufferOne[0] != _safeGetTextBufferTwo[0])
                        return false;
                }
            }
        }
        else if (sourceAbsolutePathString == _currentFileBeingParsedTuple.AbsolutePathString)
        {
            // string.Empty as file path is primitive keywords hack.
            if (otherAbsolutePathString == string.Empty)
            {
                if (sourceTextSpan.StartInclusiveIndex + (length - 1) >= _currentFileBeingParsedTuple.Content.Length)
                    return false;
                if (otherTextSpan.StartInclusiveIndex + (length - 1) >= EmptyFileHackForLanguagePrimitiveText.Length)
                    return false;
                    
                for (int i = 0; i < length; i++)
                {
                    if (_currentFileBeingParsedTuple.Content[sourceTextSpan.StartInclusiveIndex + i] !=
                        EmptyFileHackForLanguagePrimitiveText[otherTextSpan.StartInclusiveIndex + i])
                    {
                        return false;
                    }
                }
            }
            else if (otherAbsolutePathString == _currentFileBeingParsedTuple.AbsolutePathString)
            {
                if (sourceTextSpan.StartInclusiveIndex + (length - 1) >= _currentFileBeingParsedTuple.Content.Length)
                    return false;
                if (otherTextSpan.StartInclusiveIndex + (length - 1) >= _currentFileBeingParsedTuple.Content.Length)
                    return false;
            
                for (int i = 0; i < length; i++)
                {
                    if (_currentFileBeingParsedTuple.Content[sourceTextSpan.StartInclusiveIndex + i] !=
                        _currentFileBeingParsedTuple.Content[otherTextSpan.StartInclusiveIndex + i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (sourceTextSpan.StartInclusiveIndex + (length - 1) >= _currentFileBeingParsedTuple.Content.Length)
                    return false;
                    
                var otherSr = GetOtherStreamReader(otherAbsolutePathString, otherTextSpan);
                if (otherSr is null)
                    return false;
                
                for (int i = 0; i < length; i++)
                {
                    otherSr.Read(_safeGetTextBufferTwo, 0, 1);
                    if (_currentFileBeingParsedTuple.Content[sourceTextSpan.StartInclusiveIndex + i] != _safeGetTextBufferTwo[0])
                        return false;
                }
            }
        }
        else
        {
            var sourceSr = GetOtherStreamReader(sourceAbsolutePathString, sourceTextSpan);
            if (sourceSr is null)
                return false;

            var otherSr = GetBackupStreamReader(otherAbsolutePathString, otherTextSpan);
            if (otherSr is null)
                return false;

            for (int i = 0; i < length; i++)
            {
                sourceSr.Read(_safeGetTextBufferOne, 0, 1);
                otherSr.Read(_safeGetTextBufferTwo, 0, 1);
                if (_safeGetTextBufferOne[0] != _safeGetTextBufferTwo[0])
                    return false;
            }
        }

        return true;
    }

    private StreamReader? GetOtherStreamReader(string otherAbsolutePathString, TextEditorTextSpan otherTextSpan)
    {
        if (!TryGetCachedStreamReader(otherAbsolutePathString, out var otherSr))
            return null;
        otherSr.BaseStream.Seek(otherTextSpan.ByteIndex, SeekOrigin.Begin);
        otherSr.DiscardBufferedData();
        return otherSr;
    }

    private bool TryGetCachedStreamReader(string absolutePathString, out StreamReader sr)
    {
        if (!StreamReaderTupleCache.TryGetValue(absolutePathString, out sr))
        {
            if (!File.Exists(absolutePathString))
                return false;

            sr = new StreamReader(absolutePathString, _safeOnlyUTF8Encoding);
            // Solution wide parse on Walk.sln
            //
            // 350 -> _countCacheClear: 15
            // 450 -> _countCacheClear: 9
            // 500 -> _countCacheClear: 7
            // 800 -> _countCacheClear: 2
            // 1000 -> _countCacheClear: 0
            //
            // 512 is c library limit?
            // 1024 is linux DEFAULT soft limit?
            // The reality is that you can go FAR higher when not limited?
            // But how do I know the limit of each user?
            // So I guess 500 is a safe bet for now?
            //
            // CSharpCompilerService at ~2k lines of text needed `StreamReaderTupleCache.Count: 214`.
            // ParseExpressions at ~4k lines of text needed `StreamReaderTupleCache.Count: 139`.
            //
            // This isn't just used for single file parsing though, it is also used for solution wide.
            if (StreamReaderTupleCache.Count >= 360)
            {
                Clear_MAIN_StreamReaderTupleCache();
            }

            StreamReaderTupleCache.Add(absolutePathString, sr);

            // I presume this is needed so the StreamReader can get the encoding.
            sr.Read();
        }

        return true;
    }
    
    private bool BACKUP_TryGetCachedStreamReader(string absolutePathString, out StreamReader sr)
    {
        if (!StreamReaderTupleCacheBackup.TryGetValue(absolutePathString, out sr))
        {
            if (!File.Exists(absolutePathString))
                return false;

            sr = new StreamReader(absolutePathString, _safeOnlyUTF8Encoding);
            // Solution wide parse on Walk.sln
            //
            // 350 -> _countCacheClear: 15
            // 450 -> _countCacheClear: 9
            // 500 -> _countCacheClear: 7
            // 800 -> _countCacheClear: 2
            // 1000 -> _countCacheClear: 0
            //
            // 512 is c library limit?
            // 1024 is linux DEFAULT soft limit?
            // The reality is that you can go FAR higher when not limited?
            // But how do I know the limit of each user?
            // So I guess 500 is a safe bet for now?
            //
            // CSharpCompilerService at ~2k lines of text needed `StreamReaderTupleCacheBackup.Count: 214`.
            // ParseExpressions at ~4k lines of text needed `StreamReaderTupleCacheBackup.Count: 139`.
            //
            // This isn't just used for single file parsing though, it is also used for solution wide.
            if (StreamReaderTupleCacheBackup.Count >= 140)
            {
                Clear_BACKUP_StreamReaderTupleCache();
            }

            StreamReaderTupleCacheBackup.Add(absolutePathString, sr);

            // I presume this is needed so the StreamReader can get the encoding.
            sr.Read();
        }

        return true;
    }

    private StreamReader? GetBackupStreamReader(string otherAbsolutePathString, TextEditorTextSpan otherTextSpan)
    {
        if (!BACKUP_TryGetCachedStreamReader(otherAbsolutePathString, out var otherSr))
            return null;
        otherSr.BaseStream.Seek(otherTextSpan.ByteIndex, SeekOrigin.Begin);
        otherSr.DiscardBufferedData();
        return otherSr;
    }

    private MenuRecord? GetAutocompleteMenuPart(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu, int positionIndex)
    {
        var character = '\0';
        
        var foundMemberAccessToken = false;
        var memberAccessTokenPositionIndex = -1;
        
        var isParsingIdentifier = false;
        var isParsingNumber = false;
        
        // banana.Price
        //
        // 'banana.' is  the context
        // 'banana' is the operating word
        var operatingWordEndExclusiveIndex = -1;
        
        var filteringWordEndExclusiveIndex = -1;
        var filteringWordStartInclusiveIndex = -1;
        
        // '|' indicates cursor position:
        //
        // "apple banana.Pri|ce"
        // "apple.banana Pri|ce"
        var notParsingButTouchingletterOrDigit = false;
        var letterOrDigitIntoNonMatchingCharacterKindOccurred = false;
        
        var i = positionIndex - 1;
        
        for (; i >= 0; i--)
        {
            character = virtualizationResult.Model.GetCharacter(i);
            
            switch (character)
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
                    if (foundMemberAccessToken)
                    {
                        isParsingIdentifier = true;
                        
                        if (operatingWordEndExclusiveIndex == -1)
                            operatingWordEndExclusiveIndex = i;
                    }
                    else
                    {
                        if (!notParsingButTouchingletterOrDigit)
                        {
                            notParsingButTouchingletterOrDigit = true;
                            
                            if (filteringWordEndExclusiveIndex == -1)
                                filteringWordEndExclusiveIndex = i + 1;
                        }
                    }
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
                    if (foundMemberAccessToken)
                    {
                        if (!isParsingIdentifier)
                        {
                            isParsingNumber = true;
                            
                            if (operatingWordEndExclusiveIndex == -1)
                                operatingWordEndExclusiveIndex = i;
                        }
                    }
                    else
                    {
                        if (!notParsingButTouchingletterOrDigit)
                        {
                            notParsingButTouchingletterOrDigit = true;
                            
                            if (filteringWordEndExclusiveIndex == -1)
                                filteringWordEndExclusiveIndex = i + 1;
                        }
                    }
                    break;
                case '\r':
                case '\n':
                case '\t':
                case ' ':
                    if (isParsingIdentifier || isParsingNumber)
                        goto exitOuterForLoop;
        
                    if (notParsingButTouchingletterOrDigit)
                    {
                        if (letterOrDigitIntoNonMatchingCharacterKindOccurred)
                        {
                            goto exitOuterForLoop;
                        }
                        else
                        {
                            letterOrDigitIntoNonMatchingCharacterKindOccurred = true;
                        }
                    }
                    break;
                case '.':
                    if (!foundMemberAccessToken)
                    {
                        if (notParsingButTouchingletterOrDigit && filteringWordStartInclusiveIndex == -1)
                            filteringWordStartInclusiveIndex = i + 1;
                    
                        foundMemberAccessToken = true;
                        notParsingButTouchingletterOrDigit = false;
                        letterOrDigitIntoNonMatchingCharacterKindOccurred = false;
                        
                        if (i > 0)
                        {
                            var innerCharacter = virtualizationResult.Model.GetCharacter(i - 1);
                            
                            if (innerCharacter == '?' || innerCharacter == '!')
                                i--;
                        }
                    }
                    else
                    {
                        goto exitOuterForLoop;
                    }
                    break;
                default:
                    goto exitOuterForLoop;
            }
        }
        
        exitOuterForLoop:
        
        // Invalidate the parsed identifier if it starts with a number.
        if (isParsingIdentifier)
        {
            switch (character)
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
                    isParsingIdentifier = false;
                    break;
            }
        }
        
        var filteringWord = string.Empty;
        
        if (filteringWordStartInclusiveIndex != -1 && filteringWordEndExclusiveIndex != -1)
        {
            var textSpan = new TextEditorTextSpan(
                filteringWordStartInclusiveIndex,
                filteringWordEndExclusiveIndex,
                DecorationByte: 0);
                
            filteringWord = textSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService);
        }
            
        if (foundMemberAccessToken && operatingWordEndExclusiveIndex != -1)
        {
            var autocompleteEntryList = new List<AutocompleteEntry>();
            
            var operatingWordAmongPositionIndex = operatingWordEndExclusiveIndex - 1;
           
            if (operatingWordAmongPositionIndex < 0)
                operatingWordAmongPositionIndex = 0;
       
            var foundMatch = false;
            
            var resource = GetResource(virtualizationResult.Model.PersistentState.ResourceUri);
            var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
            
            var symbols = __CSharpBinder.SymbolList.Skip(compilationUnitLocal.IndexSymbolList).Take(compilationUnitLocal.CountSymbolList).ToList();
            // var diagnostics = compilationUnitLocal.DiagnosticList;
            
            Symbol foundSymbol = default;
    
            if (!foundMatch && symbols.Count != 0)
            {
                foreach (var symbol in symbols)
                {
                    if (operatingWordAmongPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                        operatingWordAmongPositionIndex < symbol.TextSpan.EndExclusiveIndex)
                    {
                        foundMatch = true;
                        foundSymbol = symbol;
                        break;
                    }
                }
            }
            
            if (foundMatch)
            {
                // var textEditorModel = _textEditorService.ModelApi.GetOrDefault(foundSymbol.TextSpan.ResourceUri);
                var textEditorModel = virtualizationResult.Model;
                var extendedCompilerService = (IExtendedCompilerService)textEditorModel.PersistentState.CompilerService;
                var compilerServiceResource = extendedCompilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
        
                var definitionNode = extendedCompilerService.GetDefinitionNode(foundSymbol.TextSpan, textEditorModel.PersistentState.ResourceUri, compilerServiceResource, foundSymbol);
                
                if (definitionNode is not null)
                {
                    if (definitionNode.SyntaxKind == SyntaxKind.NamespaceClauseNode)
                    {
                        var namespaceClauseNode = (NamespaceClauseNode)definitionNode;
                    
                        NamespacePrefixNode? namespacePrefixNode = null;
                        
                        if (namespaceClauseNode.NamespacePrefixNode is not null)
                        {
                            namespacePrefixNode = namespaceClauseNode.NamespacePrefixNode;
                        }
                        else
                        {
                            namespacePrefixNode = __CSharpBinder.FindPrefix(
                                __CSharpBinder.NamespacePrefixTree.__Root,
                                foundSymbol.TextSpan,
                                textEditorModel.PersistentState.ResourceUri.Value);
                        }

                        if (namespacePrefixNode is not null)
                        {
                            var filteringWordCharIntSum = 0;
                            foreach (var c in filteringWord)
                            {
                                filteringWordCharIntSum += (int)c;
                            }
                        
                            var findTuple = __CSharpBinder.NamespacePrefixTree.FindRange(
                                namespacePrefixNode,
                                filteringWordCharIntSum);
                        
                            for (int prefixIndex = findTuple.StartIndex; prefixIndex < findTuple.EndIndex; prefixIndex++)
                            {
                                var prefix = namespacePrefixNode.Children[prefixIndex];
                                var prefixText = SafeGetText(prefix.ResourceUri.Value, prefix.TextSpan);
                                if (prefixText.Contains(filteringWord))
                                {
                                    autocompleteEntryList.Add(new AutocompleteEntry(
                                        prefixText,
                                        AutocompleteEntryKind.Namespace,
                                        () => MemberAutocomplete(prefixText, filteringWord, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                                }
                            }
                            
                            findTuple = __CSharpBinder.NamespaceGroup_FindRange(new NamespaceContributionEntry(foundSymbol.TextSpan));
                            
                            NamespaceGroup namespaceGroup = default;
                            
                            for (int namespaceGroupIndex = findTuple.StartIndex; namespaceGroupIndex < findTuple.EndIndex; namespaceGroupIndex++)
                            {
                                var possibleNamespaceGroup = __CSharpBinder.NamespaceGroupMap[namespaceGroupIndex];
                                
                                if (possibleNamespaceGroup.NamespaceStatementNodeList.Count > 0)
                                {
                                    var sampleNamespaceStatementNode = possibleNamespaceGroup.NamespaceStatementNodeList[0];
                                    
                                    if (SafeCompareTextSpans(
                                            virtualizationResult.Model.PersistentState.ResourceUri.Value,
                                            foundSymbol.TextSpan,
                                            sampleNamespaceStatementNode.ResourceUri.Value,
                                            sampleNamespaceStatementNode.IdentifierToken.TextSpan))
                                    {
                                        namespaceGroup = possibleNamespaceGroup;
                                        break;
                                    }
                                }
                            }
                            
                            if (namespaceGroup.ConstructorWasInvoked)
                            {
                                foreach (var typeDefinitionNode in __CSharpBinder.GetTopLevelTypeDefinitionNodes_NamespaceGroup(namespaceGroup).Where(x => x.TypeIdentifierToken.TextSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService).Contains(filteringWord)).Take(5))
                                {
                                    var resourceUriValue = virtualizationResult.Model.PersistentState.ResourceUri.Value;


                                    if (typeDefinitionNode.ResourceUri != virtualizationResult.Model.PersistentState.ResourceUri)
                                    {
                                        if (__CSharpBinder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out var innerCompilationUnit))
                                        {
                                            resourceUriValue = typeDefinitionNode.ResourceUri.Value;
                                        }
                                    }
                                
                                    autocompleteEntryList.Add(new AutocompleteEntry(
                                        UnsafeGetText(resourceUriValue, typeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty,
                                        AutocompleteEntryKind.Type,
                                        () => MemberAutocomplete(UnsafeGetText(resourceUriValue, typeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty, filteringWord, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                                }
                            }
                            
                            return new MenuRecord(
                                autocompleteEntryList.Select(entry =>
                                {
                                    var menuOptionRecord = new MenuOptionRecord(
                                        entry.DisplayName,
                                        MenuOptionKind.Other,
                                        _ => entry.SideEffectFunc?.Invoke() ?? Task.CompletedTask);
                                    menuOptionRecord.IconKind = entry.AutocompleteEntryKind;
                                    return menuOptionRecord;
                                })
                                .ToList());
                        }
                        
                        return null;
                    }
                
                    TypeReference typeReference = default;
                    
                    if (definitionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    {
                        var variableReferenceNode = (VariableReferenceNode)definitionNode;
                        if (variableReferenceNode.VariableDeclarationNode is not null)
                            typeReference = variableReferenceNode.VariableDeclarationNode.TypeReference;
                    }
                    else if (definitionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                    {
                        var variableDeclarationNode = (VariableDeclarationNode)definitionNode;
                        typeReference = variableDeclarationNode.TypeReference;
                    }
                    else if (definitionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                    {
                        typeReference = ((FunctionInvocationNode)definitionNode).ResultTypeReference;
                    }
                    else if (definitionNode.SyntaxKind == SyntaxKind.TypeClauseNode)
                    {
                        typeReference = new TypeReference((TypeClauseNode)definitionNode);
                    }
                    else if (definitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                    {
                        var typeDefinitionNode = (TypeDefinitionNode)definitionNode;
                        typeReference = typeDefinitionNode.ToTypeReference();
                    }
                    
                    if (typeReference != default)
                    {
                        Symbol innerFoundSymbol = default;
                        var innerCompilationUnit = compilationUnitLocal;
                        var innerResourceUri = virtualizationResult.Model.PersistentState.ResourceUri;

                        if (typeReference.ExplicitDefinitionResourceUri.Value is not null && typeReference.ExplicitDefinitionResourceUri != textEditorModel.PersistentState.ResourceUri)
                        {
                            if (__CSharpBinder.__CompilationUnitMap.TryGetValue(typeReference.ExplicitDefinitionResourceUri, out innerCompilationUnit))
                            {
                                innerResourceUri = typeReference.ExplicitDefinitionResourceUri;
                                symbols = __CSharpBinder.SymbolList.Skip(innerCompilationUnit.IndexSymbolList).Take(innerCompilationUnit.CountSymbolList).ToList();
                            }
                        }
                        
                        if (symbols.Count != 0)
                        {
                            foreach (var symbol in symbols)
                            {
                                if (typeReference.ExplicitDefinitionTextSpan.StartInclusiveIndex >= symbol.TextSpan.StartInclusiveIndex &&
                                    typeReference.ExplicitDefinitionTextSpan.StartInclusiveIndex < symbol.TextSpan.EndExclusiveIndex)
                                {
                                    innerFoundSymbol = symbol;
                                    break;
                                }
                            }
                        }
                        
                        if (innerFoundSymbol != default)
                        {
                            var maybeTypeDefinitionNode = __CSharpBinder.GetDefinitionNode(
                                innerResourceUri,
                                innerCompilationUnit,
                                innerFoundSymbol.TextSpan,
                                syntaxKind: innerFoundSymbol.SyntaxKind,
                                symbol: innerFoundSymbol);
                            
                            if (maybeTypeDefinitionNode is not null && maybeTypeDefinitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                            {
                                var typeDefinitionNode = (TypeDefinitionNode)maybeTypeDefinitionNode;
                                var memberList = __CSharpBinder.GetMemberList_TypeDefinitionNode(typeDefinitionNode);
                                ISyntaxNode? foundDefinitionNode = null;
                                
                                foreach (var member in memberList.Where(x => __CSharpBinder.GetIdentifierText(x, innerResourceUri, innerCompilationUnit)?.Contains(filteringWord) ?? false).Take(25))
                                {
                                    switch (member.SyntaxKind)
                                    {
                                        case SyntaxKind.VariableDeclarationNode:
                                        {
                                            string resourceUriValue;
                                            var variableDeclarationNode = (VariableDeclarationNode)member;
                                            
                                            if (variableDeclarationNode.ResourceUri != innerResourceUri)
                                            {
                                                if (__CSharpBinder.__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var variableDeclarationCompilationUnit))
                                                    resourceUriValue = variableDeclarationNode.ResourceUri.Value;
                                                else
                                                    resourceUriValue = innerResourceUri.Value;
                                            }
                                            else
                                            {
                                                resourceUriValue = innerResourceUri.Value;
                                            }
                                            
                                            autocompleteEntryList.Add(new AutocompleteEntry(
                                                UnsafeGetText(resourceUriValue, variableDeclarationNode.IdentifierToken.TextSpan) ?? string.Empty,
                                                AutocompleteEntryKind.Variable,
                                                () => MemberAutocomplete(UnsafeGetText(resourceUriValue, variableDeclarationNode.IdentifierToken.TextSpan) ?? string.Empty, filteringWord, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                                            break;
                                        }
                                        case SyntaxKind.FunctionDefinitionNode:
                                        {
                                            string sourceText;
                                            string resourceUriValue;
                                            var functionDefinitionNode = (FunctionDefinitionNode)member;
                                            
                                            if (functionDefinitionNode.ResourceUri != innerResourceUri)
                                            {
                                                if (__CSharpBinder.__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var functionDefinitionCompilationUnit))
                                                    resourceUriValue = functionDefinitionNode.ResourceUri.Value;
                                                else
                                                    resourceUriValue = innerResourceUri.Value;
                                            }
                                            else
                                            {
                                                resourceUriValue = innerResourceUri.Value;
                                            }
                                            
                                            autocompleteEntryList.Add(new AutocompleteEntry(
                                                UnsafeGetText(resourceUriValue, functionDefinitionNode.FunctionIdentifierToken.TextSpan) ?? string.Empty,
                                                AutocompleteEntryKind.Function,
                                                () => MemberAutocomplete(UnsafeGetText(resourceUriValue, functionDefinitionNode.FunctionIdentifierToken.TextSpan) ?? string.Empty, filteringWord, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                                            break;
                                        }
                                        case SyntaxKind.TypeDefinitionNode:
                                        {
                                            var innerTypeDefinitionNode = (TypeDefinitionNode)member;
                                            autocompleteEntryList.Add(new AutocompleteEntry(
                                                UnsafeGetText(innerResourceUri.Value, innerTypeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty,
                                                AutocompleteEntryKind.Type,
                                                () => MemberAutocomplete(UnsafeGetText(innerResourceUri.Value, innerTypeDefinitionNode.TypeIdentifierToken.TextSpan) ?? string.Empty, filteringWord, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        
            if (autocompleteEntryList.Count == 0)
                return new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            
            return new MenuRecord(
                autocompleteEntryList.Select(entry =>
                {
                    var menuOptionRecord = new MenuOptionRecord(
                        entry.DisplayName,
                        MenuOptionKind.Other,
                        _ => entry.SideEffectFunc?.Invoke() ?? Task.CompletedTask);
                    
                    menuOptionRecord.IconKind = entry.AutocompleteEntryKind;
                    return menuOptionRecord;
                })
                .ToList());
        }
        
        return null;
    }

    public MenuRecord GetAutocompleteMenu(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu)
    {
        var positionIndex = virtualizationResult.Model.GetPositionIndex(virtualizationResult.ViewModel);
        
        var autocompleteMenuPart = GetAutocompleteMenuPart(virtualizationResult, autocompleteMenu, positionIndex);
        if (autocompleteMenuPart is not null)
            return autocompleteMenuPart;
        
        var word = virtualizationResult.Model.ReadPreviousWordOrDefault(
            virtualizationResult.ViewModel.LineIndex,
            virtualizationResult.ViewModel.ColumnIndex);
    
        // The cursor is 1 character ahead.
        var textSpan = new TextEditorTextSpan(
            positionIndex - 1,
            positionIndex,
            0);
    
        var compilerServiceAutocompleteEntryList = OBSOLETE_GetAutocompleteEntries(
            word,
            textSpan,
            virtualizationResult);
    
        return autocompleteMenu.GetDefaultMenuRecord(compilerServiceAutocompleteEntryList);
    }
    
    private Task MemberAutocomplete(string text, string filteringWord, ResourceUri resourceUri, Key<TextEditorViewModel> viewModelKey)
    {
        _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            var model = editContext.GetModelModifier(resourceUri);
            var viewModel = editContext.GetViewModelModifier(viewModelKey);
            
            model.Insert(new string(text.Skip(filteringWord.Length).ToArray()), viewModel);
            await viewModel.FocusAsync();
            
            await ParseAsync(editContext, model, shouldApplySyntaxHighlighting: true);
        });
        
        return Task.CompletedTask;
    }
    
    public ValueTask<MenuRecord> GetQuickActionsSlashRefactorMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier)
    {
        var compilerService = modelModifier.PersistentState.CompilerService;
    
        var compilerServiceResource = viewModelModifier is null
            ? null
            : compilerService.GetResource(modelModifier.PersistentState.ResourceUri);

        int? primaryCursorPositionIndex = modelModifier is null || viewModelModifier is null
            ? null
            : modelModifier.GetPositionIndex(viewModelModifier);

        ISyntaxNode? syntaxNode = primaryCursorPositionIndex is null || __CSharpBinder is null || compilerServiceResource?.CompilationUnit is null
            ? null
            : null; // __CSharpBinder.GetSyntaxNode(null, primaryCursorPositionIndex.Value, (CSharpCompilationUnit)compilerServiceResource);
            
        var menuOptionList = new List<MenuOptionRecord>();
            
        menuOptionList.Add(new MenuOptionRecord(
            "QuickActionsSlashRefactorMenu",
            MenuOptionKind.Other));
            
        if (syntaxNode is null)
        {
            menuOptionList.Add(new MenuOptionRecord(
                "syntaxNode was null",
                MenuOptionKind.Other,
                onClickFunc: async _ => {}));
        }
        else
        {
            if (syntaxNode.SyntaxKind == SyntaxKind.TypeClauseNode)
            {
                var allTypeDefinitions = __CSharpBinder.AllTypeDefinitions;
                
                var typeClauseNode = (TypeClauseNode)syntaxNode;
                
                if (allTypeDefinitions.TryGetValue(typeClauseNode.TypeIdentifierToken.TextSpan.GetText(modelModifier.GetAllText(), _textEditorService), out var typeDefinitionNode))
                {
                    var usingStatementText = $"using {typeDefinitionNode.NamespaceName};";
                        
                    menuOptionList.Add(new MenuOptionRecord(
                        $"Copy: {usingStatementText}",
                        MenuOptionKind.Other,
                        onClickFunc: async _ =>
                        {
                            await _textEditorService.CommonService.SetClipboard(usingStatementText).ConfigureAwait(false);
                        }));
                }
                else
                {
                    menuOptionList.Add(new MenuOptionRecord(
                        "type not found",
                        MenuOptionKind.Other,
                        onClickFunc: async _ => {}));
                }
            }
            else
            {
                menuOptionList.Add(new MenuOptionRecord(
                    syntaxNode.SyntaxKind.ToString(),
                    MenuOptionKind.Other,
                    onClickFunc: async _ => {}));
            }
        }
        
        MenuRecord menu;
        
        if (menuOptionList.Count == 0)
            menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
        else
            menu = new MenuRecord(menuOptionList);
    
        return ValueTask.FromResult(menu);
    }
    
    public async ValueTask OnInspect(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        double clientX,
        double clientY,
        bool shiftKey,
        bool ctrlKey,
        bool altKey,
        TextEditorComponentData componentData,
        ResourceUri resourceUri)
    {
        // Lazily calculate row and column index a second time. Otherwise one has to calculate it every mouse moved event.
        var lineAndColumnIndex = await EventUtils.CalculateLineAndColumnIndex(
                modelModifier,
                viewModelModifier,
                clientX,
                clientY,
                componentData,
                editContext)
            .ConfigureAwait(false);
    
        var cursorPositionIndex = modelModifier.GetPositionIndex(
            lineAndColumnIndex.LineIndex,
            lineAndColumnIndex.ColumnIndex);

        var foundMatch = false;
        
        var resource = GetResource(modelModifier.PersistentState.ResourceUri);
        var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
        
        // var diagnostics = compilationUnitLocal.DiagnosticList;

        /*if (diagnostics.Count != 0)
        {
            foreach (var diagnostic in diagnostics)
            {
                if (cursorPositionIndex >= diagnostic.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < diagnostic.TextSpan.EndExclusiveIndex)
                {
                    // Prefer showing a diagnostic over a symbol when both exist at the mouse location.
                    foundMatch = true;

                    var parameterMap = new Dictionary<string, object?>
                    {
                        {
                            nameof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.DiagnosticDisplay.Diagnostic),
                            diagnostic
                        }
                    };

                    viewModelModifier.PersistentState.TooltipModel = new Walk.Common.RazorLib.Tooltips.Models.TooltipModel<(TextEditorService TextEditorService, Key<TextEditorViewModel> ViewModelKey, int PositionIndex)>(
                        typeof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.DiagnosticDisplay),
                        parameterMap,
                        clientX,
                        clientY,
                        cssClassString: null,
                        componentData.ContinueRenderingTooltipAsync,
                        Walk.TextEditor.RazorLib.Commands.Models.Defaults.TextEditorCommandDefaultFunctions.OnWheel,
                        (_textEditorService, viewModelModifier.PersistentState.ViewModelKey, cursorPositionIndex));
                    componentData.TextEditorViewModelSlimDisplay.TextEditorService.CommonService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
                }
            }
        }*/

        if (!foundMatch)
        {
            for (int i = compilationUnitLocal.IndexSymbolList; i < compilationUnitLocal.IndexSymbolList + compilationUnitLocal.CountSymbolList; i++)
            {
                var symbol = __CSharpBinder.SymbolList[i];
                
                if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex)
                {
                    foundMatch = true;

                    var parameters = new Dictionary<string, object?>
                    {
                        {
                            "Symbol",
                            symbol
                        },
                        {
                            "ResourceUri",
                            modelModifier.PersistentState.ResourceUri
                        }
                    };

                    viewModelModifier.PersistentState.TooltipModel = new Walk.Common.RazorLib.Tooltips.Models.TooltipModel<(TextEditorService TextEditorService, Key<TextEditorViewModel> ViewModelKey, int PositionIndex)>(
                        typeof(Walk.Extensions.CompilerServices.Displays.SymbolDisplay),
                        parameters,
                        clientX,
                        clientY,
                        cssClassString: null,
                        componentData.ContinueRenderingTooltipAsync,
                        Walk.TextEditor.RazorLib.Commands.Models.Defaults.TextEditorCommandDefaultFunctions.OnWheel,
                        (_textEditorService, viewModelModifier.PersistentState.ViewModelKey, cursorPositionIndex));
                    componentData.TextEditorViewModelSlimDisplay.TextEditorService.CommonService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
                    
                    break;
                }
            }
        }

        if (!foundMatch && viewModelModifier.PersistentState.TooltipModel is not null)
        {
            viewModelModifier.PersistentState.TooltipModel = null;
            componentData.TextEditorViewModelSlimDisplay.TextEditorService.CommonService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
        }

        // TODO: Measure the tooltip, and reposition if it would go offscreen.
    }
    
    public async ValueTask ShowCallingSignature(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        int positionIndex,
        TextEditorComponentData componentData,
        ResourceUri resourceUri)
    {
        return;
        /*var success = __CSharpBinder.TryGetCompilationUnit(
            cSharpCompilationUnit: null,
            resourceUri,
            out CSharpCompilationUnit compilationUnit);
            
        if (!success)
            return;
        
        var scope = __CSharpBinder.GetScopeByPositionIndex(compilationUnit, resourceUri, positionIndex);
        
        if (!scope.ConstructorWasInvoked)
            return;
        
        if (scope.CodeBlockOwner is null)
            return;
        
        if (!scope.CodeBlockOwner.CodeBlock.ConstructorWasInvoked)
            return;
        
        FunctionInvocationNode? functionInvocationNode = null;
        
        foreach (var childSyntax in scope.CodeBlockOwner.CodeBlock.ChildList)
        {
            if (childSyntax.SyntaxKind == SyntaxKind.ReturnStatementNode)
            {
                var returnStatementNode = (ReturnStatementNode)childSyntax;
                
                if (returnStatementNode.ExpressionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    functionInvocationNode = (FunctionInvocationNode)returnStatementNode.ExpressionNode;
                    break;
                }
            }
        
            if (functionInvocationNode is not null)
                break;
        
            if (childSyntax.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                functionInvocationNode = (FunctionInvocationNode)childSyntax;
                break;
            }
        }
        
        if (functionInvocationNode is null)
            return;
        
        var foundMatch = false;
        
        var resource = modelModifier.PersistentState.ResourceUri;
        var compilationUnitLocal = compilationUnit;
        
        var symbols = compilationUnitLocal.SymbolList;
        
        var cursorPositionIndex = functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex;
        
        var lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(cursorPositionIndex);
        
        var elementPositionInPixels = await _textEditorService.JsRuntimeTextEditorApi
            .GetBoundingClientRect(componentData.PrimaryCursorContentId)
            .ConfigureAwait(false);

        elementPositionInPixels = elementPositionInPixels with
        {
            Top = elementPositionInPixels.Top +
                (.9 * viewModelModifier.CharAndLineMeasurements.LineHeight)
        };
        
        var mouseEventArgs = new MouseEventArgs
        {
            ClientX = elementPositionInPixels.Left,
            ClientY = elementPositionInPixels.Top
        };
            
        var relativeCoordinatesOnClick = new RelativeCoordinates(
            mouseEventArgs.ClientX - viewModelModifier.TextEditorDimensions.BoundingClientRectLeft,
            mouseEventArgs.ClientY - viewModelModifier.TextEditorDimensions.BoundingClientRectTop,
            viewModelModifier.ScrollLeft,
            viewModelModifier.ScrollTop);

        if (!foundMatch && symbols.Count != 0)
        {
            foreach (var symbol in symbols)
            {
                if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex &&
                    symbol.SyntaxKind == SyntaxKind.FunctionSymbol)
                {
                    foundMatch = true;

                    var parameters = new Dictionary<string, object?>
                    {
                        {
                            "Symbol",
                            symbol
                        }
                    };

                    viewModelModifier.PersistentState.TooltipViewModel = new(
                        typeof(Walk.Extensions.CompilerServices.Displays.SymbolDisplay),
                        parameters,
                        relativeCoordinatesOnClick,
                        null,
                        componentData.ContinueRenderingTooltipAsync);
                        
                    break;
                }
            }
        }

        if (!foundMatch)
        {
            viewModelModifier.PersistentState.TooltipViewModel = null;
        }
        */
    }
    
    public async ValueTask GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        Category category,
        int positionIndex)
    {
        var cursorPositionIndex = positionIndex;

        var foundMatch = false;
        
        var resource = GetResource(modelModifier.PersistentState.ResourceUri);
        var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
        
        var symbolList = __CSharpBinder.SymbolList.Skip(compilationUnitLocal.IndexSymbolList).Take(compilationUnitLocal.CountSymbolList).ToList();
        var foundSymbol = default(Symbol);
        
        foreach (var symbol in symbolList)
        {
            if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex)
            {
                foundMatch = true;
                foundSymbol = symbol;
                break;
            }
        }
        
        if (!foundMatch)
            return;
    
        var symbolLocal = foundSymbol;
        var targetNode = SymbolDisplay.GetTargetNode(_textEditorService, symbolLocal, modelModifier.PersistentState.ResourceUri);
        var definitionNode = SymbolDisplay.GetDefinitionNode(_textEditorService, symbolLocal, targetNode, modelModifier.PersistentState.ResourceUri);
        
        if (definitionNode is null)
            return;
            
        string? resourceUriValue = null;
        var indexInclusiveStart = -1;
        var indexPartialTypeDefinition = -1;
        
        if (definitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
        {
            var typeDefinitionNode = (TypeDefinitionNode)definitionNode;
            resourceUriValue = typeDefinitionNode.ResourceUri.Value;
            indexInclusiveStart = typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex;
            indexPartialTypeDefinition = typeDefinitionNode.IndexPartialTypeDefinition;
        }
        else if (definitionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
        {
            var variableDeclarationNode = (VariableDeclarationNode)definitionNode;
            resourceUriValue = variableDeclarationNode.ResourceUri.Value;
            indexInclusiveStart = variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex;
        }
        else if (definitionNode.SyntaxKind == SyntaxKind.NamespaceStatementNode)
        {
            var namespaceStatementNode = (NamespaceStatementNode)definitionNode;
            resourceUriValue = namespaceStatementNode.ResourceUri.Value;
            indexInclusiveStart = namespaceStatementNode.IdentifierToken.TextSpan.StartInclusiveIndex;
        }
        else if (definitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
        {
            var functionDefinitionNode = (FunctionDefinitionNode)definitionNode;
            resourceUriValue = functionDefinitionNode.ResourceUri.Value;
            indexInclusiveStart = functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex;
        }
        else if (definitionNode.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
        {
            var constructorDefinitionNode = (ConstructorDefinitionNode)definitionNode;
            resourceUriValue = constructorDefinitionNode.ResourceUri.Value;
            indexInclusiveStart = constructorDefinitionNode.FunctionIdentifier.TextSpan.StartInclusiveIndex;
        }
        
        if (resourceUriValue is null || indexInclusiveStart == -1)
            return;
        
        if (indexPartialTypeDefinition == -1)
        {
            if (_textEditorService.CommonService.GetTooltipState().TooltipModel is not null)
            {
                _textEditorService.CommonService.SetTooltipModel(tooltipModel: null);
            }
            
            _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
            {
                if (category.Value == "CodeSearchService")
                {
                    await ((TextEditorKeymapDefault)TextEditorFacts.Keymap_DefaultKeymap).AltF12Func.Invoke(
                        editContext,
                        resourceUriValue,
                        indexInclusiveStart);
                }
                else
                {
                    await _textEditorService.OpenInEditorAsync(
                            editContext,
                            resourceUriValue,
                            true,
                            indexInclusiveStart,
                            category,
                            Key<TextEditorViewModel>.NewKey())
                        .ContinueWith(_ => _textEditorService.ViewModel_StopCursorBlinking());
                }
            });
        }
        else
        {
            var componentData = viewModelModifier.PersistentState.ComponentData;
            if (componentData is null)
                return;
        
            MeasuredHtmlElementDimensions cursorDimensions;
            
            var tooltipState = _textEditorService.CommonService.GetTooltipState();
            
            if (positionIndex != modelModifier.GetPositionIndex(viewModelModifier) &&
                tooltipState.TooltipModel.ItemUntyped is ValueTuple<TextEditorService, Key<TextEditorViewModel>, int>)
            {
                cursorDimensions = new MeasuredHtmlElementDimensions(
                    WidthInPixels: 0,
                    HeightInPixels: 0,
                    LeftInPixels: tooltipState.TooltipModel.X,
                    TopInPixels: tooltipState.TooltipModel.Y,
                    ZIndex: 0);
                _textEditorService.CommonService.SetTooltipModel(tooltipModel: null);
            }
            else
            {
                cursorDimensions = await _textEditorService.CommonService.JsRuntimeCommonApi
                    .MeasureElementById(componentData.PrimaryCursorContentId)
                    .ConfigureAwait(false);
            }
    
            var resourceAbsolutePath = _textEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
                modelModifier.PersistentState.ResourceUri.Value,
                false,
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder(),
                AbsolutePathNameKind.NameWithExtension);
        
            var siblingFileStringList = new List<(string ResourceUriValue, int ScopeIndexKey)>();
            
            int positionExclusive = indexPartialTypeDefinition;
            while (positionExclusive < __CSharpBinder.PartialTypeDefinitionList.Count)
            {
                if (__CSharpBinder.PartialTypeDefinitionList[positionExclusive].IndexStartGroup == indexPartialTypeDefinition)
                {
                    siblingFileStringList.Add(
                        (
                            __CSharpBinder.PartialTypeDefinitionList[positionExclusive].ResourceUri.Value,
                            __CSharpBinder.PartialTypeDefinitionList[positionExclusive].ScopeIndexKey
                        ));
                    positionExclusive++;
                }
                else
                {
                    break;
                }
            }
            
            var menuOptionList = new List<MenuOptionRecord>();
            
            siblingFileStringList = siblingFileStringList.OrderBy(x => x).ToList();
            
            var initialActiveMenuOptionRecordIndex = -1;
            
            for (int i = 0; i < siblingFileStringList.Count; i++)
            {
                var tuple = siblingFileStringList[i];
                var file = tuple.ResourceUriValue;
                
                var siblingAbsolutePath = _textEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(file, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder(), AbsolutePathNameKind.NameWithExtension);
                
                menuOptionList.Add(new MenuOptionRecord(
                    siblingAbsolutePath.Name,
                    MenuOptionKind.Other,
                    onClickFunc: async _ => 
                    {
                        int? positionIndex = null;
                        
                        if (__CSharpBinder.__CompilationUnitMap.TryGetValue(new ResourceUri(file), out var innerCompilationUnit))
                        {
                            ISyntaxNode? otherTypeDefinitionNode = null;
                            
                            for (int i = innerCompilationUnit.IndexCodeBlockOwnerList; i < innerCompilationUnit.IndexCodeBlockOwnerList + innerCompilationUnit.CountCodeBlockOwnerList; i++)
                            {
                                var x = __CSharpBinder.CodeBlockOwnerList[i];
                                
                                if (x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                                    ((TypeDefinitionNode)x).Unsafe_SelfIndexKey == tuple.ScopeIndexKey)
                                {
                                    otherTypeDefinitionNode = x;
                                    break;
                                }
                            }
                            
                            if (otherTypeDefinitionNode is not null)
                            {
                                var typeDefinitionNode = (TypeDefinitionNode)otherTypeDefinitionNode;
                                positionIndex = typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex;
                            }
                        }
                    
                        _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
                        {
                            if (category.Value == "CodeSearchService")
                            {
                                await ((TextEditorKeymapDefault)TextEditorFacts.Keymap_DefaultKeymap).AltF12Func.Invoke(
                                    editContext,
                                    file,
                                    positionIndex);
                            }
                            else
                            {
                                await _textEditorService.OpenInEditorAsync(
                                        editContext,
                                        file,
                                        true,
                                        positionIndex,
                                        category,
                                        Key<TextEditorViewModel>.NewKey())
                                    .ContinueWith(_ => _textEditorService.ViewModel_StopCursorBlinking());
                            }
                        });
                    }));
                        
                if (siblingAbsolutePath.Name == resourceAbsolutePath.Name)
                    initialActiveMenuOptionRecordIndex = i;
            }
            
            if (menuOptionList.Count == 1)
            {
                await menuOptionList[0].OnClickFunc.Invoke(default);
            }
            else
            {
                MenuRecord menu;
                
                if (menuOptionList.Count == 0)
                    menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
                else
                    menu = new MenuRecord(menuOptionList);
                
                menu.InitialActiveMenuOptionRecordIndex = initialActiveMenuOptionRecordIndex;
                
                var dropdownRecord = new DropdownRecord(
                    Key<DropdownRecord>.NewKey(),
                    cursorDimensions.LeftInPixels,
                    cursorDimensions.TopInPixels + cursorDimensions.HeightInPixels,
                    typeof(MenuDisplay),
                    new Dictionary<string, object?>
                    {
                        {
                            nameof(MenuDisplay.Menu),
                            menu
                        }
                    },
                    // TODO: this callback when the dropdown closes is suspect.
                    //       The editContext is supposed to live the lifespan of the
                    //       Post. But what if the Post finishes before the dropdown is closed?
                    async () => 
                    {
                        // TODO: Even if this '.single or default' to get the main group works it is bad and I am ashamed...
                        //       ...I'm too tired at the moment, need to make this sensible.
                        //       The key is in the IDE project yet its circular reference if I do so, gotta
                        //       make groups more sensible I'm not sure what to say here I'm super tired and brain checked out.
                        //       |
                        //       I ran this and it didn't work. Its for the best that it doesn't.
                        //       maybe when I wake up tomorrow I'll realize what im doing here.
                        var mainEditorGroup = _textEditorService.Group_GetTextEditorGroupState().GroupList.SingleOrDefault();
                        
                        if (mainEditorGroup is not null &&
                            mainEditorGroup.ActiveViewModelKey != Key<TextEditorViewModel>.Empty)
                        {
                            var activeViewModel = _textEditorService.ViewModel_GetOrDefault(mainEditorGroup.ActiveViewModelKey);
        
                            if (activeViewModel is not null)
                                await activeViewModel.FocusAsync();
                        }
                        
                        await viewModelModifier.FocusAsync();
                    });
        
                _textEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
            }
        }
    }
    
    /// <summary>
    /// This implementation is NOT thread safe.
    /// </summary>
    public ValueTask ParseAsync(TextEditorEditContext editContext, TextEditorModel modelModifier, bool shouldApplySyntaxHighlighting)
    {
        var resourceUri = modelModifier.PersistentState.ResourceUri;
    
        if (!__CSharpBinder.__CompilationUnitMap.ContainsKey(resourceUri))
            return ValueTask.CompletedTask;
    
        _textEditorService.Model_StartPendingCalculatePresentationModel(
            editContext,
            modelModifier,
            TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
            TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);

        var presentationModel = modelModifier.PresentationModelList.First(
            x => x.TextEditorPresentationKey == TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey);
        
        var cSharpCompilationUnit = new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData);

        _currentFileBeingParsedTuple = (resourceUri.Value, presentationModel.PendingCalculation.ContentAtRequest);
        _textEditorService.EditContext_GetText_Clear();

        CSharpLexerOutput lexerOutput;

        // Convert the string to a byte array using a specific encoding
        byte[] byteArray = Encoding.UTF8.GetBytes(presentationModel.PendingCalculation.ContentAtRequest);

        // Create a MemoryStream from the byte array
        using (MemoryStream memoryStream = new MemoryStream(byteArray))
        {
            // Create a StreamReader from the MemoryStream
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                _streamReaderWrap.ReInitialize(reader);
                lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, _streamReaderWrap, shouldUseSharedStringWalker: true);
            }
        }

        // Even if the parser throws an exception, be sure to
        // make use of the Lexer to do whatever syntax highlighting is possible.
        try
        {
            __CSharpBinder.StartCompilationUnit(resourceUri);
            CSharpParser.Parse(resourceUri, ref cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
        }
        finally
        {
            //var diagnosticTextSpans = cSharpCompilationUnit.DiagnosticList
            //    .Select(x => x.TextSpan)
            //    .ToList();

            modelModifier.CompletePendingCalculatePresentationModel(
                TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
                TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel,
                _emptyDiagnosticTextSpans);
            
            if (shouldApplySyntaxHighlighting)
            {
                editContext.TextEditorService.Model_ApplySyntaxHighlighting(
                    editContext,
                    modelModifier,
                    lexerOutput.SyntaxTokenList.Select(x => x.TextSpan)
                        .Concat(lexerOutput.MiscTextSpanList)
                        .Concat(__CSharpBinder.SymbolList.Skip(cSharpCompilationUnit.IndexSymbolList).Take(cSharpCompilationUnit.CountSymbolList).Select(x => x.TextSpan)));
            }

            _currentFileBeingParsedTuple = (null, null);

            Clear_MAIN_StreamReaderTupleCache();
            Clear_BACKUP_StreamReaderTupleCache();
        }
        
        return ValueTask.CompletedTask;
    }
    
    private readonly List<TextEditorTextSpan> _emptyDiagnosticTextSpans = new();
    private readonly SafeOnlyUTF8Encoding _safeOnlyUTF8Encoding;

    public async ValueTask FastParseAsync(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
    {
        var content = await fileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);
    
        if (!__CSharpBinder.__CompilationUnitMap.ContainsKey(resourceUri))
            return;

        var cSharpCompilationUnit = new CSharpCompilationUnit(compilationUnitKind);

        CSharpLexerOutput lexerOutput;

        using (StreamReader sr = new StreamReader(resourceUri.Value))
        {
            _streamReaderWrap.ReInitialize(sr);
            lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, _streamReaderWrap, shouldUseSharedStringWalker: true);
        }

        __CSharpBinder.StartCompilationUnit(resourceUri);
        CSharpParser.Parse(resourceUri, ref cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
    }
    
    public void FastParse(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
    {
        if (!__CSharpBinder.__CompilationUnitMap.ContainsKey(resourceUri))
            return;
    
        var cSharpCompilationUnit = new CSharpCompilationUnit(compilationUnitKind);

        CSharpLexerOutput lexerOutput;

        using (StreamReader sr = new StreamReader(resourceUri.Value, _safeOnlyUTF8Encoding))
        {
            _streamReaderWrap.ReInitialize(sr);
            
            lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, _streamReaderWrap, shouldUseSharedStringWalker: true);

            FastParseTuple = (resourceUri.Value, sr);
            __CSharpBinder.StartCompilationUnit(resourceUri);
            CSharpParser.Parse(resourceUri, ref cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
        }

        FastParseTuple = (null, null);
    }
    
    /// <summary>
    /// Looks up the <see cref="IScope"/> that encompasses the provided positionIndex.
    ///
    /// Then, checks the <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/>'s children
    /// to determine which node exists at the positionIndex.
    ///
    /// If the <see cref="IScope"/> cannot be found, then as a fallback the provided compilationUnit's
    /// <see cref="CompilationUnit.RootCodeBlockNode"/> will be treated
    /// the same as if it were the <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/>.
    ///
    /// If the provided compilerServiceResource?.CompilationUnit is null, then the fallback step will not occur.
    /// The fallback step is expected to occur due to the global scope being implemented with a null
    /// <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/> at the time of this comment.
    /// </summary>
    public ISyntaxNode? GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource)
    {
        return null;
        // return __CSharpBinder.GetSyntaxNode(compilationUnit: null, positionIndex, (CSharpCompilationUnit)compilerServiceResource);
    }
    
    /// <summary>
    /// Returns the <see cref="ISyntaxNode"/> that represents the definition in the <see cref="CompilationUnit"/>.
    ///
    /// The option argument 'symbol' can be provided if available. It might provide additional information to the method's implementation
    /// that is necessary to find certain nodes (ones that are in a separate file are most common to need a symbol to find).
    /// </summary>
    public ISyntaxNode? GetDefinitionNode(TextEditorTextSpan textSpan, ResourceUri resourceUri, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null)
    {
        if (symbol is null)
            return null;
        
        if (__CSharpBinder.__CompilationUnitMap.TryGetValue(resourceUri, out var compilationUnit))
            return __CSharpBinder.GetDefinitionNode(resourceUri, compilationUnit, textSpan, symbol.Value.SyntaxKind, symbol);
        
        return null;
    }

    public ICodeBlockOwner? GetScopeByPositionIndex(ResourceUri resourceUri, int positionIndex)
    {
        if (__CSharpBinder.__CompilationUnitMap.TryGetValue(resourceUri, out var compilationUnit))
            return __CSharpBinder.GetScopeByPositionIndex(compilationUnit, positionIndex);
        
        return null;
    }
    
    public List<AutocompleteEntry>? OBSOLETE_GetAutocompleteEntries(string word, TextEditorTextSpan textSpan, TextEditorVirtualizationResult virtualizationResult)
    {
        if (word is null || !__CSharpBinder.__CompilationUnitMap.TryGetValue(virtualizationResult.Model.PersistentState.ResourceUri, out var compilationUnit))
            return null;
            
        var boundScope = __CSharpBinder.GetScope(compilationUnit, textSpan);

        if (boundScope is null)
            return null;
        
        var autocompleteEntryList = new List<AutocompleteEntry>();

        var targetScope = boundScope;
        
        if (textSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService) == ".")
        {
            var textEditorModel = virtualizationResult.Model;
            if (textEditorModel is null)
                return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
            
            var compilerService = textEditorModel.PersistentState.CompilerService;
            
            var compilerServiceResource = compilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
            if (compilerServiceResource is null)
                return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
    
            var targetNode = __CSharpBinder.GetSyntaxNode(
                (CSharpCompilationUnit)compilerServiceResource.CompilationUnit,
                textSpan.StartInclusiveIndex - 1,
                (CSharpCompilationUnit)compilerServiceResource);
                
            if (targetNode is null)
                return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
        
            TypeReference typeReference = default;
    
            if (targetNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                var variableReferenceNode = (VariableReferenceNode)targetNode;
            
                if (variableReferenceNode.VariableDeclarationNode is not null)
                {
                    typeReference = variableReferenceNode.VariableDeclarationNode.TypeReference;
                }
            }
            else if (targetNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                typeReference = ((VariableDeclarationNode)targetNode).TypeReference;
            }
            else if (targetNode.SyntaxKind == SyntaxKind.TypeClauseNode)
            {
                typeReference = new TypeReference((TypeClauseNode)targetNode);
            }
            else if (targetNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                typeReference = ((TypeDefinitionNode)targetNode).ToTypeReference();
            }
            else if (targetNode.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
            {
                typeReference = ((ConstructorDefinitionNode)targetNode).ReturnTypeReference;
            }
            
            if (typeReference == default)
                return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
            
            var maybeTypeDefinitionNode = __CSharpBinder.GetDefinitionNode(virtualizationResult.Model.PersistentState.ResourceUri, (CSharpCompilationUnit)compilerServiceResource.CompilationUnit, typeReference.TypeIdentifierToken.TextSpan, SyntaxKind.TypeClauseNode);
            if (maybeTypeDefinitionNode is null || maybeTypeDefinitionNode.SyntaxKind != SyntaxKind.TypeDefinitionNode)
                return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
            
            var typeDefinitionNode = (TypeDefinitionNode)maybeTypeDefinitionNode;
            var memberList = __CSharpBinder.GetMemberList_TypeDefinitionNode(typeDefinitionNode);
            
            autocompleteEntryList.AddRange(
                memberList
                .Select(node => 
                {
                    if (node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                    {
                        var variableDeclarationNode = (VariableDeclarationNode)node;
                        return variableDeclarationNode.IdentifierToken.TextSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService);
                    }
                    else if (node.SyntaxKind == SyntaxKind.TypeDefinitionNode)
                    {
                        var typeDefinitionNode = (TypeDefinitionNode)node;
                        return typeDefinitionNode.TypeIdentifierToken.TextSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService);
                    }
                    else if (node.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                    {
                        var functionDefinitionNode = (FunctionDefinitionNode)node;
                        return functionDefinitionNode.FunctionIdentifierToken.TextSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService);
                    }
                    else
                    {
                        return string.Empty;
                    }
                })
                .ToArray()
                //.Where(x => x.Contains(word, StringComparison.InvariantCulture))
                .Distinct()
                .Take(5)
                .Select(x =>
                {
                    return new AutocompleteEntry(
                        x,
                        AutocompleteEntryKind.Variable,
                        null);
                }));
        }
        else
        {
            while (targetScope is not null)
            {
                autocompleteEntryList.AddRange(
                    __CSharpBinder.GetVariableDeclarationNodesByScope(virtualizationResult.Model.PersistentState.ResourceUri, compilationUnit, targetScope.Unsafe_SelfIndexKey)
                    .Select(x => __CSharpBinder.GetIdentifierText(x, virtualizationResult.Model.PersistentState.ResourceUri, compilationUnit))
                    .ToArray()
                    .Where(x => x?.Contains(word, StringComparison.InvariantCulture) ?? false)
                    .Distinct()
                    .Take(5)
                    .Select(x =>
                    {
                        return new AutocompleteEntry(
                            x,
                            AutocompleteEntryKind.Variable,
                            null);
                    }));
    
                autocompleteEntryList.AddRange(
                    __CSharpBinder.GetFunctionDefinitionNodesByScope(compilationUnit, targetScope.Unsafe_SelfIndexKey)
                    .Select(x => x.FunctionIdentifierToken.TextSpan.GetText(virtualizationResult.Model.GetAllText(), _textEditorService))
                    .ToArray()
                    .Where(x => x.Contains(word, StringComparison.InvariantCulture))
                    .Distinct()
                    .Take(5)
                    .Select(x =>
                    {
                        return new AutocompleteEntry(
                            x,
                            AutocompleteEntryKind.Function,
                            null);
                    }));
    
                if (targetScope.Unsafe_ParentIndexKey == -1)
                    targetScope = default;
                else
                    targetScope = __CSharpBinder.GetScopeByScopeIndexKey(compilationUnit, targetScope.Unsafe_ParentIndexKey);
            }
        
            var allTypeDefinitions = __CSharpBinder.AllTypeDefinitions;
    
            autocompleteEntryList.AddRange(
                allTypeDefinitions
                .Where(x => x.Key.Contains(word, StringComparison.InvariantCulture))
                .Distinct()
                .Take(5)
                .Select(x =>
                {
                    return new AutocompleteEntry(
                        x.Key,
                        AutocompleteEntryKind.Type,
                        () =>
                        {
                            // TODO: The namespace code is buggy at the moment.
                            //       It is annoying how this keeps adding the wrong namespace.
                            //       Just have it do nothing for now. (2024-08-24)
                            // ===============================================================
                            /*if (boundScope.EncompassingNamespaceStatementNode.IdentifierToken.TextSpan.GetText() == x.Key.NamespaceIdentifier ||
                                boundScope.CurrentUsingStatementNodeList.Any(usn => usn.NamespaceIdentifier.TextSpan.GetText() == x.Key.NamespaceIdentifier))
                            {
                                return Task.CompletedTask;
                            }
    
                            _textEditorService.PostUnique(
                                "Add using statement",
                                editContext =>
                                {
                                    var modelModifier = editContext.GetModelModifier(textSpan.ResourceUri);
    
                                    if (modelModifier is null)
                                        return Task.CompletedTask;
    
                                    var viewModelList = _textEditorService.ModelApi.GetViewModelsOrEmpty(textSpan.ResourceUri);
    
                                    var cursor = new TextEditorCursor(0, 0, true);
                                    var cursorModifierBag = new CursorModifierBagTextEditor(
                                        Key<TextEditorViewModel>.Empty,
                                        new List<TextEditorCursorModifier> { new(cursor) });
    
                                    var textToInsert = $"using {x.Key.NamespaceIdentifier};\n";
    
                                    modelModifier.Insert(
                                        textToInsert,
                                        cursorModifierBag,
                                        cancellationToken: CancellationToken.None);
    
                                    foreach (var unsafeViewModel in viewModelList)
                                    {
                                        var viewModelModifier = editContext.GetViewModelModifier(unsafeViewModel.ViewModelKey);
                                        var viewModelCursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier);
    
                                        if (viewModelModifier is null || viewModelCursorModifierBag is null)
                                            continue;
    
                                        foreach (var cursorModifier in viewModelCursorModifierBag.List)
                                        {
                                            for (int i = 0; i < textToInsert.Length; i++)
                                            {
                                                _textEditorService.ViewModelApi.MoveCursor(
                                                    new KeyboardEventArgs
                                                    {
                                                        Key = KeyboardKeyFacts.MovementKeys.ARROW_RIGHT,
                                                    },
                                                    editContext,
                                                    modelModifier,
                                                    viewModelModifier,
                                                    viewModelCursorModifierBag);
                                            }
                                        }
    
                                        editContext.TextEditorService.ModelApi.ApplySyntaxHighlighting(
                                            editContext,
                                            modelModifier);
                                    }
    
                                    return Task.CompletedTask;
                                });*/
                            return Task.CompletedTask;
                        });
                }));
        }
        
        /*foreach (var namespaceGroupKvp in __CSharpBinder.NamespacePrefixTree.__Root.Children.Where(x => x.Key.Contains(word)).Take(5))
        {
            autocompleteEntryList.Add(new AutocompleteEntry(
                namespaceGroupKvp.Key,
                AutocompleteEntryKind.Namespace,
                () => Task.CompletedTask));
        }*/
            
        AddSnippets(autocompleteEntryList, word, textSpan, virtualizationResult.Model.PersistentState.ResourceUri);

        return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
    }
    
    private void AddSnippets(List<AutocompleteEntry> autocompleteEntryList, string word, TextEditorTextSpan textSpan, ResourceUri resourceUri)
    {
        if ("prop".Contains(word))
        {
            autocompleteEntryList.Add(new AutocompleteEntry(
                "prop",
                AutocompleteEntryKind.Snippet,
                () => PropSnippet(word, textSpan, "public TYPE NAME { get; set; }", resourceUri)));
        }
        
        if ("propnn".Contains(word))
        {
            autocompleteEntryList.Add(new AutocompleteEntry(
                "propnn",
                AutocompleteEntryKind.Snippet,
                () => PropSnippet(word, textSpan, "public TYPE NAME { get; set; } = null!;", resourceUri)));
        }
    }
    
    private Task PropSnippet(string word, TextEditorTextSpan textSpan, string textToInsert, ResourceUri resourceUri)
    {
        _textEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(resourceUri);

            if (modelModifier is null)
                return ValueTask.CompletedTask;

            var viewModelList = _textEditorService.Model_GetViewModelsOrEmpty(resourceUri);
            
            var viewModel = viewModelList.FirstOrDefault(x => x.PersistentState.Category.Value == "main")
                ?? viewModelList.FirstOrDefault();
            
            if (viewModel is null)
                return ValueTask.CompletedTask;
                
            var viewModelModifier = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);
            
            if (viewModelModifier is null)
                return ValueTask.CompletedTask;

            var cursorPositionIndex = modelModifier.GetPositionIndex(viewModelModifier);
            var behindPositionIndex = cursorPositionIndex - 1;
                    
            modelModifier.Insert(
                textToInsert,
                viewModelModifier);
                
            /*if (behindPositionIndex > 0 && modelModifier.GetCharacter(behindPositionIndex) == 'p')
            {
                modelModifier.Delete(
                    viewModelModifier,
                    1,
                    expandWord: false,
                    TextEditorModel.DeleteKind.Delete);
            }*/

            modelModifier.PersistentState.CompilerService.ResourceWasModified(
                (ResourceUri)modelModifier.PersistentState.ResourceUri,
                (IReadOnlyList<TextEditorTextSpan>)Array.Empty<TextEditorTextSpan>());
                
            return ValueTask.CompletedTask;
        }));
            
        return Task.CompletedTask;
    }
    
    public string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri)
    {
        if (__CSharpBinder.__CompilationUnitMap.TryGetValue(resourceUri, out var compilationUnit))
            return __CSharpBinder.GetIdentifierText(node, resourceUri, compilationUnit) ?? string.Empty;
    
        return string.Empty;
    }
}
