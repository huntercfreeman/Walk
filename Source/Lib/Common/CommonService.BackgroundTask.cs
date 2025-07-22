using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Exceptions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private readonly Dictionary<Key<IBackgroundTaskGroup>, TaskCompletionSource> _taskCompletionSourceMap = new();
    private readonly object _taskCompletionSourceLock = new();
    
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

        Indefinite_Enqueue(backgroundTask);
            
        return taskCompletionSource.Task;
    }

    public Task Indefinite_EnqueueAsync(Key<IBackgroundTaskGroup> taskKey, int queueKey, string name, Func<ValueTask> runFunc)
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
}
