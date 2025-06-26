using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Ide.RazorLib.StartupControls.Models;

public class StartupControlService : IStartupControlService
{
    private readonly object _stateModificationLock = new();

    private StartupControlState _startupControlState = new();
	
	public event Action? StartupControlStateChanged;
	
	public StartupControlState GetStartupControlState() => _startupControlState;

	public void RegisterStartupControl(IStartupControlModel startupControl)
	{
		lock (_stateModificationLock)
		{
			var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
				x => x.Key == startupControl.Key);

			if (indexOfStartupControl != -1)
			{
    			var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
    			outStartupControlList.Add(startupControl);
    
    			_startupControlState = _startupControlState with
    			{
    				StartupControlList = outStartupControlList
    			};
    	    }
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void DisposeStartupControl(Key<IStartupControlModel> startupControlKey)
	{
		lock (_stateModificationLock)
		{
			var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
				x => x.Key == startupControlKey);

			if (indexOfStartupControl != -1)
            {
                var outActiveStartupControlKey = _startupControlState.ActiveStartupControlKey;
    			if (_startupControlState.ActiveStartupControlKey == startupControlKey)
    				outActiveStartupControlKey = Key<IStartupControlModel>.Empty;
    
    			var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
    			outStartupControlList.RemoveAt(indexOfStartupControl);
    
    			_startupControlState = _startupControlState with
    			{
    				StartupControlList = outStartupControlList,
    				ActiveStartupControlKey = outActiveStartupControlKey
    			};
            }
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void SetActiveStartupControlKey(Key<IStartupControlModel> startupControlKey)
	{
		lock (_stateModificationLock)
		{
			var startupControl = _startupControlState.StartupControlList.FirstOrDefault(
				x => x.Key == startupControlKey);

			if (startupControlKey == Key<IStartupControlModel>.Empty ||
				startupControl is null)
			{
				_startupControlState = _startupControlState with
				{
					ActiveStartupControlKey = Key<IStartupControlModel>.Empty
				};
            }
            else
            {
    			_startupControlState = _startupControlState with
    			{
    				ActiveStartupControlKey = startupControl.Key
    			};
			}
        }

        StartupControlStateChanged?.Invoke();
    }
	
	public void StateChanged()
	{
		StartupControlStateChanged?.Invoke();
	}
}
