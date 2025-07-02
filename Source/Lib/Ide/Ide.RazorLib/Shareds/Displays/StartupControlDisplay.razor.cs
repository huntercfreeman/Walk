using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib.Shareds.Displays;

public partial class StartupControlDisplay : ComponentBase, IDisposable
{
    [Inject]
    private ITerminalGroupService TerminalGroupService { get; set; } = null!;
    [Inject]
    private ITerminalService TerminalService { get; set; } = null!;
    [Inject]
    private IIdeService IdeService { get; set; } = null!;
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

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
    		
    		IdeService.SetActiveStartupControlKey(startupControlKey);
    	}
    }
    	
    protected override void OnInitialized()
    {
    	TerminalService.TerminalStateChanged += Shared_OnStateChanged;
    	IdeService.StartupControlStateChanged += Shared_OnStateChanged;
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
			    onClickFunc: async () => 
				{
					var success = await TrySetFocus(ContextFacts.OutputContext).ConfigureAwait(false);
	
	                if (!success)
	                {
	                    CommonUtilityService.SetPanelTabAsActiveByContextRecordKey(
	                        ContextFacts.OutputContext.ContextKey);
	
	                    _ = await TrySetFocus(ContextFacts.OutputContext).ConfigureAwait(false);
	                }
				}));
			    
			menuOptionList.Add(new MenuOptionRecord(
				"View Terminal",
			    MenuOptionKind.Other,
			    onClickFunc: async () => 
				{
					TerminalGroupService.SetActiveTerminal(TerminalFacts.EXECUTION_KEY);
				
					var success = await TrySetFocus(ContextFacts.TerminalContext).ConfigureAwait(false);
	
	                if (!success)
	                {
	                    CommonUtilityService.SetPanelTabAsActiveByContextRecordKey(
	                        ContextFacts.TerminalContext.ContextKey);
	
	                    _ = await TrySetFocus(ContextFacts.TerminalContext).ConfigureAwait(false);
	                }
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
    			CommonUtilityService,
    			CommonUtilityService.JsRuntimeCommonApi,
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
	
	private async Task<bool> TrySetFocus(ContextRecord contextRecord)
    {
        return await CommonUtilityService.JsRuntimeCommonApi
            .TryFocusHtmlElementById(contextRecord.ContextElementId)
            .ConfigureAwait(false);
    }
    
    private async void Shared_OnStateChanged() => await InvokeAsync(StateHasChanged);
    
    public void Dispose()
    {
    	TerminalService.TerminalStateChanged -= Shared_OnStateChanged;
    	IdeService.StartupControlStateChanged -= Shared_OnStateChanged;
    }
}