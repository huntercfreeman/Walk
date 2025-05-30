using Walk.Common.RazorLib.Keys.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

public interface IDotNetSolutionService
{
	public event Action? DotNetSolutionStateChanged;
	
	public DotNetSolutionState GetDotNetSolutionState();

    public void ReduceRegisterAction(DotNetSolutionModel dotNetSolutionModel);

    public void ReduceDisposeAction(Key<DotNetSolutionModel> dotNetSolutionModelKey);

    public void ReduceWithAction(DotNetBackgroundTaskApi.IWithAction withActionInterface);
    
	public Task NotifyDotNetSolutionStateStateHasChanged();
}
