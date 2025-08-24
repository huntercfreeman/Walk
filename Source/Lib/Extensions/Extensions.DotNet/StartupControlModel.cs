using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Extensions.DotNet.CommandLines.Models;

namespace Walk.Extensions.DotNet;

public class StartupControlModel : IStartupControlModel
{
    public StartupControlModel(
        string title,
        AbsolutePath startupProjectAbsolutePath)
    {
        Title = title;
        StartupProjectAbsolutePath = startupProjectAbsolutePath;
    }
    
    public string Title { get; }
    public AbsolutePath StartupProjectAbsolutePath { get; }
    
    public TerminalCommandRequest? ExecutingTerminalCommandRequest { get; set; }
    public bool IsExecuting => ExecutingTerminalCommandRequest is not null;
    
    public Task StartButtonOnClick(object item)
    {
        var dotNetService = (DotNetService)item;
    
        var ancestorDirectory = StartupProjectAbsolutePath.CreateSubstringParentDirectory();
        if (ancestorDirectory is null)
        {
            return Task.CompletedTask;
        }

        var formattedCommandValue = DotNetCliCommandFormatter.FormatStartProjectWithoutDebugging(
            StartupProjectAbsolutePath);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommandValue,
            ancestorDirectory,
            dotNetService.NewDotNetSolutionTerminalCommandRequestKey)
        {
            BeginWithFunc = parsedCommand =>
            {
                dotNetService.ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Run-Project_started");

                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                ExecutingTerminalCommandRequest = null;
                dotNetService.IdeService.Ide_TriggerStartupControlStateChanged();

                dotNetService.ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Run-Project_completed");

                return Task.CompletedTask;
            }
        };

        ExecutingTerminalCommandRequest = terminalCommandRequest;

        dotNetService.IdeService.GetTerminalState().ExecutionTerminal.EnqueueCommand(terminalCommandRequest);
        return Task.CompletedTask;
    }

    public Task StopButtonOnClick(object item)
    {
        var dotNetService = (DotNetService)item;
    
        dotNetService.IdeService.GetTerminalState().ExecutionTerminal.KillProcess();
        ExecutingTerminalCommandRequest = null;

        dotNetService.IdeService.Ide_TriggerStartupControlStateChanged();
        return Task.CompletedTask;
    }
}
