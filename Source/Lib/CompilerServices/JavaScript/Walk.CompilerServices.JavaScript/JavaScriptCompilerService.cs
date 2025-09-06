using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptCompilerService : ICompilerService
{
    private readonly TextEditorService _textEditorService;
    
    private readonly Dictionary<ResourceUri, JavaScriptResource> _resourceMap = new();
    private readonly object _resourceMapLock = new();

    public JavaScriptCompilerService(TextEditorService textEditorService)
    {
        _textEditorService = textEditorService;
    }

    public IReadOnlyList<ICompilerServiceResource> CompilerServiceResources { get; }
    
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions { get; }
    
    public char[] KeywordCheckBuffer { get; } = new char[10];

    public void RegisterResource(ResourceUri resourceUri, bool shouldTriggerResourceWasModified)
    {
        lock (_resourceMapLock)
        {
            if (_resourceMap.ContainsKey(resourceUri))
                return;

            _resourceMap.Add(resourceUri, new JavaScriptResource(resourceUri, this));
        }

        if (shouldTriggerResourceWasModified)
            ResourceWasModified(resourceUri, Array.Empty<TextEditorTextSpan>());
    }
    
    public void DisposeResource(ResourceUri resourceUri)
    {
        lock (_resourceMapLock)
        {
            _resourceMap.Remove(resourceUri);
        }
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
        var model = _textEditorService.Model_GetOrDefault(resourceUri);

        if (model is null)
            return null;

        lock (_resourceMapLock)
        {
            if (!_resourceMap.ContainsKey(resourceUri))
                return null;

            return _resourceMap[resourceUri];
        }
    }
    
    public MenuRecord GetContextMenu(TextEditorVirtualizationResult virtualizationResult, ContextMenu contextMenu)
    {
        return contextMenu.GetDefaultMenuRecord();
    }

    public MenuRecord GetAutocompleteMenu(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu)
    {
        return autocompleteMenu.GetDefaultMenuRecord();
    }
    
    public ValueTask<MenuRecord> GetQuickActionsSlashRefactorMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier)
    {
        return ValueTask.FromResult(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
    }
    
    public ValueTask OnInspect(
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
        return ValueTask.CompletedTask;
    }
    
    public ValueTask ShowCallingSignature(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        int positionIndex,
        TextEditorComponentData componentData,
        ResourceUri resourceUri)
    {
        return ValueTask.CompletedTask;
    }
    
    public ValueTask GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        Category category,
        int positionIndex)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ParseAsync(TextEditorEditContext editContext, TextEditorModel modelModifier, bool shouldApplySyntaxHighlighting)
    {
        using StreamReader sr = new StreamReader(modelModifier.PersistentState.ResourceUri.Value);
        _textEditorService.LEXER_miscTextSpanList.Clear();
        var lexerOutput = JavaScriptLexer.Lex(this, new StreamReaderWrap(sr), _textEditorService.LEXER_miscTextSpanList);
    
        lock (_resourceMapLock)
        {
            if (_resourceMap.ContainsKey(modelModifier.PersistentState.ResourceUri))
            {
                var resource = _resourceMap[modelModifier.PersistentState.ResourceUri];
                
                /*resource.CompilationUnit = new JavaScriptCompilationUnit
                {
                    TextSpanList = lexerOutput.TextSpanList
                };*/
            }
        }
        
        editContext.TextEditorService.Model_ApplySyntaxHighlighting(
            editContext,
            modelModifier,
            lexerOutput.TextSpanList);
        
        return ValueTask.CompletedTask;
    }
    
    public ValueTask FastParseAsync(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
    {
        return ValueTask.CompletedTask;
    }
    
    public void FastParse(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
    {
        return;
    }
}
