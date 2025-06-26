using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

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
    
    public event Action? ResourceRegistered;
    public event Action? ResourceParsed;
    public event Action? ResourceDisposed;

    public IReadOnlyList<ICompilerServiceResource> CompilerServiceResources { get; }
    
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions { get; }
    
    public Type? SymbolRendererType { get; }
    public Type? DiagnosticRendererType { get; }

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
	        
        ResourceRegistered?.Invoke();
    }
    
    public void DisposeResource(ResourceUri resourceUri)
    {
    	lock (_resourceMapLock)
        {
            _resourceMap.Remove(resourceUri);
        }

        ResourceDisposed?.Invoke();
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
    	var model = _textEditorService.ModelApi.GetOrDefault(resourceUri);

        if (model is null)
            return null;

        lock (_resourceMapLock)
        {
            if (!_resourceMap.ContainsKey(resourceUri))
                return null;

            return _resourceMap[resourceUri];
        }
    }
    
    public MenuRecord GetContextMenu(TextEditorRenderBatch renderBatch, ContextMenu contextMenu)
	{
		return contextMenu.GetDefaultMenuRecord();
	}

	public MenuRecord GetAutocompleteMenu(TextEditorRenderBatch renderBatch, AutocompleteMenu autocompleteMenu)
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
		IWalkTextEditorComponentRenderers textEditorComponentRenderers,
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
		IWalkTextEditorComponentRenderers textEditorComponentRenderers,
        ResourceUri resourceUri)
    {
    	return ValueTask.CompletedTask;
    }
    
    public ValueTask GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        Category category)
    {
    	return ValueTask.CompletedTask;
    }

	public ValueTask ParseAsync(TextEditorEditContext editContext, TextEditorModel modelModifier, bool shouldApplySyntaxHighlighting)
    {
    	var lexer = new JavaScriptLexer(
		    modelModifier.PersistentState.ResourceUri,
		    modelModifier.GetAllText());
		    
		lexer.Lex();
    
    	lock (_resourceMapLock)
		{
			if (_resourceMap.ContainsKey(modelModifier.PersistentState.ResourceUri))
			{
				var resource = _resourceMap[modelModifier.PersistentState.ResourceUri];
				
				resource.CompilationUnit = new ExtendedCompilationUnit
				{
					TokenList = lexer.SyntaxTokenList,
				};
			}
		}
		
		editContext.TextEditorService.ModelApi.ApplySyntaxHighlighting(
			editContext,
			modelModifier);

		ResourceParsed?.Invoke();
		
		return ValueTask.CompletedTask;
    }
	
	public ValueTask FastParseAsync(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
	{
		return ValueTask.CompletedTask;
	}
}
