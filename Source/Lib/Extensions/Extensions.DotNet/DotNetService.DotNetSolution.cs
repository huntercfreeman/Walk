using Walk.Common.RazorLib.Keys.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
	private DotNetSolutionState _dotNetSolutionState = new();

	public event Action? DotNetSolutionStateChanged;

	public DotNetSolutionState GetDotNetSolutionState() => _dotNetSolutionState;

	public void ReduceRegisterAction(DotNetSolutionModel argumentDotNetSolutionModel)
	{
		var inState = GetDotNetSolutionState();

		var dotNetSolutionModel = inState.DotNetSolutionModel;

		if (dotNetSolutionModel is not null)
		{
			DotNetSolutionStateChanged?.Invoke();
			return;
		}

		var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
		nextList.Add(argumentDotNetSolutionModel);

		_dotNetSolutionState = inState with
		{
			DotNetSolutionsList = nextList
		};

		DotNetSolutionStateChanged?.Invoke();
		return;
	}

	public void ReduceDisposeAction(Key<DotNetSolutionModel> dotNetSolutionModelKey)
	{
		var inState = GetDotNetSolutionState();

		var dotNetSolutionModel = inState.DotNetSolutionModel;

		if (dotNetSolutionModel is null)
		{
			DotNetSolutionStateChanged?.Invoke();
			return;
		}

		var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
		nextList.Remove(dotNetSolutionModel);

		_dotNetSolutionState = inState with
		{
			DotNetSolutionsList = nextList
		};

		DotNetSolutionStateChanged?.Invoke();
		return;
	}

	public void ReduceWithAction(IWithAction withActionInterface)
	{
		var inState = GetDotNetSolutionState();

		var withAction = (WithAction)withActionInterface;
		_dotNetSolutionState = withAction.WithFunc.Invoke(inState);

		DotNetSolutionStateChanged?.Invoke();
		return;
	}

	public Task NotifyDotNetSolutionStateStateHasChanged()
	{
		return Task.CompletedTask;
	}
}
