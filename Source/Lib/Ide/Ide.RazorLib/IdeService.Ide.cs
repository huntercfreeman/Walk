using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
	private IdeState _ideState = new();
	private StartupControlState _startupControlState = new();

	public event Action? Ide_IdeStateChanged;
	public event Action? Ide_StartupControlStateChanged;

	public IdeState GetIdeState() => _ideState;
	public StartupControlState GetIdeStartupControlState() => _startupControlState;

	public void Ide_RegisterFooterBadge(IBadgeModel badgeModel)
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

		Ide_IdeStateChanged?.Invoke();
	}

	public void Ide_SetMenuFile(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menu
			};
		}
	}

	public void Ide_SetMenuTools(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menu
			};
		}
	}

	public void Ide_SetMenuView(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menu
			};
		}
	}

	public void Ide_SetMenuRun(MenuRecord menu)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menu
			};
		}
	}

	public void Ide_ModifyMenuFile(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuFile = menuFunc.Invoke(_ideState.MenuFile)
			};
		}
	}

	public void Ide_ModifyMenuTools(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuTools = menuFunc.Invoke(_ideState.MenuTools)
			};
		}
	}

	public void Ide_ModifyMenuView(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuView = menuFunc.Invoke(_ideState.MenuView)
			};
		}
	}

	public void Ide_ModifyMenuRun(Func<MenuRecord, MenuRecord> menuFunc)
	{
		lock (_stateModificationLock)
		{
			_ideState = _ideState with
			{
				MenuRun = menuFunc.Invoke(_ideState.MenuRun)
			};
		}
	}

	public void Ide_RegisterStartupControl(IStartupControlModel startupControl)
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

		Ide_StartupControlStateChanged?.Invoke();
	}

	public void Ide_DisposeStartupControl(Key<IStartupControlModel> startupControlKey)
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

		Ide_StartupControlStateChanged?.Invoke();
	}

	public void Ide_SetActiveStartupControlKey(Key<IStartupControlModel> startupControlKey)
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

		Ide_StartupControlStateChanged?.Invoke();
	}

	public void Ide_TriggerStartupControlStateChanged()
	{
		Ide_StartupControlStateChanged?.Invoke();
	}
}
