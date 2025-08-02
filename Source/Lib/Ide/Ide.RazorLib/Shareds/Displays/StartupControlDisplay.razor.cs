using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib.Shareds.Displays;

public partial class StartupControlDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    private const string _startButtonElementId = "di_ide_startup-controls-display_id";

    private ElementReference? _startButtonElementReference;
    private Key<DropdownRecord> _startButtonDropdownKey = Key<DropdownRecord>.NewKey();
    
    public string? SelectedStartupControlGuidString
    {
        get => IdeService.GetIdeStartupControlState().ActiveStartupControlKey.Guid.ToString();
        set
        {
            Key<IStartupControlModel> startupControlKey = Key<IStartupControlModel>.Empty;
            
            if (value is not null &&
                Guid.TryParse(value, out var guid))
            {
                startupControlKey = new Key<IStartupControlModel>(guid);
            }
            
            IdeService.Ide_SetActiveStartupControlKey(startupControlKey);
        }
    }
        
    protected override void OnInitialized()
    {
        IdeService.IdeStateChanged += Shared_OnStateChanged;
    }

    private async Task StartProgramWithoutDebuggingOnClick(bool isExecuting)
    {
        var localStartupControlState = IdeService.GetIdeStartupControlState();
        var activeStartupControl = localStartupControlState.StartupControlList.FirstOrDefault(
            x => x.Key == localStartupControlState.ActiveStartupControlKey);
        
        if (activeStartupControl is null)
            return;
    
        if (isExecuting)
        {
            var menuOptionList = new List<MenuOptionRecord>();
            
            menuOptionList.Add(new MenuOptionRecord(
                "View Output",
                MenuOptionKind.Other,
                onClickFunc: () => 
                {
                    var panelState = IdeService.CommonService.GetPanelState();
                    var outputPanel = panelState.PanelList.FirstOrDefault(x => x.Title == "Output");
                    IdeService.CommonService.ShowOrAddPanelTab(outputPanel);
                    return Task.CompletedTask;
                }));
                
            menuOptionList.Add(new MenuOptionRecord(
                "View Terminal",
                MenuOptionKind.Other,
                onClickFunc: () => 
                {
                    IdeService.TerminalGroup_SetActiveTerminal(IdeFacts.EXECUTION_KEY);
                    var panelState = IdeService.CommonService.GetPanelState();
                    var outputPanel = panelState.PanelList.FirstOrDefault(x => x.Title == "Terminal");
                    IdeService.CommonService.ShowOrAddPanelTab(outputPanel);
                    return Task.CompletedTask;
                }));
                
            menuOptionList.Add(new MenuOptionRecord(
                "Stop Execution",
                MenuOptionKind.Other,
                onClickFunc: () =>
                {
                    var localStartupControlState = IdeService.GetIdeStartupControlState();
                    var activeStartupControl = localStartupControlState.StartupControlList.FirstOrDefault(
                        x => x.Key == localStartupControlState.ActiveStartupControlKey);
                    
                    if (activeStartupControl is null)
                        return Task.CompletedTask;
                        
                    return activeStartupControl.StopButtonOnClickTask
                        .Invoke(activeStartupControl);
                }));
                
            await DropdownHelper.RenderDropdownAsync(
                IdeService.CommonService,
                IdeService.CommonService.JsRuntimeCommonApi,
                _startButtonElementId,
                DropdownOrientation.Bottom,
                _startButtonDropdownKey,
                new MenuRecord(menuOptionList),
                _startButtonElementReference);
        }
        else
        {
            await activeStartupControl.StartButtonOnClickTask
                .Invoke(activeStartupControl)
                .ConfigureAwait(false);
        }
    }
    
    private async void Shared_OnStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.TerminalStateChanged ||
            ideStateChangedKind == IdeStateChangedKind.Ide_StartupControlStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        IdeService.IdeStateChanged -= Shared_OnStateChanged;
    }
}
