using Walk.Common.RazorLib.Reactives.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    /// <summary>
    /// To avoid unexpected HTML movements when responding to a AppDimensionStateChanged
    /// this debounce will add 1 extra event after everything has "settled".
    ///
    /// `byte` is just a throwaway generic type, it isn't used.
    /// </summary>
    private readonly Debounce<byte> _debounceExtraEvent;
    
    private AppDimensionState _appDimensionState;
    
    public AppDimensionState GetAppDimensionState() => _appDimensionState;
    
    public void SetAppDimensions_Silent_NoEventRaised(AppDimensionState appDimensionState)
    {
        _appDimensionState = appDimensionState;
    }

    public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true)
    {
        CommonUiStateChanged?.Invoke(CommonUiEventKind.Intra_AppDimensionStateChanged);
        
        if (useExtraEvent)
            _debounceExtraEvent.Run(default);
    }

    public void AppDimension_NotifyUserAgentResize(AppDimensionState appDimensionState, bool useExtraEvent = true)
    {
        _appDimensionState = appDimensionState;
        CommonUiStateChanged?.Invoke(CommonUiEventKind.UserAgent_AppDimensionStateChanged);
        
        if (useExtraEvent)
            _debounceExtraEvent.Run(default);
    }
}
