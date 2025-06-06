using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Dialogs.Displays;

public partial class DialogInitializer : ComponentBase, IDisposable
{
	[Inject]
	private IDialogService DialogService { get; set; } = null!;
	
    private ContextBoundary? _dialogContextBoundary;
    
    protected override void OnInitialized()
    {
    	DialogService.DialogStateChanged += OnDialogStateChanged;
    	base.OnInitialized();
    }
    
    private Task HandleOnFocusIn(IDialog dialog)
    {
    	var localDialogContextBoundary = _dialogContextBoundary;
    	
    	if (localDialogContextBoundary is not null)
	    	localDialogContextBoundary.HandleOnFocusIn();
    
    	return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(IDialog dialog)
    {
    	return Task.CompletedTask;
    }
    
    private async void OnDialogStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	DialogService.DialogStateChanged -= OnDialogStateChanged;
    }
}