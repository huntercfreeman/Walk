using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Contexts.Models;

namespace Walk.Common.RazorLib.Contexts.Displays;

public partial class ContextInitializerDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IContextService ContextService { get; set; } = null!;
    
    protected override void OnInitialized()
    {
    	ContextService.ContextStateChanged += OnContextStateChanged;
    	base.OnInitialized();
    }
    
    private async void OnContextStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	ContextService.ContextStateChanged -= OnContextStateChanged;
    }
}
