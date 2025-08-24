using System.Text;
using CliWrap.EventStream;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib.Exceptions;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Ide.RazorLib.Terminals.Models;

/// <summary>
/// This implementation of <see cref="ITerminal"/> is for the website demo.
/// </summary>
public class TerminalWebsite : ITerminal, IBackgroundTaskGroup
{
    private readonly CommonService _commonService;
    private readonly TextEditorService _textEditorService;

    public TerminalWebsite(
        string displayName,
        CommonService commonService,
        TextEditorService textEditorService)
    {
        DisplayName = displayName;

        _commonService = commonService;
        _textEditorService = textEditorService;
        
        TextEditorModelResourceUri = new(
            ResourceUriFacts.Terminal_ReservedResourceUri_Prefix + Id.ToString());
        
        TextEditorViewModelKey = new Key<TextEditorViewModel>(Id);
        
        CreateTextEditor();
    }

    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly Queue<TerminalWorkKind> _workKindQueue = new();
    private readonly object _workLock = new();

    public string DisplayName { get; }

    private CancellationTokenSource _commandCancellationTokenSource = new();

    public Key<ITerminal> Key { get; init; } = Key<ITerminal>.NewKey();
    public TerminalCommandParsed? ActiveTerminalCommandParsed { get; private set; }

    /// <summary>NOTE: the following did not work => _process?.HasExited ?? false;</summary>
    public bool HasExecutingProcess { get; private set; }

    private readonly Queue<TerminalCommandRequest> _queue_general_TerminalCommandRequest = new();

    public void EnqueueCommand(TerminalCommandRequest terminalCommandRequest)
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(TerminalWorkKind.Command);
            _queue_general_TerminalCommandRequest.Enqueue(terminalCommandRequest);
            _commonService.Indefinite_Enqueue(this);
        }
    }

    public ValueTask DoCommand(TerminalCommandRequest terminalCommandRequest)
    {
        return HandleCommand(terminalCommandRequest);
    }

    public Task EnqueueCommandAsync(TerminalCommandRequest terminalCommandRequest)
    {
        return _commonService.Indefinite_EnqueueAsync(
            Key<IBackgroundTaskGroup>.NewKey(),
            CommonFacts.IndefiniteQueueKey,
            "Enqueue Command",
            () => HandleCommand(terminalCommandRequest));
    }
    
    public void ClearEnqueue()
    {
        EnqueueCommand(new TerminalCommandRequest("clear", null));
    }
    
    public void ClearFireAndForget()
    {
        var localHasExecutingProcess = HasExecutingProcess;
    
        _ = Task.Run(() =>
        {
            if (localHasExecutingProcess)
            {
                ClearOutputExceptMostRecentCommand();
            }
            else
            {
                ClearOutput();
            }
            
            return Task.CompletedTask;
        });
    }

    private async ValueTask HandleCommand(TerminalCommandRequest terminalCommandRequest)
    {
        ClearHistoryWhenExistingOutputTooLong();
    
        var parsedCommand = await TryHandleCommand(terminalCommandRequest);
        ActiveTerminalCommandParsed = parsedCommand;

        if (parsedCommand is null)
            return;
        
        try
        {
            HasExecutingProcess = true;
        
            if (parsedCommand.SourceTerminalCommandRequest.BeginWithFunc is not null)
            {
                await parsedCommand.SourceTerminalCommandRequest.BeginWithFunc
                    .Invoke(parsedCommand)
                    .ConfigureAwait(false);
            }
            
            await ExecuteWebsiteCliAsync(parsedCommand, HandleOutput, _commandCancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // TODO: This will erroneously write 'StartedCommandEvent' out twice...
            //       ...unless a check is added to see WHEN the exception was thrown.
            WriteOutput(parsedCommand, new StartedCommandEvent(-1));
        
            WriteOutput(
                parsedCommand,
                new StandardErrorCommandEvent(parsedCommand.SourceTerminalCommandRequest.CommandText + " threw an exception" + "\n"));
        
            NotificationHelper.DispatchError("Terminal Exception", e.ToString(), _commonService, TimeSpan.FromSeconds(14));
        }
        finally
        {
            if (parsedCommand.SourceTerminalCommandRequest.ContinueWithFunc is not null)
            {
                await parsedCommand.SourceTerminalCommandRequest.ContinueWithFunc
                    .Invoke(parsedCommand)
                    .ConfigureAwait(false);
            }
        
            HasExecutingProcess = false;
        }
    }
    
    private void HandleOutput(CommandEvent commandEvent)
    {
        WriteOutput(ActiveTerminalCommandParsed, commandEvent);
    }
    
    private Task ExecuteWebsiteCliAsync(
        TerminalCommandParsed parsedCommand,
        Action<CommandEvent> handleOutputAction,
        CancellationToken cancellationToken)
    {
        WriteOutput(parsedCommand, new StartedCommandEvent(-1));
        
        WriteOutput(
            parsedCommand, new StandardErrorCommandEvent("run source locally for terminal"));
            
        return Task.CompletedTask;
    }
    
    /* This method is in progress
    
    private Task ExecuteWebsiteCliAsync(
        TerminalCommandParsed parsedCommand,
        Action<CommandEvent> handleOutputAction,
        CancellationToken cancellationToken)
    {
        TerminalOutput.WriteOutput(parsedCommand, new StartedCommandEvent(-1));
    
        switch (parsedCommand.TargetFileName)
        {
            case TerminalWebsiteFacts.TargetFileNames.DOT_NET:
                return DotNetWebsiteCliAsync(parsedCommand, handleOutputAction, cancellationToken);
        }
        
        TerminalOutput.WriteOutput(
            parsedCommand,
            new StandardErrorCommandEvent($"Target file name: '{parsedCommand.TargetFileName}' was not recognized."));
        return Task.CompletedTask;
    }
    */
    
    private Task DotNetWebsiteCliAsync(
        TerminalCommandParsed parsedCommand,
        Action<CommandEvent> handleOutputAction,
        CancellationToken cancellationToken)
    {
        var argumentList = parsedCommand.Arguments.Trim().Split(' ');
    
        var firstArgument = argumentList.FirstOrDefault();
        
        if (string.IsNullOrWhiteSpace(firstArgument))
        {
            WriteOutput(parsedCommand, new StandardErrorCommandEvent($"firstArgument was null or whitespace."));
        }
    
        switch (firstArgument)
        {
            case IdeFacts.InitialArguments_RUN:
            {
                var projectPath = (string?)null;
            
                if (argumentList.Length == 2)
                {
                    projectPath = argumentList[1];
                }
                else if (argumentList.Length == 3)
                {
                    if (argumentList[1] != IdeFacts.Options_PROJECT)
                    {
                        WriteOutput(parsedCommand, new StandardErrorCommandEvent($"argumentList[1]:{argumentList[1]} != {IdeFacts.Options_PROJECT}"));
                        return Task.CompletedTask;
                    }
                
                    projectPath = argumentList[2];
                }
                else
                {
                    WriteOutput(parsedCommand, new StandardErrorCommandEvent($"Bad argument count: '{argumentList.Length}'."));
                    return Task.CompletedTask;
                }
            
                WriteOutput(parsedCommand, new StandardErrorCommandEvent($"projectPath:{projectPath}"));
                return Task.CompletedTask;
            }
            default:
            {
                WriteOutput(parsedCommand, new StandardErrorCommandEvent($"First argument: '{firstArgument}' was not recognized."));
                return Task.CompletedTask;
            }
        }
        
        throw new WalkIdeException($"The method '{nameof(DotNetWebsiteCliAsync)}' should return on each branch of the switch statement.");
    }

    public void KillProcess()
    {
        _commandCancellationTokenSource.Cancel();
        _commandCancellationTokenSource = new();
        DispatchNewStateKey();
    }

    private void DispatchNewStateKey()
    {
        // _dispatcher.Dispatch(new TerminalState.NotifyStateChangedAction(Key));
    }

    public ValueTask HandleEvent()
    {
        TerminalWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case TerminalWorkKind.Command:
            {
                var args = _queue_general_TerminalCommandRequest.Dequeue();
                return DoCommand(args);
            }
            default:
            {
                Console.WriteLine($"{nameof(TerminalWebsite)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
            }
        }
    }

    public void Dispose()
    {
    }
    
    /* Start TerminalInteractive */
    public const string RESERVED_TARGET_FILENAME_PREFIX = "Walk_";

    private readonly object _syncRoot = new();
    private readonly List<TerminalCommandRequest> _terminalCommandRequestHistory = new();

    private string? _previousWorkingDirectory;
    private string? _workingDirectory;
    
    public string? WorkingDirectory => _workingDirectory;

    public event Action? WorkingDirectoryChanged;
    
    public async Task<TerminalCommandParsed?> TryHandleCommand(TerminalCommandRequest terminalCommandRequest)
    {
        // Store in history
        lock (_syncRoot)
        {
            if (_terminalCommandRequestHistory.Count > 10)
                _terminalCommandRequestHistory.Clear();
                
            _terminalCommandRequestHistory.Insert(0, terminalCommandRequest);
        }
    
        var parsedCommand = Parse(terminalCommandRequest);
        
        // To set the working directory, is not mutually exclusive
        // to the "cd" command. Do not combine these.
        if (terminalCommandRequest.WorkingDirectory is not null &&
            terminalCommandRequest.WorkingDirectory != WorkingDirectory)
        {
            SetWorkingDirectory(terminalCommandRequest.WorkingDirectory);
        }
        
        if (parsedCommand.TargetFileName.StartsWith(RESERVED_TARGET_FILENAME_PREFIX))
        {
            WriteOutput(
                parsedCommand,
                new StartedCommandEvent(-1));
        
            await parsedCommand.SourceTerminalCommandRequest.BeginWithFunc.Invoke(parsedCommand);
            return null;
        }
        
        switch (parsedCommand.TargetFileName)
        {
            case "cd":
                WriteOutput(
                    parsedCommand,
                    new StartedCommandEvent(-1));
            
                SetWorkingDirectory(parsedCommand.Arguments);
                
                WriteOutput(
                    parsedCommand,
                    new StandardOutputCommandEvent($"WorkingDirectory set to: '{parsedCommand.Arguments}'\n"));
                return null;
            case "clear":
                ClearOutput();
                return null;
            default:
                return parsedCommand;
        }
    }
    
    public void SetWorkingDirectory(string workingDirectory)
    {
        _previousWorkingDirectory = _workingDirectory;
        _workingDirectory = workingDirectory;

        if (_previousWorkingDirectory != _workingDirectory)
            WorkingDirectoryChanged?.Invoke();
    }
    
    public List<TerminalCommandRequest> GetTerminalCommandRequestHistory()
    {
        lock (_syncRoot)
        {
            return _terminalCommandRequestHistory;
        }
    }
    
    public TerminalCommandParsed Parse(TerminalCommandRequest terminalCommandRequest)
    {
        try
        {
            var stringWalker = new StringWalker(ResourceUri.Empty, terminalCommandRequest.CommandText);
            
            // Get target file name
            string targetFileName;
            {
                var targetFileNameBuilder = new StringBuilder();
                var startPositionIndex = stringWalker.PositionIndex;
        
                while (!stringWalker.IsEof)
                {
                    if (stringWalker.CurrentCharacter == ' ' ||
                        stringWalker.CurrentCharacter == '\t' ||
                        stringWalker.CurrentCharacter == '\r' ||
                        stringWalker.CurrentCharacter == '\n')
                    {
                        break;
                    }
                    else
                    {
                        targetFileNameBuilder.Append(stringWalker.CurrentCharacter);
                    }
                
                    _ = stringWalker.ReadCharacter();
                }
                
                targetFileName = targetFileNameBuilder.ToString();
            }
            
            // Get arguments
            stringWalker.SkipWhitespace();
            var arguments = stringWalker.RemainingText;
        
            return new TerminalCommandParsed(
                targetFileName,
                arguments,
                terminalCommandRequest);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
    /* End TerminalInteractive */
    
    /* Start TerminalOutput */
    private readonly ITerminal _terminal;
    
    private readonly List<TerminalCommandParsed> _parsedCommandList = new();
    private readonly object _listLock = new();

    public event Action? OnWriteOutput;
    
    public ITerminalOutputFormatted? GetOutputFormatted()
    {
        return Format();
    }
    
    public TerminalCommandParsed? GetParsedCommandOrDefault(Key<TerminalCommandRequest> terminalCommandRequestKey)
    {
        lock (_listLock)
        {
            return _parsedCommandList.FirstOrDefault(x =>
                x.SourceTerminalCommandRequest.Key == terminalCommandRequestKey);
        }
    }
    
    public List<TerminalCommandParsed> GetParsedCommandList()
    {
        lock (_listLock)
        {
            return _parsedCommandList;
        }
    }
    
    public int GetParsedCommandListCount()
    {
        lock (_listLock)
        {
            return _parsedCommandList.Count;
        }
    }
    
    public void WriteOutput(TerminalCommandParsed terminalCommandParsed, CommandEvent commandEvent)
    {
        var output = (string?)null;

        switch (commandEvent)
        {
            case StartedCommandEvent started:
                
                // Delete any output of the previous invocation.
                lock (_listLock)
                {
                    var indexPreviousOutput = _parsedCommandList.FindIndex(x =>
                        x.SourceTerminalCommandRequest.Key ==
                            terminalCommandParsed.SourceTerminalCommandRequest.Key);
                            
                    if (indexPreviousOutput != -1)
                        _parsedCommandList.RemoveAt(indexPreviousOutput);
                        
                    _parsedCommandList.Add(terminalCommandParsed);
                }
                
                break;
            case StandardOutputCommandEvent stdOut:
                terminalCommandParsed.OutputCache.AppendTwo(stdOut.Text, "\n");
                break;
            case StandardErrorCommandEvent stdErr:
                terminalCommandParsed.OutputCache.AppendTwo(stdErr.Text, "\n");
                break;
            case ExitedCommandEvent exited:
                break;
        }
        
        OnWriteOutput?.Invoke();
    }
    
    public void ClearOutput()
    {
        lock (_listLock)
        {
            _parsedCommandList.Clear();
        }

        OnWriteOutput?.Invoke();
    }
    
    public void ClearOutputExceptMostRecentCommand()
    {
        lock (_listLock)
        {
            var rememberLastCommand = _parsedCommandList.LastOrDefault();
            
            _parsedCommandList.Clear();
            
            if (rememberLastCommand is not null &&
                rememberLastCommand.OutputCache.GetLength() < IdeFacts.MAX_OUTPUT_LENGTH)
            {
                _parsedCommandList.Add(rememberLastCommand);
            }
        }

        OnWriteOutput?.Invoke();
    }
    
    public void ClearHistoryWhenExistingOutputTooLong()
    {
        lock (_listLock)
        {
            var sumOutputLength = _parsedCommandList.Sum(x => x.OutputCache.GetLength());

            if (sumOutputLength > IdeFacts.MAX_OUTPUT_LENGTH ||
                _parsedCommandList.Count > IdeFacts.MAX_COMMAND_COUNT)
            {
                var rememberLastCommand = _parsedCommandList.LastOrDefault();
            
                _parsedCommandList.Clear();
                
                if (rememberLastCommand is not null &&
                    rememberLastCommand.OutputCache.GetLength() < IdeFacts.OUTPUT_LENGTH_PADDING)
                {
                    // It feels odd to clear the entire terminal when there is too much text output
                    // that has accumulated.
                    //
                    // So, keep the most recent command's output,
                    // unless its output length is greater than or equal to the TerminalOutputFacts.OUTPUT_LENGTH_PADDING.
                    _parsedCommandList.Add(rememberLastCommand);
                }
            }
        }

        OnWriteOutput?.Invoke();
    }
    /* End TerminalOutput */
    
    /* Start ITerminalOutputFormatter */
    public Guid Id { get; } = Guid.NewGuid();

    public ResourceUri TextEditorModelResourceUri { get; }

    public Key<TextEditorViewModel> TextEditorViewModelKey { get; }

    public ITerminalOutputFormatted Format()
    {
        var outSymbolList = new List<Symbol>();
        var outTextSpanList = new List<TextEditorTextSpan>();
        
        var parsedCommandList = _terminal.GetParsedCommandList();
        
        var outputBuilder = new StringBuilder();
        
        foreach (var parsedCommand in parsedCommandList)
        {
            var workingDirectoryText = parsedCommand.SourceTerminalCommandRequest.WorkingDirectory + "> ";
        
            var workingDirectoryTextSpan = new TextEditorTextSpan(
                outputBuilder.Length,
                outputBuilder.Length + workingDirectoryText.Length,
                (byte)GenericDecorationKind.Terminal_Keyword);
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
                new GenericDecorationMapper(),
                _textEditorService.GetCompilerService(CommonFacts.TERMINAL),
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
    /* End ITerminalOutputFormatter */
}

