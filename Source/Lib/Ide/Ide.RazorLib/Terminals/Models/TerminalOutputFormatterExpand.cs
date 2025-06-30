using System.Text;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalOutputFormatterExpand : ITerminalOutputFormatter
{
	public Guid Id { get; } = Guid.NewGuid();

	public ResourceUri TextEditorModelResourceUri { get; }

    public Key<TextEditorViewModel> TextEditorViewModelKey { get; }

	private readonly ITerminal _terminal;
	private readonly TextEditorService _textEditorService;
	private readonly ICompilerServiceRegistry _compilerServiceRegistry;
	private readonly ICommonUiService _commonUiService;
	private readonly CommonBackgroundTaskApi _commonBackgroundTaskApi;

	public TerminalOutputFormatterExpand(
		ITerminal terminal,
		TextEditorService textEditorService,
		ICompilerServiceRegistry compilerServiceRegistry,
		ICommonUiService commonUiService,
        CommonBackgroundTaskApi commonBackgroundTaskApi)
	{
		_terminal = terminal;
		_textEditorService = textEditorService;
		_compilerServiceRegistry = compilerServiceRegistry;
		_commonUiService = commonUiService;
		_commonBackgroundTaskApi = commonBackgroundTaskApi;
		
		TextEditorModelResourceUri = new(
			ResourceUriFacts.Terminal_ReservedResourceUri_Prefix + Id.ToString());
		
		TextEditorViewModelKey = new Key<TextEditorViewModel>(Id);
		
		CreateTextEditor();
	}

	public string Name { get; } = nameof(TerminalOutputFormatterExpand);
	
	public ITerminalOutputFormatted Format()
	{
		var outSymbolList = new List<Symbol>();
		var outTextSpanList = new List<TextEditorTextSpan>();
		
		var parsedCommandList = _terminal.TerminalOutput.GetParsedCommandList();
		
		var outputBuilder = new StringBuilder();
		
		foreach (var parsedCommand in parsedCommandList)
		{
			var workingDirectoryText = parsedCommand.SourceTerminalCommandRequest.WorkingDirectory + "> ";
		
			var workingDirectoryTextSpan = new TextEditorTextSpan(
				outputBuilder.Length,
		        outputBuilder.Length + workingDirectoryText.Length,
		        (byte)TerminalDecorationKind.Keyword,
		        ResourceUri.Empty,
		        string.Empty,
		        workingDirectoryText);
		    outTextSpanList.Add(workingDirectoryTextSpan);
		    
			outputBuilder
				.Append(workingDirectoryText)
				.Append(parsedCommand.SourceTerminalCommandRequest.CommandText)
				.Append('\n');
				
			var parsedCommandTextSpanList = parsedCommand.TextSpanList;
			
			if (parsedCommandTextSpanList is not null)
			{
			    outTextSpanList.AddRange(parsedCommandTextSpanList.Select(
			    	textSpan => textSpan with
			    	{
					    StartInclusiveIndex = textSpan.StartInclusiveIndex + outputBuilder.Length,
					    EndExclusiveIndex = textSpan.EndExclusiveIndex + outputBuilder.Length,
			    	}));
			}
			
			outputBuilder.Append(parsedCommand.OutputCache.ToString());
		}
		
		return new TerminalOutputFormattedTextEditor(
			outputBuilder.ToString(),
			parsedCommandList,
			outTextSpanList,
			outSymbolList);
	}
	
	private void CreateTextEditor()
    {
    	_textEditorService.WorkerArbitrary.PostUnique(editContext =>
    	{
    		var model = new TextEditorModel(
	            TextEditorModelResourceUri,
	            DateTime.UtcNow,
	            "terminal",
	            string.Empty,
	            new TerminalDecorationMapper(),
	            _compilerServiceRegistry.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL),
            	_textEditorService)
	        {
	        	UseUnsetOverride = true,
	        	UnsetOverrideLineEndKind = LineEndKind.LineFeed,
	        };
	            
	        var modelModifier = new TextEditorModel(model);
	        modelModifier.PerformRegisterPresentationModelAction(TerminalPresentationFacts.EmptyPresentationModel);
	        modelModifier.PerformRegisterPresentationModelAction(CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);
	        modelModifier.PerformRegisterPresentationModelAction(FindOverlayPresentationFacts.EmptyPresentationModel);
	        
	        model = modelModifier;
	
	        _textEditorService.ModelApi.RegisterCustom(editContext, model);
	        
			model.PersistentState.CompilerService.RegisterResource(
				model.PersistentState.ResourceUri,
				shouldTriggerResourceWasModified: true);
				
	        var viewModel = new TextEditorViewModel(
	            TextEditorViewModelKey,
	            TextEditorModelResourceUri,
	            _textEditorService,
	            _commonUiService,
	            _commonBackgroundTaskApi,
	            TextEditorVirtualizationResult.Empty,
				new TextEditorDimensions(0, 0, 0, 0),
				scrollLeft: 0,
		    	scrollTop: 0,
			    scrollWidth: 0,
			    scrollHeight: 0,
			    marginScrollHeight: 0,
	            new Category("terminal"));
	
	        var firstPresentationLayerKeys = new List<Key<TextEditorPresentationModel>>()
	        {
	            TerminalPresentationFacts.PresentationKey,
	            CompilerServiceDiagnosticPresentationFacts.PresentationKey,
	            FindOverlayPresentationFacts.PresentationKey,
	        };
	            
	        viewModel.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;
	        
	        _textEditorService.ViewModelApi.Register(editContext, viewModel);
	        
	        return ValueTask.CompletedTask;
    	});
    }
    
    public void Dispose()
    {
    	// TODO: Dispose of the text editor resources
    }
}
