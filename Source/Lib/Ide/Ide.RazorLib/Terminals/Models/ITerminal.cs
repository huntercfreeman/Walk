using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Ide.RazorLib.Terminals.Models;

/// <summary>input -> CliWrap -> output</summary>
public interface ITerminal : IDisposable
{
    public Key<ITerminal> Key { get; }
    public string DisplayName { get; }
    public ITerminalOutput TerminalOutput { get; }
    public bool HasExecutingProcess { get; }
    
    public void EnqueueCommand(TerminalCommandRequest terminalCommandRequest);
    public Task EnqueueCommandAsync(TerminalCommandRequest terminalCommandRequest);
    
    /// <summary>
    /// This will enqueue the command with text "clear".
    /// Thus, it will only execute when it is its turn in the queue.
    /// </summary>
    public void ClearEnqueue();
    
    /// <summary>
    /// This will invoke the <see cref="ITerminalOutput.ClearOutput"/> method,
    /// by using '_ = Task.Run(...)'.
    ///
    /// This will execute EVEN IF a command in the queue is currently being executed.
    ///
    /// The fire and forget is because the terminal <see cref="ITerminalOutput"/> wraps
    /// any state mutation in a 'lock(...) { }'. So, the fire and forget is to avoid
    /// freezing the UI.
    /// </summary>
    public void ClearFireAndForget();
    
    public void KillProcess();
    
    /* Start ITerminalInteractive */
    /// <summary>This property is intended to be an absolute path in string form</summary>
    public string? WorkingDirectory { get; }
    
    public event Action? WorkingDirectoryChanged;
    
    public void SetWorkingDirectory(string workingDirectoryAbsolutePathString);
    public List<TerminalCommandRequest> GetTerminalCommandRequestHistory();
    
    /// <summary>
    /// Some terminal commands will map to "interactive" commands that are
    /// meant to modify the shell session rather than to start a process.
    ///
    /// Ex: 'cd ../..' is an "interactive" command,
    ///     where as 'dotnet run' will start a process.
    ///
    /// This method gives the <see cref="ITerminalInteractive"/> an opportunity to
    /// handle the command, before it is executed at a more general level.
    ///
    /// Presumably, 'dotnet run ../MyProject.csproj' where there is a ".." in a path,
    /// would need to be taken by the <see cref="ITerminalInteractive"/>, and then
    /// modified to replace "../MyProject.csproj" with the result of traversing
    /// this relative path from the working directory path.
    ///
    /// The final result would then be returned for the <see cref="ITerminal"/>
    /// to execute, "dotnet run C:/User/MyProject/MyProject.cs"
    /// if the working directory were to be "C:/User/MyProject/wwwroot/".
    ///
    /// If null is returned, then the <see cref="ITerminal"/>
    /// should return (do nothing more).
    /// </summary>
    public Task<TerminalCommandParsed?> TryHandleCommand(TerminalCommandRequest terminalCommandRequest);
    /* End ITerminalInteractive */
}
