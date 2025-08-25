using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private StartupControlState _startupControlState = new();

    public StartupControlState GetIdeStartupControlState() => _startupControlState;

    public void Ide_ClearAllStartupControls()
    {
        lock (_stateModificationLock)
        {
            _startupControlState = _startupControlState with
            {
                StartupControlList = new List<StartupControlModel>()
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
    
    public void Ide_RegisterStartupControl(StartupControlModel startupControl)
    {
        lock (_stateModificationLock)
        {
            var indexOfStartupControl = _startupControlState.StartupControlList.FindIndex(
                x => x.StartupProjectAbsolutePath.Value == startupControl.StartupProjectAbsolutePath.Value);

            if (indexOfStartupControl == -1 && !string.IsNullOrWhiteSpace(startupControl.StartupProjectAbsolutePath.Value))
            {
                var outStartupControlList = new List<StartupControlModel>(_startupControlState.StartupControlList);
                outStartupControlList.Add(startupControl);

                _startupControlState = _startupControlState with
                {
                    StartupControlList = outStartupControlList
                };
            }
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
    
    public void Ide_SetStartupControlList(List<StartupControlModel> startupControlList)
    {
        lock (_stateModificationLock)
        {
            _startupControlState = _startupControlState with
            {
                StartupControlList = startupControlList
            };
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

                var outStartupControlList = new List<StartupControlModel>(_startupControlState.StartupControlList);
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
                startupControl.StartupProjectAbsolutePath.Value is null)
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
    
    public void Ide_TriggerStartupControlStateChanged(TerminalCommandRequest? executingTerminalCommandRequest)
    {
        lock (_stateModificationLock)
        {
            _startupControlState = _startupControlState with
            {
                ExecutingTerminalCommandRequest = executingTerminalCommandRequest
            };
        }
        IdeStateChanged?.Invoke(IdeStateChangedKind.Ide_StartupControlStateChanged);
    }
}
