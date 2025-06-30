using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Menus.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public class IdeService : IIdeService
{
    private readonly object _stateModificationLock = new();
    
    private IdeState _ideState = new();
    
    public event Action? IdeStateChanged;
    
    public IdeState GetIdeState() => _ideState;
    
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
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

        IdeStateChanged?.Invoke();
    }
}
