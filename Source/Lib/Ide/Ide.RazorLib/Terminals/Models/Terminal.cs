using System.Text;
using System.Reactive.Linq;
using CliWrap;
using CliWrap.EventStream;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

/// <summary>
/// This implementation of <see cref="ITerminal"/> is a "blank slate".
/// </summary>
public class Terminal : ITerminal, IBackgroundTaskGroup
{
    private readonly IdeService _ideService;
    
    /// <summary>The TArgs of byte is unused</summary>
    private readonly ThrottleOptimized<byte> _throttleUiUpdateFromSetHasExecutingProcess;

    public Terminal(
        string displayName,
        Func<Terminal, ITerminalOutput> terminalOutputFactory,
        IdeService ideService)
    {
        DisplayName = displayName;
        TerminalOutput = terminalOutputFactory.Invoke(this);
        
        _ideService = ideService;
        
        _throttleUiUpdateFromSetHasExecutingProcess = new(
            DelaySetHasExecutingProcess,
            (_, _) =>
            {
                _ideService.Terminal_HasExecutingProcess_StateHasChanged();
                return Task.CompletedTask;
            });
    }

    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly Queue<TerminalWorkKind> _workKindQueue = new();
    private readonly object _workLock = new();

    public static readonly TimeSpan DelaySetHasExecutingProcess = TimeSpan.FromMilliseconds(200);

    public string DisplayName { get; }
    public ITerminalOutput TerminalOutput { get; }

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
            _ideService.CommonService.Indefinite_Enqueue(this);
        }
    }

    public ValueTask DoCommand(TerminalCommandRequest terminalCommandRequest)
    {
        return HandleCommand(terminalCommandRequest);
    }
    
    public Task EnqueueCommandAsync(TerminalCommandRequest terminalCommandRequest)
    {
        return _ideService.CommonService.Indefinite_EnqueueAsync(
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
                TerminalOutput.ClearOutputExceptMostRecentCommand();
            }
            else
            {
                TerminalOutput.ClearOutput();
            }
            
            return Task.CompletedTask;
        });
    }

    private async ValueTask HandleCommand(TerminalCommandRequest terminalCommandRequest)
    {
        TerminalOutput.ClearHistoryWhenExistingOutputTooLong();
    
        var parsedCommand = await TryHandleCommand(terminalCommandRequest);
        ActiveTerminalCommandParsed = parsedCommand;

        if (parsedCommand is null)
            return;
        
        var cliWrapCommand = Cli.Wrap(parsedCommand.TargetFileName);

        cliWrapCommand = cliWrapCommand.WithWorkingDirectory(WorkingDirectory);

        if (!string.IsNullOrWhiteSpace(parsedCommand.Arguments))
            cliWrapCommand = cliWrapCommand.WithArguments(parsedCommand.Arguments);
        
        // TODO: Decide where to put invocation of 'parsedCommand.SourceTerminalCommandRequest.BeginWithFunc'...
        //       ...and invocation of 'cliWrapCommand'
        //       and invocation of 'parsedCommand.SourceTerminalCommandRequest.ContinueWithFunc'
        //       |
        //       This comment is referring to the 'try/catch' block logic.
        //       If the 'BeginWithFunc' throws an exception should the 'cliWrapCommand' run?
        //       If the 'cliWrapCommand' throws an exception should the 'ContinueWithFunc' run?
        
        try
        {
            SetHasExecutingProcess(true);
        
            if (parsedCommand.SourceTerminalCommandRequest.BeginWithFunc is not null)
            {
                await parsedCommand.SourceTerminalCommandRequest.BeginWithFunc
                    .Invoke(parsedCommand)
                    .ConfigureAwait(false);
            }
            
            await cliWrapCommand
                .Observe(_commandCancellationTokenSource.Token)
                .ForEachAsync(HandleOutput)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // TODO: This will erroneously write 'StartedCommandEvent' out twice...
            //       ...unless a check is added to see WHEN the exception was thrown.
            TerminalOutput.WriteOutput(
                parsedCommand,
                new StartedCommandEvent(-1));
        
            TerminalOutput.WriteOutput(
                parsedCommand,
                new StandardErrorCommandEvent(
                    parsedCommand.SourceTerminalCommandRequest.CommandText +
                    " threw an exception" +
                    "\n"));
        
            NotificationHelper.DispatchError("Terminal Exception", e.ToString(), _ideService.CommonService, TimeSpan.FromSeconds(14));
        }
        finally
        {
            if (parsedCommand.SourceTerminalCommandRequest.ContinueWithFunc is not null)
            {
                try
                {
                    // The code 'SetHasExecutingProcess(false);' needs to run
                    // So, in the case that their ContinueWithFunc throws an exception
                    // make sure its wrapped in a try catch block.
                    await parsedCommand.SourceTerminalCommandRequest.ContinueWithFunc
                        .Invoke(parsedCommand)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        
            SetHasExecutingProcess(false);
        }
    }
    
    private void HandleOutput(CommandEvent commandEvent)
    {
        TerminalOutput.WriteOutput(ActiveTerminalCommandParsed, commandEvent);
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
    
    public void SetHasExecutingProcess(bool value)
    {
        HasExecutingProcess = value;
        _throttleUiUpdateFromSetHasExecutingProcess.Run(default(byte));
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
                Console.WriteLine($"{nameof(Terminal)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
            }
        }
    }

    public void Dispose()
    {
        TerminalOutput.Dispose();
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
            TerminalOutput.WriteOutput(
                parsedCommand,
                new StartedCommandEvent(-1));
        
            await parsedCommand.SourceTerminalCommandRequest.BeginWithFunc.Invoke(parsedCommand);
            return null;
        }
        
        switch (parsedCommand.TargetFileName)
        {
            case "cd":
                TerminalOutput.WriteOutput(
                    parsedCommand,
                    new StartedCommandEvent(-1));
            
                SetWorkingDirectory(parsedCommand.Arguments);
                
                TerminalOutput.WriteOutput(
                    parsedCommand,
                    new StandardOutputCommandEvent($"WorkingDirectory set to: '{parsedCommand.Arguments}'\n"));
                return null;
            case "clear":
                TerminalOutput.ClearOutput();
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
}
