using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Ide.RazorLib.Terminals.Models;

public class TerminalOutputFormatterExpand : ITerminalOutputFormatter
{
    public Guid Id { get; } = Guid.NewGuid();

    public ResourceUri TextEditorModelResourceUri { get; }

    public Key<TextEditorViewModel> TextEditorViewModelKey { get; }

    private readonly ITerminal _terminal;
    private readonly TextEditorService _textEditorService;

    public TerminalOutputFormatterExpand(
        ITerminal terminal,
        TextEditorService textEditorService)
    {
        _terminal = terminal;
        _textEditorService = textEditorService;
        
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
                (byte)TerminalDecorationKind.Keyword);
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
                _textEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL),
                _textEditorService)
            {
                UseUnsetOverride = true,
                UnsetOverrideLineEndKind = LineEndKind.LineFeed,
            };
                
            var modelModifier = new TextEditorModel(model);
            modelModifier.PerformRegisterPresentationModelAction(IdeFacts.Terminal_EmptyPresentationModel);
            modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
            modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
            
            model = modelModifier;
    
            _textEditorService.Model_RegisterCustom(editContext, model);
            
            model.PersistentState.CompilerService.RegisterResource(
                model.PersistentState.ResourceUri,
                shouldTriggerResourceWasModified: true);
                
            var viewModel = new TextEditorViewModel(
                TextEditorViewModelKey,
                TextEditorModelResourceUri,
                _textEditorService,
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
                IdeFacts.Terminal_PresentationKey,
                TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
                TextEditorFacts.FindOverlayPresentation_PresentationKey,
            };
                
            viewModel.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;
            
            _textEditorService.ViewModel_Register(editContext, viewModel);
            
            return ValueTask.CompletedTask;
        });
    }
    
    public void Dispose()
    {
        // TODO: Dispose of the text editor resources
    }
}
