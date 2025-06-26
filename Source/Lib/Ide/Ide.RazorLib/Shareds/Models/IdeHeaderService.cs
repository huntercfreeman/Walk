using Walk.Common.RazorLib.Menus.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public class IdeHeaderService : IIdeHeaderService
{
    private readonly object _stateModificationLock = new();

    private IdeHeaderState _ideHeaderState = new();
	
	public event Action? IdeHeaderStateChanged;
	
	public IdeHeaderState GetIdeHeaderState() => _ideHeaderState;

	public void SetMenuFile(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuFile = menu
			};
		}

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void SetMenuTools(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuTools = menu
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void SetMenuView(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuView = menu
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void SetMenuRun(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuRun = menu
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void ModifyMenuFile(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuFile = menuFunc.Invoke(_ideHeaderState.MenuFile)
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void ModifyMenuTools(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuTools = menuFunc.Invoke(_ideHeaderState.MenuTools)
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }

	public void ModifyMenuView(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuView = menuFunc.Invoke(_ideHeaderState.MenuView)
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
	
	public void ModifyMenuRun(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideHeaderState = _ideHeaderState with
			{
				MenuRun = menuFunc.Invoke(_ideHeaderState.MenuRun)
			};
        }

        IdeHeaderStateChanged?.Invoke();
    }
}
