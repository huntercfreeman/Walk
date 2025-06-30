using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Ide.RazorLib.Shareds.Models;

public class IdeService : IIdeService
{
    private readonly object _stateModificationLock = new();
    
    private IdeState _ideState = new();
    private StartupControlState _startupControlState = new();
    
    public event Action? IdeStateChanged;
    public event Action? StartupControlStateChanged;
    
    public IdeState GetIdeState() => _ideState;
    public StartupControlState GetIdeStartupControlState() => _startupControlState;
    
    public void RegisterFooterBadge(IBadgeModel badgeModel)
	{
		lock (_stateModificationLock)
		{
			var existingComponent = _ideState.FooterBadgeList.FirstOrDefault(x =>
				x.Key == badgeModel.Key);

			if (existingComponent is null)
            {
    			var outFooterBadgeList = new List<IBadgeModel>(_ideState.FooterBadgeList);
    			outFooterBadgeList.Add(badgeModel);
    
    			_ideState = _ideState with
    			{
    				FooterBadgeList = outFooterBadgeList
    			};
    	    }
		}

        IdeStateChanged?.Invoke();
    }
    
    public void SetMenuFile(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menu
			};
		}
    }
	
	public void SetMenuTools(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menu
			};
        }
    }
	
	public void SetMenuView(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menu
			};
        }
    }
	
	public void SetMenuRun(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menu
			};
        }
    }
	
	public void ModifyMenuFile(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menuFunc.Invoke(_ideState.MenuFile)
			};
        }
    }
	
	public void ModifyMenuTools(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menuFunc.Invoke(_ideState.MenuTools)
			};
        }
    }

	public void ModifyMenuView(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menuFunc.Invoke(_ideState.MenuView)
			};
        }
    }
	
	public void ModifyMenuRun(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menuFunc.Invoke(_ideState.MenuRun)
			};
        }
    }

	public void RegisterStartupControl(IStartupControlModel startupControl)
	{
		lock (_stateModificationLock)
		{
			var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
				x => x.Key == startupControl.Key);

			if (indexOfStartupControl == -1)
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
	
	public void TriggerStartupControlStateChanged()
	{
		StartupControlStateChanged?.Invoke();
	}
}
