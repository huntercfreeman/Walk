using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

/// <summary>
/// TODO: Investigate making this a record struct
/// TODO: 'Key<DotNetSolutionModel>? DotNetSolutionModelKey' should not be nullable use Key<DotNetSolutionModel>.Empty.
/// </summary>
public record DotNetSolutionState(
    Key<DotNetSolutionModel>? DotNetSolutionModelKey,
    int IsExecutingAsyncTaskLinks)
{
    public static readonly Key<TreeViewContainer> TreeViewSolutionExplorerStateKey = Key<TreeViewContainer>.NewKey();

    public DotNetSolutionState() : this(Key<DotNetSolutionModel>.Empty, 0)
    {
    }

    public List<DotNetSolutionModel> DotNetSolutionsList { get; init; } = new();

    public DotNetSolutionModel? DotNetSolutionModel => DotNetSolutionsList.FirstOrDefault(x =>
        x.Key == DotNetSolutionModelKey);

    public static void ShowInputFile(
    	IdeService ideService,
    	DotNetBackgroundTaskApi dotNetBackgroundTaskApi)
    {
        ideService.Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.RequestInputFileStateForm,
			StringValue = "Solution Explorer",
            OnAfterSubmitFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is not null)
                    dotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
                    {
                    	WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
                    	DotNetSolutionAbsolutePath = absolutePath,
                    });

				return Task.CompletedTask;
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(
                    absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION ||
    				absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X);
            },
            InputFilePatterns = new()
            {
                new InputFilePattern(
                    ".NET Solution",
                    absolutePath => absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION ||
                    				absolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X)
            }
        });
    }
}