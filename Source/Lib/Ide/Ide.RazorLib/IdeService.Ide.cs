using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private IdeState _ideState = new();
    private StartupControlState _startupControlState = new();

    public IdeState GetIdeState() => _ideState;
    public StartupControlState GetIdeStartupControlState() => _startupControlState;

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

    public void Ide_ClearAllStartupControls()
    {
        lock (_stateModificationLock)
        {
            _startupControlState = _startupControlState with
            {
                StartupControlList = new List<IStartupControlModel>()
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
    
    public void Ide_RegisterStartupControl(IStartupControlModel startupControl)
    {
        lock (_stateModificationLock)
        {
            var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
                x => x.StartupProjectAbsolutePath.Value == startupControl.StartupProjectAbsolutePath.Value);

            if (indexOfStartupControl == -1 && !string.IsNullOrWhiteSpace(startupControl.StartupProjectAbsolutePath.Value))
            {
                var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
                outStartupControlList.Add(startupControl);

                _startupControlState = _startupControlState with
                {
                    StartupControlList = outStartupControlList
                };
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }

    public void Ide_DisposeStartupControl(string startupProjectAbsolutePathValue)
    {
        lock (_stateModificationLock)
        {
            var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
                x => x.StartupProjectAbsolutePath.Value == startupProjectAbsolutePathValue);

            if (indexOfStartupControl != -1)
            {
                var outActiveStartupProjectAbsolutePathValue = _startupControlState.ActiveStartupProjectAbsolutePathValue;
                if (_startupControlState.ActiveStartupProjectAbsolutePathValue == startupProjectAbsolutePathValue)
                    outActiveStartupProjectAbsolutePathValue = string.Empty;

                var outStartupControlList = new List<IStartupControlModel>(_startupControlState.StartupControlList);
                outStartupControlList.RemoveAt(indexOfStartupControl);

                _startupControlState = _startupControlState with
                {
                    StartupControlList = outStartupControlList,
                    ActiveStartupProjectAbsolutePathValue = outActiveStartupProjectAbsolutePathValue
                };
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }

    public void Ide_SetActiveStartupControlKey(string startupProjectAbsolutePathValue)
    {
        lock (_stateModificationLock)
        {
            var startupControl = _startupControlState.StartupControlList.FirstOrDefault(
                x => x.StartupProjectAbsolutePath.Value == startupProjectAbsolutePathValue);

            if (startupProjectAbsolutePathValue == string.Empty ||
                startupControl is null)
            {
                _startupControlState = _startupControlState with
                {
                    ActiveStartupProjectAbsolutePathValue = string.Empty
                };
            }
            else
            {
                _startupControlState = _startupControlState with
                {
                    ActiveStartupProjectAbsolutePathValue = startupControl.StartupProjectAbsolutePath.Value
                };
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
    
    public void Ide_TriggerStartupControlStateChanged()
    {
        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
}
