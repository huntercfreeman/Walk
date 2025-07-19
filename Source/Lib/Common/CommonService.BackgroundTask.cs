using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private readonly Dictionary<Key<IBackgroundTaskGroup>, TaskCompletionSource> _taskCompletionSourceMap = new();
    private readonly object _taskCompletionSourceLock = new();
    
	/// <summary>
	/// Generally speaking: Presume that the ContinuousTaskWorker is "always ready" to run the next task that gets enqueued.
	/// </summary>
	public ContinuousBackgroundTaskWorker ContinuousWorker { get; private set; }
	/// <summary>
	/// Generally speaking: Presume that the ContinuousTaskWorker is "always ready" to run the next task that gets enqueued.
	/// </summary>
	public BackgroundTaskQueue ContinuousQueue { get; private set; }
	
	/// <summary>
	/// Generally speaking: Presume that the IndefiniteTaskWorker is NOT ready to run the next task that gets enqueued.
	/// </summary>
    public IndefiniteBackgroundTaskWorker IndefiniteWorker { get; private set; }
    /// <summary>
	/// Generally speaking: Presume that the IndefiniteTaskWorker is NOT ready to run the next task that gets enqueued.
	/// </summary>
    public BackgroundTaskQueue IndefiniteQueue { get; private set; }

	public void Continuous_EnqueueGroup(IBackgroundTaskGroup backgroundTaskGroup)
	{
		ContinuousQueue.Enqueue(backgroundTaskGroup);
	}
	
	public void Indefinite_EnqueueGroup(IBackgroundTaskGroup backgroundTaskGroup)
	{
		IndefiniteQueue.Enqueue(backgroundTaskGroup);
	}

    public Task Indefinite_EnqueueAsync(IBackgroundTaskGroup backgroundTask)
    {
    	backgroundTask.__TaskCompletionSourceWasCreated = true;
    	
    	if (backgroundTask.BackgroundTaskKey == Key<IBackgroundTaskGroup>.Empty)
    	{
    		throw new WalkCommonException(
    			$"{nameof(Indefinite_EnqueueAsync)} cannot be invoked with an {nameof(IBackgroundTaskGroup)} that has a 'BackgroundTaskKey == Key<IBackgroundTask>.Empty'. An empty key disables tracking, and task completion source. The non-async Enqueue(...) will still work however.");
    	}

        TaskCompletionSource taskCompletionSource = new();
            
		lock (_taskCompletionSourceLock)
		{
			if (_taskCompletionSourceMap.ContainsKey(backgroundTask.BackgroundTaskKey))
	        {
	        	var existingTaskCompletionSource = _taskCompletionSourceMap[backgroundTask.BackgroundTaskKey];
	        	
	        	if (!existingTaskCompletionSource.Task.IsCompleted)
	        	{
	        		existingTaskCompletionSource.SetException(new InvalidOperationException("SIMULATED EXCEPTION"));
	        	}
	        	
	        	// Retrospective: Shouldn't this be in an 'else'?
	        	//
	        	// The re-use of the key is not an issue, so long as the previous usage has completed
        		_taskCompletionSourceMap[backgroundTask.BackgroundTaskKey] = taskCompletionSource;
	        }
	        else
	        {
	        	_taskCompletionSourceMap.Add(backgroundTask.BackgroundTaskKey, taskCompletionSource);
	        }
		}

        IndefiniteQueue.Enqueue(backgroundTask);
			
		return taskCompletionSource.Task;
    }

    public Task Indefinite_EnqueueAsync(Key<IBackgroundTaskGroup> taskKey, Key<BackgroundTaskQueue> queueKey, string name, Func<ValueTask> runFunc)
    {
        return Indefinite_EnqueueAsync(new BackgroundTask(taskKey, runFunc));
    }
    
    public void CompleteTaskCompletionSource(Key<IBackgroundTaskGroup> backgroundTaskKey)
    {
    	lock (_taskCompletionSourceLock)
		{
			if (_taskCompletionSourceMap.ContainsKey(backgroundTaskKey))
	        {
	        	var existingTaskCompletionSource = _taskCompletionSourceMap[backgroundTaskKey];
	        	
	        	if (!existingTaskCompletionSource.Task.IsCompleted)
	        		existingTaskCompletionSource.SetResult();
	        	
	        	_taskCompletionSourceMap.Remove(backgroundTaskKey);
	        }
		}
    }

    public void SetContinuousWorker(ContinuousBackgroundTaskWorker worker)
    {
    	ContinuousWorker = worker;
    }
    
    public void SetContinuousQueue(BackgroundTaskQueue queue)
    {
    	ContinuousQueue = queue;
    }
    
    public void SetIndefiniteWorker(IndefiniteBackgroundTaskWorker worker)
    {
    	IndefiniteWorker = worker;
    }
    
    public void SetIndefiniteQueue(BackgroundTaskQueue queue)
    {
    	IndefiniteQueue = queue;
    }
}
