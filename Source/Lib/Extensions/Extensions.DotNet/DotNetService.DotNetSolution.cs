using Walk.Common.RazorLib.Keys.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
    private DotNetSolutionState _dotNetSolutionState = new();

    public DotNetSolutionState GetDotNetSolutionState() => _dotNetSolutionState;

    public void ReduceRegisterAction(DotNetSolutionModel argumentDotNetSolutionModel)
    {
        var inState = GetDotNetSolutionState();

        var dotNetSolutionModel = inState.DotNetSolutionModel;

        if (dotNetSolutionModel is not null)
        {
            DotNetStateChanged?.Invoke(DotNetStateChangedKind.SolutionStateChanged);
            return;
        }

        var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
        nextList.Add(argumentDotNetSolutionModel);

        _dotNetSolutionState = inState with
        {
            DotNetSolutionsList = nextList
        };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.SolutionStateChanged);
        return;
    }

    public void ReduceDisposeAction(Key<DotNetSolutionModel> dotNetSolutionModelKey)
    {
        var inState = GetDotNetSolutionState();

        var dotNetSolutionModel = inState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
        {
            DotNetStateChanged?.Invoke(DotNetStateChangedKind.SolutionStateChanged);
            return;
        }

        var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
        nextList.Remove(dotNetSolutionModel);

        _dotNetSolutionState = inState with
        {
            DotNetSolutionsList = nextList
        };

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.SolutionStateChanged);
        return;
    }

    public void ReduceWithAction(IWithAction withActionInterface)
    {
        var inState = GetDotNetSolutionState();

        var withAction = (WithAction)withActionInterface;
        _dotNetSolutionState = withAction.WithFunc.Invoke(inState);

        DotNetStateChanged?.Invoke(DotNetStateChangedKind.SolutionStateChanged);
        return;
    }

    public Task NotifyDotNetSolutionStateStateHasChanged()
    {
        return Task.CompletedTask;
    }
}
